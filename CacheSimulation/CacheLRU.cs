namespace CacheSimulation
{
    public class CacheLRU : Cache
    {
        public CacheLRU(string ramFileName, CacheConfiguration config)
        {
            RamFileName = ramFileName;
            CacheConfig = config;
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
