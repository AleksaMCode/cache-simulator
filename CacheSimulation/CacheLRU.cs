namespace CacheSimulation
{
    public class CacheLRU : Cache
    {
        public CacheLRU(string ramFileName, CacheConfiguration config)
        {
            RamFileName = ramFileName;
            CacheConfig = config;
        }

        /// <summary>
        /// Updates the most recently used set, allowing the LRU algorithm to work.
        /// </summary>
        /// <param name="newestEntryIndex">Index of the newest entry in the cache.</param>
        /// <param name="index"></param>
        protected override void Aging(int newestEntryIndex, int index)
        {
            var limit = index * Associativity;
            var tmpAge = CacheEntries[newestEntryIndex].Age;

            for (var i = limit; i < limit + Associativity; ++i)
            {
                ++CacheEntries[i].Age;
            }

            CacheEntries[newestEntryIndex].Age = tmpAge;
        }

        protected override int GetReplacementIndex(int index, int traceIndex)
        {
            var replacementIndex = index;

            for (int i = index, highestAge = 0; i < index + Associativity; ++i)
            {
                if (CacheEntries[i].Age > highestAge)
                {
                    highestAge = CacheEntries[i].Age;
                    replacementIndex = i;
                }
            }

            return replacementIndex;
        }
    }
}
