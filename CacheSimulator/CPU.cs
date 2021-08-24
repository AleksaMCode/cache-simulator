using System.Collections.Generic;
using CacheSimulation;

namespace CacheSimulator
{
    public class CPU
    {
        private List<CpuCore> cores { get; set; }

        public CPU((string ramFileName, string traceFileName, int size, int associativity, int blockSize, WritePolicy writeHitPolicy, WritePolicy writeMissPolicy, ReplacementPolicy replacementPolicy) cacheInfo, int numberOfCores)
        {
            cores = new List<CpuCore>(numberOfCores);

            //CPU cores initialization.
            for (var i = 0; i < numberOfCores; ++i)
            {
                cores.Add(new CpuCore(cacheInfo));
            }
        }

        //public void Start()
        //{
        //    const int bufferSize = 4_096;
        //    using var fileStream = File.OpenRead(L1.TraceFileName);
        //    using var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, bufferSize);

        //    string line;
        //    var currentLine = -1;
        //    while ((line = streamReader.ReadLine()) != null)
        //    {
        //        ++currentLine;
        //        Instruction instruction;

        //        try
        //        {
        //            instruction = L1.TraceLineParser(line);
        //        }
        //        catch (Exception)
        //        {
        //            continue;
        //        }

        //        if (instruction != null)
        //        {
        //            if (instruction.InstructionType == MemoryRelatedInstructions.Store)
        //            {
        //                var size = instruction.DataSize < L1.CacheConfig.BlockSize ? instruction.DataSize : L1.CacheConfig.BlockSize;
        //                L1.WriteToCache(instruction.MemoryAddress, size, instruction.Data, out var _, currentLine);
        //            }
        //            else
        //            {
        //                var size = instruction.DataSize < L1.CacheConfig.BlockSize ? instruction.DataSize : L1.CacheConfig.BlockSize;
        //                L1.ReadFromCache(instruction.MemoryAddress, size, out var _, currentLine);
        //            }
        //        }
        //    }
        //}

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
