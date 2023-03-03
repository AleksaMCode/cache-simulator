using System.Collections.Generic;

namespace CacheSimulation
{
    public sealed class CacheFIFO : Cache
    {
        private Queue<int> indexQueue { get; set; } = new();

        public CacheFIFO(string ramFileName, CacheConfiguration config)
        {
            RamFileName = ramFileName;
            CacheConfig = config;
        }

        protected override int GetReplacementIndex(int index, int traceIndex)
        {
            var replacementIndex = indexQueue.Dequeue();
            indexQueue.Enqueue(replacementIndex);

            return replacementIndex;
        }
    }
}
