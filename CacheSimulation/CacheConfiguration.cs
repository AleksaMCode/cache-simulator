namespace CacheSimulation
{
    public class CacheConfiguration
    {
        public int BlockSize { get; set; } = 0;
        public WritePolicy WritePolicy { get; set; } = WritePolicy.WriteBack;
        public ReplacementPolicy ReplacementPolicy { get; set; } = ReplacementPolicy.LeastRecentlyUsed;

        public CacheConfiguration()
        {
        }

        public CacheConfiguration(int blockSize, WritePolicy writePolicy, ReplacementPolicy replacementPolicy)
        {
            SetCacheConfig(blockSize, writePolicy, replacementPolicy);
        }

        public void SetCacheConfig(int blockSize, WritePolicy writePolicy, ReplacementPolicy replacementPolicy)
        {
            BlockSize = blockSize;
            WritePolicy = writePolicy;
            ReplacementPolicy = replacementPolicy;
        }
    }
}
