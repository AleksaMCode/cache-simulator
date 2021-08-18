using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CacheSimulation
{
    public enum WritePolicy
    {
        WriteThrough = 0,
        WriteBack = 1,
        WriteAllocate = 2,
        WriteAround = 3
    }

    public enum ReplacementPolicy
    {
        LeastRecentlyUsed = 0,
        Belady = 1,
        FirstInFirstOut = 2,
        LastInFirstOut = 3
    }

    public class Cache
    {
        public List<CacheEntry> CacheEntries;

        /// <summary>
        /// Data used for statistics.
        /// </summary>
        public StatisticsData StatisticsInfo { get; set; } = new StatisticsData();
        public int NumberOfLines { get; set; } = 0;
        public int Size { get; set; } = 0;
        public int Associativity { get; set; } = 0;
        public int BlockOffsetLength { get; set; } = 0;
        public int SetIndexLength { get; set; } = 0;
        public CacheConfiguration CacheConfig { get; set; } = new CacheConfiguration();

        private string ramFileName { get; set; }
        private string traceFileName { get; set; }

        public Cache(string ramFileName, string traceFileName)
        {
            this.ramFileName = ramFileName;
            this.traceFileName = traceFileName;
        }

        public void CreateColdCache(int numberOfLines)
        {
            CacheEntries = new List<CacheEntry>(numberOfLines);
            for (var i = 0; i < numberOfLines; ++i)
            {
                CacheEntries[i].Set = i / Associativity;
            }
        }

        public int GetTagLength(string address)
        {
            return address.Length - SetIndexLength - BlockOffsetLength;
        }

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
            for (var i = index * Associativity; i < index * Associativity + Associativity; ++i)
            {
                ++CacheEntries[i].Age;
            }

            CacheEntries[newestEntryIndex].Age = 0;
        }

        //public void WriteToCache(string binaryAddress, int size, string data)
        //{
        //    if (CacheConfig.WritePolicy == WritePolicy.WriteBack)
        //    {
        //        WriteBackWriteToCache(binaryAddress, size, data);
        //    }
        //}

        public void WriteToCache(string address, int size, string data)
        {
            // Check if address exists in the cache first.
            byte[] buffer;
            var binaryAddress = GetBinaryAddress(address);
            var index = GetIndex(binaryAddress, GetTagLength(binaryAddress)) * Associativity;
            var replacementIndex = index;

            for (var i = index; i < index + Associativity; ++i)
            {
                if (CacheEntries[i].FlagBits.Valid == Validity.Valid)
                {
                    if (CacheEntries[i].Tag == binaryAddress.Substring(0, CacheEntries[i].TagLength))
                    {
                        ++StatisticsInfo.CacheHits;
                        CacheEntries[i].TagLength = GetTagLength(binaryAddress);

                        //if (CacheConfig.ReplacementPolicy == ReplacementPolicy.LeastRecentlyUsed)

                        // Write data to cache.
                        buffer = Encoding.ASCII.GetBytes(data);
                        if (buffer.Length > size)
                        {
                            CacheEntries[i].DataBlock = new byte[size];
                            Buffer.BlockCopy(buffer, 0, CacheEntries[i].DataBlock, 0, size);
                        }
                        else
                        {
                            CacheEntries[i].DataBlock = buffer;
                        }

                        if (CacheConfig.WritePolicy == WritePolicy.WriteBack)
                        {
                            CacheEntries[i].FlagBits.Dirty = true;
                        }
                        else if (CacheConfig.WritePolicy == WritePolicy.WriteThrough)
                        {
                            using var stream = File.Open(ramFileName, FileMode.Open);
                            if (UInt64.TryParse(address, out var offset))
                            {
                                stream.Seek((long)offset, SeekOrigin.Begin);
                                stream.Write(CacheEntries[i].DataBlock, 0, CacheEntries[i].DataBlock.Length);
                                ++StatisticsInfo.MemoryWrites;
                            }
                            //TODO: handle else case!
                        }

                        // Set age values.
                        if (CacheConfig.ReplacementPolicy == ReplacementPolicy.LeastRecentlyUsed)
                        {
                            Aging(i, CacheEntries[i].Set);
                        }

                        return;
                    }
                }
            }

            // After a cache miss look for available entry structure.
            ++StatisticsInfo.CacheMisses;
            ++StatisticsInfo.MemoryReads;
            index = GetIndex(binaryAddress, GetTagLength(binaryAddress)) * Associativity;

            for (var i = index; i < index + Associativity; ++i)
            {
                if (CacheEntries[i].FlagBits.Valid == Validity.Invalid)
                {
                    CacheEntries[i].FlagBits.Valid = Validity.Valid;
                    CacheEntries[i].TagLength = GetTagLength(binaryAddress);
                    CacheEntries[i].Tag = binaryAddress;

                    // Write data to cache.
                    buffer = Encoding.ASCII.GetBytes(data);
                    if (buffer.Length > size)
                    {
                        CacheEntries[i].DataBlock = new byte[size];
                        Buffer.BlockCopy(buffer, 0, CacheEntries[i].DataBlock, 0, size);
                    }
                    else
                    {
                        CacheEntries[i].DataBlock = buffer;
                    }

                    if (CacheConfig.WritePolicy == WritePolicy.WriteBack)
                    {
                        CacheEntries[i].FlagBits.Dirty = true;
                    }
                    else if (CacheConfig.WritePolicy == WritePolicy.WriteThrough)
                    {
                        using var stream = File.Open(ramFileName, FileMode.Open);
                        if (UInt64.TryParse(address, out var offset))
                        {
                            stream.Seek((long)offset, SeekOrigin.Begin);
                            stream.Write(CacheEntries[i].DataBlock, 0, CacheEntries[i].DataBlock.Length);
                            ++StatisticsInfo.MemoryWrites;
                        }
                        //TODO: handle else case!
                    }

                    // Set age values.
                    if (CacheConfig.ReplacementPolicy == ReplacementPolicy.LeastRecentlyUsed)
                    {
                        Aging(i, CacheEntries[i].Set);
                    }

                    return;
                }
            }

            ++StatisticsInfo.CacheEviction;
            if (CacheConfig.ReplacementPolicy == ReplacementPolicy.LeastRecentlyUsed)
            {
                // Check for entry structure in cache that can be removed and replaced with new data.
                index = GetIndex(binaryAddress, GetTagLength(binaryAddress)) * Associativity;

                for (int i = index, highestAge = 0; i < index + Associativity; ++i)
                {
                    if (CacheEntries[i].Age > highestAge)
                    {
                        highestAge = CacheEntries[i].Age;
                        replacementIndex = i;
                    }
                }
            }
            else if (CacheConfig.ReplacementPolicy == ReplacementPolicy.FirstInFirstOut)
            {
                replacementIndex = 0;
            }
            else if (CacheConfig.ReplacementPolicy == ReplacementPolicy.LastInFirstOut)
            {
                replacementIndex = CacheEntries.Count - 1;
            }
            else if (CacheConfig.ReplacementPolicy == ReplacementPolicy.Belady)
            {
                replacementIndex = BeladyGetIndex(LoadFutureCacheEntries(address.TrimStart('0')));
            }

            // If the write policy is write-back and the dirty flag is set, write the cache entry to RAM first.
            if (CacheConfig.WritePolicy == WritePolicy.WriteBack && CacheEntries[replacementIndex].FlagBits.Dirty)
            {
                try
                {
                    // Write oldest data data in cache to RAM because the dirty flag has been set.
                    using var stream = File.Open(ramFileName, FileMode.Open);
                    if (UInt64.TryParse(address, out var offset))
                    {
                        stream.Seek((long)offset, SeekOrigin.Begin);
                        stream.Write(CacheEntries[replacementIndex].DataBlock, 0, CacheEntries[replacementIndex].DataBlock.Length);
                        ++StatisticsInfo.MemoryWrites;
                    }
                    //TODO: handle else case!
                }
                catch (Exception)
                {
                    //TOOD: handle this!
                }
            }

            // Else just replace data in cache with new data.
            CacheEntries[replacementIndex].TagLength = GetTagLength(binaryAddress);
            CacheEntries[replacementIndex].Tag = binaryAddress;

            // Write data to cache.
            buffer = Encoding.ASCII.GetBytes(data);
            if (buffer.Length > size)
            {
                CacheEntries[replacementIndex].DataBlock = new byte[size];
                Buffer.BlockCopy(buffer, 0, CacheEntries[replacementIndex].DataBlock, 0, size);
            }
            else
            {
                CacheEntries[replacementIndex].DataBlock = buffer;
            }

            if (CacheConfig.WritePolicy == WritePolicy.WriteBack)
            {
                CacheEntries[replacementIndex].FlagBits.Dirty = true;
            }
            else if (CacheConfig.WritePolicy == WritePolicy.WriteThrough)
            {
                using var stream = File.Open(ramFileName, FileMode.Open);
                if (UInt64.TryParse(address, out var offset))
                {
                    stream.Seek((long)offset, SeekOrigin.Begin);
                    stream.Write(CacheEntries[replacementIndex].DataBlock, 0, CacheEntries[replacementIndex].DataBlock.Length);
                    ++StatisticsInfo.MemoryWrites;
                }
                //TODO: handle else case!
            }

            // Set age values.
            if (CacheConfig.ReplacementPolicy == ReplacementPolicy.LeastRecentlyUsed)
            {
                Aging(replacementIndex, CacheEntries[replacementIndex].Set);
            }
        }

        /// <summary>
        /// Returns index of the cache entry that needs to be replaced.
        /// </summary>
        /// <param name="addressList">List of all of the memory addresses that will be used in the future.</param>
        /// <returns>Index of the cache entry that needs to be replaced.</returns>
        private int BeladyGetIndex(List<string> addressList)
        {
            //TODO: test this!
            int farthestElement = 0, index = 0;

            for (var i = 0; i < CacheEntries.Count; ++i)
            {
                var tmpIndex = addressList.IndexOf(Convert.ToInt32(CacheEntries[i].Tag, 2).ToString("X"));

                if (tmpIndex >= farthestElement)
                {
                    farthestElement = tmpIndex;
                    index = i;
                }
                else if (tmpIndex == -1)
                {
                    return i;
                }
            }

            return index;
        }

        /// <summary>
        /// Load all of the addresses from the trace file used for the Bélády's algorithm.
        /// </summary>
        /// <param name="currentAddress">Address of the current request.</param>
        /// <returns>List of all of the unique memory addresses that will be used in the future.</returns>
        private List<string> LoadFutureCacheEntries(string currentAddress)
        {
            const int BufferSize = 4_096;
            using var fileStream = File.OpenRead(traceFileName);
            using var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, BufferSize);

            var output = new HashSet<string>();
            string line;
            while ((line = streamReader.ReadLine()) != null)
            {
                // Skip 0x and any leading 0 from the address.
                var address = line.Split(' ')[1].Trim(' ').Substring(2).TrimStart('0');
                if (currentAddress != address && !output.Contains(address))
                {
                    output.Add(address);
                }
            }

            return output.ToList();
        }

        //public void ReadFromCache(string binaryAddress, int size)
        //{
        //    if (CacheConfig.WritePolicy == WritePolicy.WriteBack)
        //    {
        //        WriteBackReadFromCache(binaryAddress, size);
        //    }
        //}

        public void ReadFromCache(string address, int size)
        {
            // Check if address exists in the cache first.
            var binaryAddress = GetBinaryAddress(address);
            var index = GetIndex(binaryAddress, GetTagLength(binaryAddress)) * Associativity;
            var replacementIndex = index;

            for (var i = index; i < index + Associativity; ++i)
            {
                if (CacheEntries[i].FlagBits.Valid == Validity.Valid)
                {
                    if (CacheEntries[i].Tag == binaryAddress.Substring(0, CacheEntries[i].TagLength))
                    {
                        ++StatisticsInfo.CacheHits;

                        if (CacheConfig.ReplacementPolicy == ReplacementPolicy.LeastRecentlyUsed)
                        {
                            // Set age values.
                            Aging(i, CacheEntries[i].Set);
                        }

                        return;
                    }
                }
            }

            // After a cache miss look for available entry structure.
            ++StatisticsInfo.CacheMisses;
            ++StatisticsInfo.MemoryReads;
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
                        using var stream = new FileStream(ramFileName, FileMode.Open, FileAccess.Read);
                        if (UInt64.TryParse(address, out var offset))
                        {
                            var buffer = new byte[size];
                            // TODO: could be a problem conversion from long to int. Fix this!
                            stream.Read(buffer, (int)offset, size);
                            CacheEntries[i].DataBlock = buffer;
                            ++StatisticsInfo.MemoryWrites;
                        }
                        //TODO: handle else case!
                    }
                    catch (Exception)
                    {
                        //TOOD: handle this!
                    }

                    if (CacheConfig.WritePolicy == WritePolicy.WriteBack)
                    {
                        CacheEntries[i].FlagBits.Dirty = false;
                    }

                    if (CacheConfig.ReplacementPolicy == ReplacementPolicy.LeastRecentlyUsed)
                    {
                        // Set age values.
                        Aging(i, CacheEntries[i].Set);
                    }

                    return;
                }
            }

            ++StatisticsInfo.CacheEviction;
            if (CacheConfig.ReplacementPolicy == ReplacementPolicy.LeastRecentlyUsed)
            {
                index = GetIndex(binaryAddress, GetTagLength(binaryAddress)) * Associativity;

                for (int i = index, highestAge = 0; i < index + Associativity; ++i)
                {
                    if (CacheEntries[i].Age > highestAge)
                    {
                        highestAge = CacheEntries[i].Age;
                        replacementIndex = i;
                    }
                }
            }
            else if (CacheConfig.ReplacementPolicy == ReplacementPolicy.FirstInFirstOut)
            {
                replacementIndex = 0;
            }
            else if (CacheConfig.ReplacementPolicy == ReplacementPolicy.LastInFirstOut)
            {
                replacementIndex = CacheEntries.Count - 1;
            }
            else if (CacheConfig.ReplacementPolicy == ReplacementPolicy.Belady)
            {
                replacementIndex = BeladyGetIndex(LoadFutureCacheEntries(address.TrimStart('0')));
            }

            // If the write policy is write-back and the dirty flag is set, write the cache entry to RAM first.
            if (CacheConfig.WritePolicy == WritePolicy.WriteBack && CacheEntries[replacementIndex].FlagBits.Dirty)
            {
                try
                {
                    // Write oldest data data in cache to RAM because the dirty flag has been set.
                    using var stream = File.Open(ramFileName, FileMode.Open);
                    if (UInt64.TryParse(address, out var offset))
                    {
                        stream.Seek((long)offset, SeekOrigin.Begin);
                        stream.Write(CacheEntries[replacementIndex].DataBlock, 0, CacheEntries[replacementIndex].DataBlock.Length);
                        ++StatisticsInfo.MemoryWrites;
                    }
                    //TODO: handle else case!
                }
                catch (Exception)
                {
                    //TOOD: handle this!
                }
            }

            CacheEntries[replacementIndex].TagLength = GetTagLength(binaryAddress);
            CacheEntries[replacementIndex].Tag = binaryAddress;

            if (CacheConfig.WritePolicy == WritePolicy.WriteBack)
            {
                CacheEntries[replacementIndex].FlagBits.Dirty = false;
            }

            try
            {
                // Read the data from the RAM.
                using var stream = new FileStream(ramFileName, FileMode.Open, FileAccess.Read);
                if (UInt64.TryParse(address, out var offset))
                {
                    var buffer = new byte[size];
                    // TODO: could be a problem conversion from long to int. Fix this!
                    stream.Read(buffer, (int)offset, size);
                    CacheEntries[replacementIndex].DataBlock = buffer;
                    ++StatisticsInfo.MemoryWrites;
                }
                //TODO: handle else case!
            }
            catch (Exception)
            {
                //TOOD: handle this!
            }

            // Set age values.
            if (CacheConfig.ReplacementPolicy == ReplacementPolicy.LeastRecentlyUsed)
            {
                Aging(replacementIndex, CacheEntries[replacementIndex].Set);
            }
        }
    }
}
