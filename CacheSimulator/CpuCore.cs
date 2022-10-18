using System;
using System.Globalization;
using System.Text;
using CacheSimulation;

namespace CacheSimulator
{
    public class CpuCore
    {
        private Cache L1d;

        public CpuCore(CacheBuilder cacheBuilder)
        {
            L1d = cacheBuilder.Build();
        }

        public string ExecuteTraceLine(string traceLine, int traceIndex, int coreNumber)
        {
            Instruction instruction;
            var sb = new StringBuilder();
            try
            {
                instruction = L1d.TraceLineParser(traceLine);

                if (instruction.DataSize == 0)
                {
                    return null;
                }

                var dataSizeString = $"data_size={instruction.DataSize}B";
                var tmp = new StringBuilder();
                tmp.Append($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] core={coreNumber} operation={(instruction.InstructionType == MemoryRelatedInstructions.Load ? $"LOAD {dataSizeString}" : $"STORE {dataSizeString} data=0x{instruction.Data}")} ");

                if (instruction != null)
                {
                    if (instruction.InstructionType == MemoryRelatedInstructions.Store)
                    {
                        var size = instruction.DataSize < L1d.CacheConfig.BlockSize ? instruction.DataSize : L1d.CacheConfig.BlockSize;
                        var hitCheck = L1d.WriteToCache(instruction.MemoryAddress, size, instruction.Data, out var additionalData, traceIndex, coreNumber);

                        tmp.Append($"status={(hitCheck ? "hit" : "miss")}");
                        sb.AppendLine(tmp.ToString());

                        if (additionalData != "")
                        {
                            sb.AppendLine(additionalData);
                        }
                    }
                    else
                    {
                        var size = instruction.DataSize < L1d.CacheConfig.BlockSize ? instruction.DataSize : L1d.CacheConfig.BlockSize;
                        var hitCheck = L1d.ReadFromCache(instruction.MemoryAddress, size, out var additionalData, traceIndex, coreNumber);

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

        public void SetTraceFileForL1(string traceFileName)
        {
            L1d.TraceFileName = traceFileName;
        }

        public string GetCacheStatistics(int coreNumber)
        {
            var sb = new StringBuilder();

            sb.AppendLine($"Core {coreNumber}");
            sb.AppendLine("CACHE SETTINGS:");
            sb.AppendLine("Only D-cache");
            sb.AppendLine($"D-cache size: {string.Format(CultureInfo.InvariantCulture, "{0:0,0}", L1d.Size)}");
            sb.AppendLine($"Associativity: {(L1d.Associativity == 1 ? "Directly mapped" : L1d.Associativity + "-way set associative")}");
            sb.AppendLine($"Block size: {L1d.CacheConfig.BlockSize}");
            sb.AppendLine($"Write-hit policy: {(L1d.CacheConfig.WriteHitPolicy == WritePolicy.WriteBack ? "Write-back" : "Write-through")}");
            sb.AppendLine($"Write-miss policy: {(L1d.CacheConfig.WriteMissPolicy == WritePolicy.WriteAllocate ? "Write allocate" : "No-write allocate")}");
            sb.AppendLine("\nCACHE STATISTICS:");
            sb.AppendLine(L1d.StatisticsInfo.GetStatistics());

            return sb.ToString();
        }
    }
}
