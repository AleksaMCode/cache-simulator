namespace CacheSimulation
{
    public class CacheEntry
    {
        public string Tag { get; set; }
        public byte[] DataBlock { get; set; }
        public int Set { get; set; }
        /// <summary>
        /// Age bits used when Cache uses Least-recently used replacement algorithm to track of the usage of the LRU cache-lines.
        /// </summary>
        public int Age { get; set; } = 0;
        public int TagLength { get; set; } = 1;
        public FlagBits FlagBits { get; set; } = new FlagBits();
    }
}
