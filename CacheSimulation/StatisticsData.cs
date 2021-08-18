namespace CacheSimulation
{
    public class StatisticsData
    {
        public int CacheHits { get; set; } = 0;
        public int CacheMisses { get; set; } = 0;
        public int MemoryReads { get; set; } = 0;
        public int MemoryWrites { get; set; } = 0;
        public int CacheEviction { get; set; } = 0;

        public int GetTotalNumberOfAccessToCache()
        {
            return CacheHits + CacheMisses;
        }

        public double GetHitRate()
        {
            var totalNmb = GetTotalNumberOfAccessToCache();
            return totalNmb == 0 ? 0 : (double) CacheHits / totalNmb;
        }

        public double GetMissRate()
        {
            var totalNmb = GetTotalNumberOfAccessToCache();
            return totalNmb == 0 ? 0 : (double)CacheMisses / totalNmb;
        }
    }
}
