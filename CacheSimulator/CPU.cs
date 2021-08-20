using System;
using System.IO;
using System.Text;
using CacheSimulation;

namespace CacheSimulator
{
    public class CPU
    {
        public Cache L1;

        public CPU((string ramFileName, string traceFileName, int size, int associativity, int blockSize, WritePolicy writePolicy, ReplacementPolicy replacementPolicy) cacheInfo)
        {
            L1 = new Cache(cacheInfo);
        }

        public void Start()
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
                catch (Exception)
                {
                    continue;
                }

                if (instruction != null)
                {
                    if (instruction.InstructionType == MemoryRelatedInstructions.Store)
                    {
                        var size = instruction.DataSize < L1.CacheConfig.BlockSize ? instruction.DataSize : L1.CacheConfig.BlockSize;
                        L1.WriteToCache(instruction.MemoryAddress, size, instruction.Data, out var _);
                    }
                    else
                    {
                        var size = instruction.DataSize < L1.CacheConfig.BlockSize ? instruction.DataSize : L1.CacheConfig.BlockSize;
                        L1.ReadFromCache(instruction.MemoryAddress, size, out var _);
                    }
                }
            }
        }

        public string Start(string traceLine)
        {
            Instruction instruction;
            var sb = new StringBuilder();
            try
            {
                instruction = L1.TraceLineParser(traceLine);
                var dataSizeString = $"data_size={instruction.DataSize}B";
                var tmp = new StringBuilder();
                tmp.Append($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] operation={(instruction.InstructionType == MemoryRelatedInstructions.Load ? $"LOAD {dataSizeString}" : $"STORE {dataSizeString} data=0x{instruction.Data}")} ");

                if (instruction != null)
                {
                    if (instruction.InstructionType == MemoryRelatedInstructions.Store)
                    {
                        var size = instruction.DataSize < L1.CacheConfig.BlockSize ? instruction.DataSize : L1.CacheConfig.BlockSize;
                        var hitCheck = L1.WriteToCache(instruction.MemoryAddress, size, instruction.Data, out var additionalData);

                        tmp.Append($"status={(hitCheck ? "hit" : "miss")}");
                        sb.AppendLine(tmp.ToString());

                        if (additionalData != "")
                        {
                            sb.AppendLine(additionalData);
                        }
                    }
                    else
                    {
                        var size = instruction.DataSize < L1.CacheConfig.BlockSize ? instruction.DataSize : L1.CacheConfig.BlockSize;
                        var hitCheck = L1.ReadFromCache(instruction.MemoryAddress, size, out var additionalData);

                        tmp.Append($"status={(hitCheck ? "hit" : "miss")}");
                        sb.AppendLine(tmp.ToString());

                        if (additionalData != "")
                        {
                            sb.AppendLine(additionalData);
                        }
                    }

                    return sb.ToString();
                }

                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
