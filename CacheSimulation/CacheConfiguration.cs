namespace CacheSimulation
{
    public class CacheConfiguration
    {
        public int BlockSize { get; set; } = 0;
        public WritePolicy WriteHitPolicy { get; set; } = WritePolicy.WriteBack;
        public WritePolicy WriteMissPolicy { get; set; } = WritePolicy.WriteAllocate;
        public ReplacementPolicy ReplacementPolicy { get; set; } = ReplacementPolicy.LeastRecentlyUsed;

        public CacheConfiguration()
        {
        }

        public CacheConfiguration(int blockSize, WritePolicy writeHitPolicy, WritePolicy writeMissPolicy, ReplacementPolicy replacementPolicy)
        {
            SetCacheConfig(blockSize, writeHitPolicy, writeMissPolicy, replacementPolicy);
        }

        public void SetCacheConfig(int blockSize, WritePolicy writeHitPolicy, WritePolicy writeMissPolicy, ReplacementPolicy replacementPolicy)
        {
            BlockSize = blockSize;
            WriteHitPolicy = writeHitPolicy;
            WriteMissPolicy = writeMissPolicy;
            ReplacementPolicy = replacementPolicy;
        }
    }
}
