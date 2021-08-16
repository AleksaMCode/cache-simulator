using System;
using System.Collections.Generic;
using System.Linq;

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
        FirstInFirstOut = 2
    }

    public class Cache
    {
        public List<CacheEntry> CacheEntries;

        public int CacheHits { get; set; } = 0;
        public int CacheMisses { get; set; } = 0;
        public int MemoryReads { get; set; } = 0;
        public int MemoryWrites { get; set; } = 0;
        public int NumberOfLines { get; set; } = 0;
        public int Size { get; set; } = 0;
        public int Associativity { get; set; } = 0;
        public int BlockOffsetLength { get; set; } = 0;
        public int SetIndexLength { get; set; } = 0;
        public CacheConfiguration CacheConfig { get; set; } = new CacheConfiguration();

        public void CreateColdCache(int numberOfLines)
        {
            CacheEntries = new List<CacheEntry>(numberOfLines);
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
        /// <param name="newest"></param>
        /// <param name="index"></param>
        public void Aging(int newest, int index)
        {
            for (var i = index * Associativity; i < index * Associativity + Associativity; ++i)
            {
                ++CacheEntries[i].Age;
            }

            CacheEntries[newest].Age = 0;
        }

        public void WriteToCache(string binaryAddress)
        {
            if (CacheConfig.WritePolicy == WritePolicy.WriteBack)
            {
                WriteBackWriteToCache(binaryAddress);
            }
        }

        public void WriteBackWriteToCache(string binaryAddress)
        {
            var index = GetIndex(binaryAddress, GetTagLength(binaryAddress)) * Associativity;
            var highestAgeEntryIndex = index;

            for (var i = index; i < index + Associativity; ++i)
            {
                if (CacheEntries[i].FlagBits.Valid == Validity.Valid)
                {
                    if (CacheEntries[i].Tag == binaryAddress.Substring(0, CacheEntries[i].TagLength))
                    {
                        ++CacheHits;
                        CacheEntries[i].TagLength = GetTagLength(binaryAddress);

                        if (CacheConfig.ReplacementPolicy == ReplacementPolicy.LeastRecentlyUsed)
                        {
                            CacheEntries[i].FlagBits.Dirty = true;
                            Aging(i, CacheEntries[i].Set);
                        }

                        return;
                    }

                }
            }

            ++CacheMisses;
            ++MemoryReads;
            index = GetIndex(binaryAddress, GetTagLength(binaryAddress)) * Associativity;

            for (var i = index; i < index + Associativity; ++i)
            {
                if (CacheEntries[i].FlagBits.Valid == Validity.Invalid)
                {
                    CacheEntries[i].FlagBits.Valid = Validity.Valid;
                    CacheEntries[i].TagLength = GetTagLength(binaryAddress);
                    CacheEntries[i].Tag = binaryAddress;

                    if (CacheConfig.ReplacementPolicy == ReplacementPolicy.LeastRecentlyUsed)
                    {
                        CacheEntries[i].FlagBits.Dirty = true;
                        Aging(i, CacheEntries[i].Set);
                    }

                    return;
                }
            }

            index = GetIndex(binaryAddress, GetTagLength(binaryAddress)) * Associativity;
            var highestAge = 0;

            for (var i = index; i < index + Associativity; ++i)
            {
                if (CacheEntries[i].Age > highestAge)
                {
                    highestAge = CacheEntries[i].Age;
                    highestAgeEntryIndex = i;
                }
            }

            if (CacheEntries[highestAgeEntryIndex].FlagBits.Dirty)
            {
                ++MemoryWrites;
            }

            CacheEntries[highestAgeEntryIndex].TagLength = GetTagLength(binaryAddress);
            CacheEntries[highestAgeEntryIndex].Tag = binaryAddress;
            CacheEntries[highestAgeEntryIndex].FlagBits.Dirty = true;
            Aging(highestAgeEntryIndex, CacheEntries[highestAgeEntryIndex].Set);
        }

        public void ReadFromCache(string binaryAddress)
        {
            if (CacheConfig.WritePolicy == WritePolicy.WriteBack)
            {
                WriteBackReadFromCache(binaryAddress);
            }
        }

        public void WriteBackReadFromCache(string binaryAddress)
        {
            var index = GetIndex(binaryAddress, GetTagLength(binaryAddress)) * Associativity;
            var highestAgeEntryIndex = index;

            for (var i = index; i < index + Associativity; ++i)
            {
                if (CacheEntries[i].FlagBits.Valid == Validity.Valid)
                {
                    if (CacheEntries[i].Tag == binaryAddress.Substring(0, CacheEntries[i].TagLength))
                    {
                        ++CacheHits;

                        if (CacheConfig.ReplacementPolicy == ReplacementPolicy.LeastRecentlyUsed)
                        {
                            Aging(i, CacheEntries[i].Set);
                        }

                        return;
                    }

                }
            }

            ++CacheMisses;
            ++MemoryReads;
            index = GetIndex(binaryAddress, GetTagLength(binaryAddress)) * Associativity;

            for (var i = index; i < index + Associativity; ++i)
            {
                if (CacheEntries[i].FlagBits.Valid == Validity.Invalid)
                {
                    CacheEntries[i].FlagBits.Valid = Validity.Valid;
                    CacheEntries[i].TagLength = GetTagLength(binaryAddress);
                    CacheEntries[i].Tag = binaryAddress;

                    if (CacheConfig.ReplacementPolicy == ReplacementPolicy.LeastRecentlyUsed)
                    {
                        CacheEntries[i].FlagBits.Dirty = true;
                        Aging(i, CacheEntries[i].Set);
                    }

                    return;
                }
            }

            index = GetIndex(binaryAddress, GetTagLength(binaryAddress)) * Associativity;
            var highestAge = 0;

            for (var i = index; i < index + Associativity; ++i)
            {
                if (CacheEntries[i].Age > highestAge)
                {
                    highestAge = CacheEntries[i].Age;
                    highestAgeEntryIndex = i;
                }
            }

            if (CacheEntries[highestAgeEntryIndex].FlagBits.Dirty)
            {
                ++MemoryWrites;
            }

            CacheEntries[highestAgeEntryIndex].TagLength = GetTagLength(binaryAddress);
            CacheEntries[highestAgeEntryIndex].Tag = binaryAddress;
            CacheEntries[highestAgeEntryIndex].FlagBits.Dirty = false;
            Aging(highestAgeEntryIndex, CacheEntries[highestAgeEntryIndex].Set);
        }
    }
}
