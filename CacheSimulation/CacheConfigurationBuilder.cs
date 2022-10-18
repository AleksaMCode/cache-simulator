using System;

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
            return _config.WriteHitPolicy == WritePolicy.WriteThrough && _config.WriteMissPolicy == WritePolicy.WriteAllocate
                ? throw new Exception("A write-through cache uses no-write allocate (write around). Here, subsequent writes have no advantage, since they still need to be written directly to the backing store.")
                : _config;
        }
    }
}
