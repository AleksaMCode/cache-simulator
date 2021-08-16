using System.Collections;

namespace CacheSimulation
{
    public class CacheEntry
    {
        public BitArray Tag { get; set; } = new BitArray(81);
        public BitArray DataBlock { get; set; }
        public int Set { get; set; }
        /// <summary>
        /// Age bits used when Cache uses Least-recently used replacment algorithm to track of the usage of the LRU cache-lines.
        /// </summary>
        public int Age { get; set; } = 0;
        public int TagLength { get; set; } = 1;
        public FlagBits FlagBits { get; set; } = new FlagBits();
    }
}
