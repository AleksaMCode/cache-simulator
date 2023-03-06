using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CacheSimulation
{
    public sealed class CacheBelady : Cache
    {
        public CacheBelady(string ramFileName, CacheConfiguration config)
        {
            RamFileName = ramFileName;
            CacheConfig = config;
        }

        /// <summary>
        /// Load all of the addresses from the trace file used for the Bélády's algorithm.
        /// </summary>
        /// <param name="traceIndex">Current line in trace file.</param>
        /// <returns>List of all of the unique memory addresses that will be used in the future.</returns>
        private List<string> LoadFutureCacheEntries(int traceIndex)
        {
            const int bufferSize = 4_096;
            using var fileStream = File.OpenRead(TraceFileName);
            using var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, bufferSize);
            //streamReader.BaseStream.Seek(traceIndex, SeekOrigin.Begin);

            var output = new List<string>();
            string line;
            var currentLine = 0;
            while ((line = streamReader.ReadLine()) != null)
            {
                ++currentLine;

                // Go to the trace index line.
                if (currentLine < traceIndex + 1)
                {
                    continue;
                }

                // Skip 0x and any leading 0 from the address.
                var address = line.Split('\t')[1].Trim(' ').Substring(2).TrimStart('0').TrimEnd(',').Trim(' ');
                if (!output.Contains(address))
                {
                    output.Add(address);
                }
            }

            return output;
        }

        /// <summary>
        /// Returns index of the cache entry that needs to be replaced.
        /// </summary>
        /// <param name="addressList">List of all of the memory addresses that will be used in the future.</param>
        /// <returns>Index of the cache entry that needs to be replaced.</returns>
        private int BeladyGetIndex(List<string> addressList, int startingIndex)
        {
            int farthestElement = 0, index = 0;

            for (var i = startingIndex; i < startingIndex + Associativity; ++i)
            {
                if (CacheEntries[i].Tag == null)
                {
                    continue;
                }

                var tmpIndex = addressList.IndexOf(ConvertBinaryToHex(CacheEntries[i].Tag));

                if (tmpIndex >= farthestElement)
                {
                    farthestElement = tmpIndex;
                    index = i;
                }
                else if (tmpIndex == -1)
                {
                    return i;
                }
            }

            return index;
        }

        protected override int GetReplacementIndex(int index, int traceIndex)
        {
            return BeladyGetIndex(LoadFutureCacheEntries(traceIndex), index);
        }
    }
}
