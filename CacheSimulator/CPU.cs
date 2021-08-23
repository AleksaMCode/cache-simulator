using System;
using System.Globalization;
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

        public void Start()
        {
            const int bufferSize = 4_096;
            using var fileStream = File.OpenRead(L1.TraceFileName);
            using var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, bufferSize);

            string line;
            var currentLine = -1;
            while ((line = streamReader.ReadLine()) != null)
            {
                ++currentLine;
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
                        L1.WriteToCache(instruction.MemoryAddress, size, instruction.Data, out var _, currentLine);
                    }
                    else
                    {
                        var size = instruction.DataSize < L1.CacheConfig.BlockSize ? instruction.DataSize : L1.CacheConfig.BlockSize;
                        L1.ReadFromCache(instruction.MemoryAddress, size, out var _, currentLine);
                    }
                }
            }
        }

        public string ExecuteTraceLine(string traceLine, int traceIndex)
        {
            Instruction instruction;
            var sb = new StringBuilder();
            try
            {
                instruction = L1.TraceLineParser(traceLine);

                if (instruction.DataSize == 0)
                {
                    return null;
                }

                var dataSizeString = $"data_size={instruction.DataSize}B";
                var tmp = new StringBuilder();
                tmp.Append($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] operation={(instruction.InstructionType == MemoryRelatedInstructions.Load ? $"LOAD {dataSizeString}" : $"STORE {dataSizeString} data=0x{instruction.Data}")} ");

                if (instruction != null)
                {
                    if (instruction.InstructionType == MemoryRelatedInstructions.Store)
                    {
                        var size = instruction.DataSize < L1.CacheConfig.BlockSize ? instruction.DataSize : L1.CacheConfig.BlockSize;
                        var hitCheck = L1.WriteToCache(instruction.MemoryAddress, size, instruction.Data, out var additionalData, traceIndex);

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
                        var hitCheck = L1.ReadFromCache(instruction.MemoryAddress, size, out var additionalData, traceIndex);

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

        public string GetTraceFileName()
        {
            return L1.TraceFileName;
        }

        public string GetCacheStatistics()
        {
            var sb = new StringBuilder();

            sb.AppendLine("CACHE SETTINGS:");
            sb.AppendLine("Only D-cache");
            sb.AppendLine($"D-cache size: {string.Format(CultureInfo.InvariantCulture, "{0:0,0}", L1.Size)}");
            sb.AppendLine($"Associativity: {(L1.Associativity == 1 ? "Directly mapped" : L1.Associativity + "-way set associative")}");
            sb.AppendLine($"Block size: {L1.CacheConfig.BlockSize}");
            sb.AppendLine($"Write policy: {(L1.CacheConfig.WritePolicy == WritePolicy.WriteBack ? "Write-back" : "Write-through")}");
            sb.AppendLine("\nCACHE STATISTICS:");
            sb.AppendLine(L1.StatisticsInfo.Statistics());

            return sb.ToString();
        }
    }
}
