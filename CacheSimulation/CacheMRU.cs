namespace CacheSimulation
{
    public class CacheMRU : Cache
    {
        public CacheMRU(string ramFileName, CacheConfiguration config)
        {
            RamFileName = ramFileName;
            CacheConfig = config;
        }

        protected override int GetReplacementIndex(int index, int traceIndex)
        {
            var replacementIndex = index;

            for (int i = index, lowestAge = 0; i < index + Associativity; ++i)
            {
                if (CacheEntries[i].Age < lowestAge)
                {
                    lowestAge = CacheEntries[i].Age;
                    replacementIndex = i;
                }
            }

            return replacementIndex;
        }
    }
}
