using System.Collections.Generic;
using System.Linq;

namespace CacheSimulation
{
    public sealed class CacheFIFO : Cache
    {
        private List<int> fifoIndexQueue { get; set; }

        public CacheFIFO(string ramFileName, CacheConfiguration config)
        {
            RamFileName = ramFileName;
            CacheConfig = config;
        }

        protected override int GetReplacementIndex(int index, int traceIndex)
        {
            var replacementIndex = fifoIndexQueue.First();
            fifoIndexQueue.RemoveAt(0);
            fifoIndexQueue.Add(replacementIndex);

            return replacementIndex;
        }
    }
}
