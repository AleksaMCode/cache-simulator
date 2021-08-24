using CacheSimulation;

namespace CacheSimulator
{
    public class CpuCore
    {
        private string name { get; set; }
        private Cache L1;

        public CpuCore(string name)
        {
            this.name = name;
        }
    }
}
