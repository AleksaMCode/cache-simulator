namespace CacheSimulation
{
    public class CacheBuilder : ICacheBuilder
    {
        private readonly string ramFileName;
        private readonly CacheConfiguration config;
        private int size;
        private int associativity;

        public CacheBuilder(string ramFileName, CacheConfiguration config)
        {
            this.ramFileName = ramFileName;
            this.config = config;
        }

        public void Size(int size)
        {
            this.size = size;
        }

        public void Associativity(int associativity)
        {
            this.associativity = associativity;
        }

        public Cache Build()
        {
            Cache cache = config.ReplacementPolicy switch
            {
                ReplacementPolicy.Belady => new CacheBelady(ramFileName, config)
                {
                    Size = size,
                    Associativity = associativity
                },
                ReplacementPolicy.LeastRecentlyUsed => new CacheLRU(ramFileName, config)
                {
                    Size = size,
                    Associativity = associativity
                },
                _ => new CacheRandom(ramFileName, config)
                {
                    Size = size,
                    Associativity = associativity
                },
            };

            cache.CreateCache();
            return cache;
        }
    }
}
