using System.Collections.Generic;
using CacheSimulation;

namespace CacheSimulator
{
    public class CPU
    {
        private List<CpuCore> cores { get; set; }

        public CPU(CacheBuilder cacheBuilder, int numberOfCores)
        {
            cores = new List<CpuCore>(numberOfCores);

            // CPU cores initialization.
            for (var i = 0; i < numberOfCores; ++i)
            {
                cores.Add(new CpuCore(cacheBuilder.Build()));
            }
        }

        public void SetCoreTraceFile(int coreNumber, string traceFileName)
        {
            cores[coreNumber].SetTraceFileForL1(traceFileName);
        }

        public string ExecuteTraceLine(string traceLine, int traceIndex, int coreNumber)
        {
            return cores[coreNumber].ExecuteTraceLine(traceLine, traceIndex, coreNumber);
        }

        public string GetCacheStatistics(int coreNumber)
        {
            return cores[coreNumber].GetCacheStatistics(coreNumber);
        }
    }
}
