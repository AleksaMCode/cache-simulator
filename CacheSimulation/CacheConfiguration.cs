namespace CacheSimulation
{
    public class CacheConfiguration
    {
        public int BlockSize { get; set; } = 0;
        public WritePolicy WritePolicy { get; set; } = WritePolicy.WriteBack;
        public ReplacementPolicy ReplacementPolicy { get; set; } = ReplacementPolicy.LeastRecentlyUsed;
    }
}
