using System.Collections;
using System.Collections.Generic;

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
        public int NumLines { get; set; } = 0;
        public int Size { get; set; } = 0;
        public int Associativity { get; set; } = 0;
        public int BlockNumber { get; set; } = 0;
        public int SetNumber { get; set; } = 0;
        public CacheConfiguration CacheConfig { get; set; } = new CacheConfiguration();

        public void CreateColdCache(int numberOfLines)
        {
            CacheEntries = new List<CacheEntry>(numberOfLines);
        }

        public int GetTagLength(BitArray address)
        {
            return address.Length - SetNumber - BlockNumber;
        }
    }
}
