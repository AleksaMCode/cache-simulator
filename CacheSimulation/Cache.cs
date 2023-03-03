using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace CacheSimulation
{
    public enum WritePolicy
    {
        WriteThrough = 0,
        /// <summary>
        /// Write-back also called write-behind
        /// </summary>
        WriteBack = 1,
        /// <summary>
        /// Write allocate also called fetch on write
        /// </summary>
        WriteAllocate = 2,
        /// <summary>
        /// No-write allocate also called write-no-allocate or write around
        /// </summary>
        WriteAround = 3
    }

    public enum ReplacementPolicy
    {
        LeastRecentlyUsed = 0,
        Belady = 1,
        FirstInFirstOut = 2,
        LastInFirstOut = 3,
        TimeAwareLeastRecentlyUsed = 4,
        MostRecentlyUsed = 5,
        RandomReplacement = 6,
        LeastFrequentlyUsed = 7,
        LeastFrequentlyUsedWithDynamicAging = 8
    }

    public abstract class Cache
    {
        public List<CacheEntry> CacheEntries;

        private static readonly ReaderWriterLockSlim readWriteLock = new ReaderWriterLockSlim();

        /// <summary>
        /// Data used for statistics.
        /// </summary>
        public StatisticsData StatisticsInfo { get; set; } = new StatisticsData();
        public int NumberOfLines { get; set; } = 0;
        public int Size { get; set; } = 0;
        public int Associativity { get; set; } = 0;
        public int BlockOffsetLength { get; set; } = 0;
        public int SetIndexLength { get; set; } = 0;
        public CacheConfiguration CacheConfig { get; set; }

        public int NumberOfSets;
        public int SetSize;

        public string RamFileName { get; set; }
        public string TraceFileName { get; set; }

        public virtual void CreateCache()
        {
            if (CacheConfig.BlockSize >= Size)
            {
                throw new Exception($"Size of the cache line ({CacheConfig.BlockSize} B) can't be larger than the total cache size ({Size} B).");
            }

            // Explanation for this check implementation https://stackoverflow.com/questions/2751593/how-to-determine-if-a-decimal-double-is-an-integer .
            if (!CheckNumberForPowerOfTwo(CacheConfig.BlockSize))
            {
                throw new Exception("Block size is not a power of 2.");
            }
            else if (!CheckNumberForPowerOfTwo(Associativity))
            {
                throw new Exception("Associativity is not a power of 2.");
            }

            NumberOfLines = Size / CacheConfig.BlockSize;

            if (Associativity > NumberOfLines)
            {
                throw new Exception($"The cache with {NumberOfLines}-lines can't be {Associativity}-way set-associative.");
            }

            SetSize = Associativity;

            NumberOfSets = Size / (SetSize * CacheConfig.BlockSize);
            BlockOffsetLength = (int)Math.Ceiling(Math.Log(CacheConfig.BlockSize, 2));
            SetIndexLength = (int)Math.Ceiling(Math.Log(Size / (SetSize * CacheConfig.BlockSize), 2));


            if (CacheConfig.ReplacementPolicy == ReplacementPolicy.FirstInFirstOut)
            {
                fifoIndexQueue = new List<int>();
            }

            CreateColdCache();
        }

        public static bool CheckNumberForPowerOfTwo(int number)
        {
            var logValue = Math.Log(number, 2);

            return Math.Ceiling(logValue) == Math.Floor(logValue);
        }

        public Instruction TraceLineParser(string line)
        {
            char[] charsToTrim = { ' ', '0' };
            var splitLine = line.Split(',').Select(x => x.TrimStart('\t')).ToArray();

            if (line[0] == 'L')
            {
                return Int32.TryParse(splitLine[1].Trim(), out var size)
                    ? new Instruction(MemoryRelatedInstructions.Load, splitLine[0].Split('\t')[1].Trim(' ').Substring(2).TrimStart(charsToTrim), size) :
                    null;
            }
            else if (line[0] == 'S')
            {
                return Int32.TryParse(splitLine[1].Trim(), out var size)
                    ? new Instruction(MemoryRelatedInstructions.Store, splitLine[0].Split('\t')[1].Substring(2).TrimStart(charsToTrim), size, splitLine[2].Trim(' ').Substring(2).TrimStart(charsToTrim))
                    : null;
            }
            else
            {
                throw new Exception("Unknown instruction used in trace file.");
            }
        }

        private void CreateColdCache()
        {
            CacheEntries = new List<CacheEntry>();
            for (var i = 0; i < NumberOfLines; ++i)
            {
                CacheEntries.Add(new CacheEntry
                {
                    Set = i / Associativity
                });
            }
        }

        public int GetTagLength(string address)
        {
            return address.Length - SetIndexLength - BlockOffsetLength;
        }

        [Obsolete("This method is no longer necessary, because we calculate lengths using ceiling and log2 in class constructor.", true)]
        /// <summary>
        /// Sets the length of set index and block offset.
        /// </summary>
        public void SetLengths()
        {
            var i = 1;

            for (; i < CacheConfig.BlockSize; i *= 2)
            {
                ++BlockOffsetLength;
            }

            if (i != CacheConfig.BlockSize && BlockOffsetLength != 0)
            {
                throw new Exception("Block size is not a power of 2.");
            }

            i = 1;
            for (; i < NumberOfLines / Associativity; i *= 2)
            {
                ++SetIndexLength;
            }

            if (i != NumberOfLines / Associativity && SetIndexLength != 0)
            {
                throw new Exception("Associativity is not a power of 2.");
            }
        }

        /// <summary>
        /// Converts an address from hex format to binary format.
        /// </summary>
        /// <param name="address">Address in hex format.</param>
        /// <returns>Address in binary format.</returns>
        public string GetBinaryAddress(string address)
        {
            return string.Join(string.Empty,
                address.Select(c => Convert.ToString(Convert.ToInt32(c.ToString(), 16), 2).PadLeft(4, '0')));
        }

        /// <summary>
        /// Gets set index from binary address.
        /// </summary>
        /// <param name="binaryAddress">Address in binary format.</param>
        /// <param name="tagLength">Length of the tag.</param>
        /// <returns>Set index.</returns>
        public int GetIndex(string binaryAddress, int tagLength)
        {
            var index = 0;
            var exp = 1;
            for (var i = binaryAddress.Length - 1 - BlockOffsetLength; i >= tagLength; --i)
            {
                if (binaryAddress[i] == '1')
                {
                    index += exp;
                }
                exp <<= 1;
            }

            return index;
        }

        /// <summary>
        /// Updates the most recently used set, allowing the LRU algorithm to work.
        /// </summary>
        /// <param name="newestEntryIndex">Index of the newest entry in the cache.</param>
        /// <param name="index"></param>
        public void Aging(int newestEntryIndex, int index)
        {
            var limit = index * Associativity;
            var tmpAge = CacheEntries[newestEntryIndex].Age;

            for (var i = limit; i < limit + Associativity; ++i)
            {
                ++CacheEntries[i].Age;
            }

            CacheEntries[newestEntryIndex].Age = tmpAge;
        }

        private byte[] ConversionBugFixer(byte[] binaryAddress)
        {
            var zeroArray = new byte[4 - binaryAddress.Length];

            for (var i = 0; i < zeroArray.Length; ++i)
            {
                zeroArray[i] = 0;
            }

            var tmpBinAddr = new byte[4];
            Buffer.BlockCopy(zeroArray, 0, tmpBinAddr, 0, zeroArray.Length);
            Buffer.BlockCopy(binaryAddress, 0, tmpBinAddr, zeroArray.Length, binaryAddress.Length);

            return tmpBinAddr;
        }

        private byte[] ReadFromRam(string address, int size)
        {
            readWriteLock.EnterWriteLock();
            var hasExceptionHappened = true;
            var buffer = new byte[size];

            try
            {
                using var stream = File.Open(RamFileName, FileMode.Open);
                var bAddress = GetBytesFromString(address);

                if (bAddress.Length != 4)
                {
                    bAddress = ConversionBugFixer(bAddress);
                }

                if (BitConverter.IsLittleEndian)
                {
                    Array.Reverse(bAddress);
                }

                var offset = BitConverter.ToInt32(bAddress, 0);

                stream.Seek(offset, SeekOrigin.Begin);
                stream.Read(buffer, 0, size);

                ++StatisticsInfo.MemoryReads;
                hasExceptionHappened = false;
            }
            finally
            {
                readWriteLock.ExitWriteLock();

                if (hasExceptionHappened)
                {
                    throw new Exception();
                }
            }

            return buffer;
        }

        private void WriteToRam(string address, byte[] data, int size)
        {
            readWriteLock.EnterWriteLock();
            var hasExceptionHappened = true;

            try
            {
                using var stream = File.Open(RamFileName, FileMode.Open);
                var bAddress = GetBytesFromString(address);

                if (bAddress.Length != 4)
                {
                    bAddress = ConversionBugFixer(bAddress);
                }

                if (BitConverter.IsLittleEndian)
                {
                    Array.Reverse(bAddress);
                }

                var offset = BitConverter.ToInt32(bAddress, 0);

                stream.Seek(offset, SeekOrigin.Begin);
                stream.Write(data, 0, size);

                ++StatisticsInfo.MemoryWrites;
                hasExceptionHappened = false;
            }
            finally
            {
                readWriteLock.ExitWriteLock();

                if (hasExceptionHappened)
                {
                    throw new Exception();
                }
            }
        }

        public bool WriteToCache(string address, int size, string data, out string additionalData, int traceIndex, int coreNumber)
        {
            additionalData = "";
            var sb = new StringBuilder();

            // Check if address exists in the cache first.
            byte[] buffer;
            var binaryAddress = GetBinaryAddress(address);
            var index = GetIndex(binaryAddress, GetTagLength(binaryAddress)) * Associativity;
            var replacementIndex = index;

            for (var i = index; i < index + Associativity; ++i)
            {
                if (CacheEntries[i].FlagBits.Valid == Validity.Valid)
                {
                    if (CacheEntries[i].Tag == binaryAddress/*.Substring(0, CacheEntries[i].TagLength)*/)
                    {
                        ++StatisticsInfo.CacheHits;
                        CacheEntries[i].TagLength = GetTagLength(binaryAddress);

                        // Write data to cache.
                        buffer = GetBytesFromString(data);
                        if (buffer.Length > size)
                        {
                            CacheEntries[i].DataBlock = new byte[size];
                            Buffer.BlockCopy(buffer, 0, CacheEntries[i].DataBlock, 0, size);
                        }
                        else
                        {
                            CacheEntries[i].DataBlock = buffer;
                        }

                        if (CacheConfig.WriteHitPolicy == WritePolicy.WriteBack)
                        {
                            CacheEntries[i].FlagBits.Dirty = true;
                        }
                        else if (CacheConfig.WriteHitPolicy == WritePolicy.WriteThrough)
                        {
                            try
                            {
                                WriteToRam(address, buffer, size);
                            }
                            catch (Exception)
                            {
                                sb.Append($"\n[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] core={coreNumber} error=WRITE_TO_RAM_FAIL");
                            }
                        }

                        if (CacheConfig.ReplacementPolicy is ReplacementPolicy.LeastRecentlyUsed
                            or ReplacementPolicy.MostRecentlyUsed)
                        {
                            // Set age values.
                            Aging(i, CacheEntries[i].Set);
                        }

                        return true;
                    }
                }
            }

            ++StatisticsInfo.CacheMisses;

            if (CacheConfig.WriteMissPolicy == WritePolicy.WriteAround)
            {
                try
                {
                    buffer = GetBytesFromString(data);
                    WriteToRam(address, buffer, size);
                }
                catch (Exception)
                {
                    sb.Append($"\n[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] core={coreNumber} error=WRITE_TO_RAM_FAIL");
                }

                return false;
            }

            // After a cache miss look for available entry structure.
            index = GetIndex(binaryAddress, GetTagLength(binaryAddress)) * Associativity;

            for (var i = index; i < index + Associativity; ++i)
            {
                if (CacheEntries[i].FlagBits.Valid == Validity.Invalid)
                {
                    CacheEntries[i].FlagBits.Valid = Validity.Valid;
                    CacheEntries[i].TagLength = GetTagLength(binaryAddress);
                    CacheEntries[i].Tag = binaryAddress;

                    // Write data to cache.
                    buffer = GetBytesFromString(data);
                    if (buffer.Length > size)
                    {
                        CacheEntries[i].DataBlock = new byte[size];
                        Buffer.BlockCopy(buffer, 0, CacheEntries[i].DataBlock, 0, size);
                    }
                    else
                    {
                        CacheEntries[i].DataBlock = buffer;
                    }

                    if (CacheConfig.WriteHitPolicy == WritePolicy.WriteBack)
                    {
                        CacheEntries[i].FlagBits.Dirty = true;
                    }

                    if (CacheConfig.ReplacementPolicy is ReplacementPolicy.LeastRecentlyUsed or ReplacementPolicy.MostRecentlyUsed)
                    {
                        // Set age values.
                        Aging(i, CacheEntries[i].Set);
                    }
                    else if (CacheConfig.ReplacementPolicy == ReplacementPolicy.FirstInFirstOut)
                    {
                        fifoIndexQueue.Add(i);
                    }

                    return false;
                }
            }

            ++StatisticsInfo.CacheEviction;
            replacementIndex = GetReplacementIndex(GetIndex(binaryAddress, GetTagLength(binaryAddress)) * Associativity, traceIndex);

            // If the write policy is write-back and the dirty flag is set, write the cache entry to RAM first.
            if (CacheConfig.WriteHitPolicy == WritePolicy.WriteBack && CacheEntries[replacementIndex].FlagBits.Dirty)
            {
                try
                {
                    sb.Append($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] core={coreNumber} operation=EVICTION cache_entry_tag={CacheEntries[replacementIndex].Tag}b");
                    // Write data from cache entry to RAM because the dirty flag has been set.
                    WriteToRam(address, CacheEntries[replacementIndex].DataBlock, CacheEntries[replacementIndex].DataBlock.Length);
                }
                catch (Exception)
                {
                    sb.Append($"\n[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] core={coreNumber} error=WRITE_TO_RAM_FAIL");
                }
            }

            // Else just replace data in cache with new data.
            CacheEntries[replacementIndex].TagLength = GetTagLength(binaryAddress);
            CacheEntries[replacementIndex].Tag = binaryAddress;

            // Write data to cache.
            buffer = GetBytesFromString(data);
            if (buffer.Length > size)
            {
                CacheEntries[replacementIndex].DataBlock = new byte[size];
                Buffer.BlockCopy(buffer, 0, CacheEntries[replacementIndex].DataBlock, 0, size);
            }
            else
            {
                CacheEntries[replacementIndex].DataBlock = buffer;
            }

            if (CacheConfig.WriteHitPolicy == WritePolicy.WriteBack)
            {
                CacheEntries[replacementIndex].FlagBits.Dirty = true;
            }

            if (CacheConfig.ReplacementPolicy is ReplacementPolicy.LeastRecentlyUsed or ReplacementPolicy.MostRecentlyUsed)
            {
                // Set age values.
                Aging(replacementIndex, CacheEntries[replacementIndex].Set);
            }

            additionalData = sb.ToString();
            return false;
        }

        /// <summary>
        /// Converts hex number in string to byte array.
        /// </summary>
        /// <param name="hexNumber">Hex number stored in string.</param>
        /// <returns>Converted hex number in byte format.</returns>
        private byte[] GetBytesFromString(string hexNumber)
        {
            // Adding a leading zero to avoid conversion errors.
            if ((hexNumber.Length % 2) != 0)
            {
                hexNumber = 0 + hexNumber;
            }

            var output = new byte[hexNumber.Length / 2];

            for (var i = 0; i < hexNumber.Length; i += 2)
            {
                output[i / 2] = Convert.ToByte(hexNumber.Substring(i, 2), 16);
            }

            return output;
        }

        protected string ConvertBinaryToHex(string binaryNumber)
        {
            if ((binaryNumber.Length % 8) != 0)
            {
                binaryNumber = "0000" + binaryNumber;
            }

            return string.Join(string.Empty,
            Enumerable.Range(0, binaryNumber.Length / 8)
            .Select(i => Convert.ToByte(binaryNumber.Substring(i * 8, 8), 2).ToString("x2"))).TrimStart('0');
        }

        public bool ReadFromCache(string address, int size, out string additionalData, int traceIndex, int coreNumber)
        {
            additionalData = "";
            var sb = new StringBuilder();

            // Check if address exists in the cache first.
            var binaryAddress = GetBinaryAddress(address);
            var index = GetIndex(binaryAddress, GetTagLength(binaryAddress)) * Associativity;
            var replacementIndex = index;

            for (var i = index; i < index + Associativity; ++i)
            {
                if (CacheEntries[i].FlagBits.Valid == Validity.Valid)
                {
                    if (CacheEntries[i].Tag == binaryAddress/*.Substring(0, CacheEntries[i].TagLength)*/)
                    {
                        ++StatisticsInfo.CacheHits;

                        if (CacheConfig.ReplacementPolicy is ReplacementPolicy.LeastRecentlyUsed or ReplacementPolicy.MostRecentlyUsed)
                        {
                            // Set age values.
                            Aging(i, CacheEntries[i].Set);
                        }

                        return true;
                    }
                }
            }

            // After a cache miss look for available entry structure.
            ++StatisticsInfo.CacheMisses;
            index = GetIndex(binaryAddress, GetTagLength(binaryAddress)) * Associativity;

            for (var i = index; i < index + Associativity; ++i)
            {
                if (CacheEntries[i].FlagBits.Valid == Validity.Invalid)
                {
                    CacheEntries[i].FlagBits.Valid = Validity.Valid;
                    CacheEntries[i].TagLength = GetTagLength(binaryAddress);
                    CacheEntries[i].Tag = binaryAddress;

                    try
                    {
                        // Read the data from the RAM.
                        CacheEntries[i].DataBlock = ReadFromRam(address, size);
                    }
                    catch (Exception)
                    {
                        sb.Append($"\n[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] core={coreNumber} error=READ_FROM_RAM_FAIL");
                    }

                    if (CacheConfig.ReplacementPolicy is ReplacementPolicy.LeastRecentlyUsed or ReplacementPolicy.MostRecentlyUsed)
                    {
                        // Set age values.
                        Aging(i, CacheEntries[i].Set);
                    }
                    else if (CacheConfig.ReplacementPolicy == ReplacementPolicy.FirstInFirstOut)
                    {
                        fifoIndexQueue.Add(i);
                    }

                    return false;
                }
            }

            ++StatisticsInfo.CacheEviction;
            replacementIndex = GetReplacementIndex(GetIndex(binaryAddress, GetTagLength(binaryAddress)) * Associativity, traceIndex);

            // If the write policy is write-back and the dirty flag is set, write the cache entry to RAM first.
            if (CacheConfig.WriteHitPolicy == WritePolicy.WriteBack && CacheEntries[replacementIndex].FlagBits.Dirty)
            {
                try
                {
                    // Write data from cache entry to RAM because the dirty flag has been set.
                    sb.Append($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] core={coreNumber} operation=EVICTION cache_entry_tag={CacheEntries[replacementIndex].Tag}b");
                    WriteToRam(address, CacheEntries[replacementIndex].DataBlock, CacheEntries[replacementIndex].DataBlock.Length);
                }
                catch (Exception)
                {
                    sb.Append($"\n[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] core={coreNumber} error=WRITE_TO_RAM_FAIL");
                }
            }

            CacheEntries[replacementIndex].TagLength = GetTagLength(binaryAddress);
            CacheEntries[replacementIndex].Tag = binaryAddress;

            if (CacheConfig.WriteHitPolicy == WritePolicy.WriteBack)
            {
                CacheEntries[replacementIndex].FlagBits.Dirty = false;
            }

            try
            {
                // Read the data from the RAM.
                CacheEntries[replacementIndex].DataBlock = ReadFromRam(address, size);
            }
            catch (Exception)
            {
                sb.Append($"\n[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] core={coreNumber} error=READ_FROM_RAM_FAIL");
            }

            // Set age values.
            if (CacheConfig.ReplacementPolicy is ReplacementPolicy.LeastRecentlyUsed or ReplacementPolicy.MostRecentlyUsed)
            {
                Aging(replacementIndex, CacheEntries[replacementIndex].Set);
            }

            additionalData = sb.ToString();
            return false;
        }

        protected abstract int GetReplacementIndex(int index, int traceIndex);
        //{
        //    var replacementIndex = index;

        //    else if (CacheConfig.ReplacementPolicy == ReplacementPolicy.LastInFirstOut)
        //    {
        //        replacementIndex = lifoIndex;
        //    }

        //    return replacementIndex;
        //}
    }
}
