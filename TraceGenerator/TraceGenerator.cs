using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Prng;
using Org.BouncyCastle.Security;

namespace TraceGenerator
{
    public class TraceGenerator
    {
        private static readonly SecureRandom csprng = new(new DigestRandomGenerator(new Sha256Digest()));

        /// <summary>
        /// Name of the trace file.
        /// </summary>
        private readonly string fileName;

        /// <summary>
        /// Size of trace file (number of instructions).
        /// </summary>
        private readonly int traceSize;

        /// <summary>
        /// Map for trace size that links size label with integer size.s
        /// </summary>
        private readonly Dictionary<string, int> traceSizes = new()
        {
            { "small", 100 },
            { "medium", 1_000 },
            { "large", 10_000 }
        };

        /// <summary>
        /// Currently available instructions to execute on the simulated CPU used to interact with cache and RAM.
        /// </summary>
        private readonly string[] instructions = new string[] { "L", "S" };

        /// <summary>
        /// Generates object of class TraceGenerator which can then be used to create a trace file on HDD filled with instructions.
        /// </summary>
        /// <param name="traceSize">Trace file category (string label for trace size).</param>
        /// <param name="fileName">Name of the trace file.</param>
        public TraceGenerator(string traceSize, string fileName = "instructions")
        {
            this.fileName = $"{fileName}-{DateTime.Now:yyyyMMddHHmmss}.trace";
            this.traceSize = traceSizes[traceSize.ToLower()];
        }

        /// <summary>
        /// Generates radnom hex number with size limit used as a data to store in RAM and cache.
        /// </summary>
        /// <param name="size">Size of number in bytes.</param>
        /// <returns>Random hex. number.</returns>
        private string RandomHexNumberGenerator(int size)
        {
            var bytes = new byte[size != 0 ? size : 8];
            csprng.NextBytes(bytes);

            return string.Join("", bytes.Select(x => x.ToString("x")));
        }

        /// <summary>
        /// Gnerates random number in hex format that is used as a memory address.
        /// </summary>
        /// <param name="ramSize">Size of RAM in bytes.</param>
        /// <param name="dataBlockSize">Size of data block in bytes.</param>
        /// <returns>Random memory address in hex format.</returns>
        private string RandomAddressInRangeGenerator(int ramSize, int dataBlockSize)
        {
            return csprng.Next(0, ramSize - dataBlockSize).ToString("x");
        }

        /// <summary>
        /// Generates random integer inside of the range [lowerLimit,upperLimit].
        /// <param name="lowerLimit">Lower limit of the range which is included in range.</param>
        /// <param name="upperLimit">Upper limit of the range which is included in range.</param>
        /// </summary>
        /// <returns>Random integer number from range.</returns>
        private int RandomIntegerGenerator(int upperLimit, int lowerLimit = 0)
        {
            return csprng.Next(lowerLimit, upperLimit + 1);
        }

        /// <summary>
        /// Generates one line for the trace file.
        /// </summary>
        /// <param name="ramSize">Size of RAM in bytes.</param>
        /// <param name="dataBlockSize">Size of data block in bytes.</param>
        /// <returns>Generated instruction line.</returns>
        private string GenerateTraceLine(int ramSize, int dataBlockSize)
        {
            var size = RandomIntegerGenerator(dataBlockSize);
            var instruction = instructions[size % 2];

            return instruction == "L"
                ? $"{instruction}\t0x{RandomAddressInRangeGenerator(ramSize, dataBlockSize)}"
                : $"{instruction}\t0x{RandomAddressInRangeGenerator(ramSize, dataBlockSize)},\t{size},\t0x{RandomHexNumberGenerator(size)}";
        }

        /// <summary>
        /// Creates a trace file on HDD filled with CPU instructions used to store data in cache.
        /// </summary>
        /// <param name="ramSize">Size of RAM in megabytes.</param>
        /// <param name="dataBlockSize">Size of data block in cache entries in bytes.</param>
        /// <returns>true if the trace creation process is successful; otherwise false.</returns>
        public bool GenerateTraceFile(int ramSize, int dataBlockSize)
        {
            try
            {
                var sb = new StringBuilder();
                for (var i = 0; i < traceSize; ++i)
                {
                    sb.AppendLine(GenerateTraceLine(ramSize * 1_024 * 1_000, dataBlockSize));
                }

                File.WriteAllText(fileName, sb.ToString());
                return true;
            }
            catch (Exception)
            {
                //TODO: handle this!
                return false;
            }
        }
    }
}
