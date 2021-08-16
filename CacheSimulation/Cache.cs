using System;
using System.Collections;
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

        public int GetTagLength(BitArray address)
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
    }
}
