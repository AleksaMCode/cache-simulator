using System;
using System.IO;
using System.Text;
using CacheSimulation;

namespace CacheSimulator
{
    public class CPU
    {
        private Cache L1;

        public CPU((string ramFileName, string traceFileName, int size, int associativity, int blockSize, WritePolicy writePolicy, ReplacementPolicy replacementPolicy) cacheInfo)
        {
            L1 = new Cache(cacheInfo);
        }

        public void StartSimulation()
        {
            const int bufferSize = 4_096;
            using var fileStream = File.OpenRead(L1.TraceFileName);
            using var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, bufferSize);

            string line;
            while ((line = streamReader.ReadLine()) != null)
            {
                Instruction instruction;
                try
                {
                    instruction = L1.TraceLineParser(line);
                }
                catch(Exception)
                {
                    continue;
                }

                if (instruction != null)
                {
                    if (instruction.InstructionType == MemoryRelatedInstructions.Store)
                    {
                        var size = instruction.DataSize < L1.CacheConfig.BlockSize ? instruction.DataSize : L1.CacheConfig.BlockSize;
                        L1.WriteToCache(instruction.MemoryAddress, size, instruction.Data);
                    }
                    else
                    {
                        var size = instruction.DataSize < L1.CacheConfig.BlockSize ? instruction.DataSize : L1.CacheConfig.BlockSize;
                        L1.ReadFromCache(instruction.MemoryAddress, size);
                    }
                }
            }
        }
    }
}
