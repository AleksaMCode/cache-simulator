using System.Globalization;
using System.Text;

namespace CacheSimulation
{
    public class StatisticsData
    {
        public int CacheHits { get; set; } = 0;
        public int CacheMisses { get; set; } = 0;
        public int MemoryReads { get; set; } = 0;
        public int MemoryWrites { get; set; } = 0;
        public int CacheEviction { get; set; } = 0;

        public double GetHitRate()
        {
            var totalNmb = GetNumberOfAccesses();
            return totalNmb == 0 ? 0 : (double)CacheHits / totalNmb;
        }

        public double GetMissRate()
        {
            var totalNmb = GetNumberOfAccesses();
            return totalNmb == 0 ? 0 : (double)CacheMisses / totalNmb;
        }

        public int GetNumberOfAccesses()
        {
            return CacheHits + CacheMisses;
        }

        public string GetStatistics()
        {
            var sb = new StringBuilder();

            sb.AppendLine($"Number of accesses: {string.Format(CultureInfo.InvariantCulture, "{0:0,0}", GetNumberOfAccesses())}");
            sb.AppendLine($"Number of hits: {string.Format(CultureInfo.InvariantCulture, "{0:0,0}", CacheHits)}");
            sb.AppendLine(value: $" (hit rate: {GetHitRate():0.000})");
            sb.AppendLine($"Number of misses: {string.Format(CultureInfo.InvariantCulture, "{0:0,0}", CacheMisses)}");
            sb.AppendLine(value: $" (miss rate: {GetMissRate():0.000})");
            sb.AppendLine($"Number of cache evictions: {string.Format(CultureInfo.InvariantCulture, "{0:0,0}", CacheEviction)}");
            sb.AppendLine($"Number of memory writes: {string.Format(CultureInfo.InvariantCulture, "{0:0,0}", MemoryWrites)}");
            sb.AppendLine($"Number of memory reads: {string.Format(CultureInfo.InvariantCulture, "{0:0,0}", MemoryReads)}");

            return sb.ToString();
        }
    }
}
