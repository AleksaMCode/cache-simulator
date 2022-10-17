namespace CacheSimulation
{
    public class CacheConfigurationBuilder : ICacheBuilder
    {
        private CacheConfiguration _config = new();

        public void Size(int size)
        {
            _config.BlockSize = size;
        }

        public void WriteHitPolicy(WritePolicy policy)
        {
            _config.WriteHitPolicy = policy;
        }

        public void WriteMissPolicy(WritePolicy policy)
        {
            _config.WriteMissPolicy = policy;
        }

        public void ReplacementPolicy(ReplacementPolicy policy)
        {
            _config.ReplacementPolicy = policy;
        }

        public CacheConfiguration Build()
        {
            return _config;
        }
    }
}
