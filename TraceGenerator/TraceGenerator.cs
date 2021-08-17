using System;
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

        private readonly string[] instructions = new string[] { "L", "S", "M" };

        public TraceGenerator(int traceSize, string fileName = "instructions.trace")
        {
            this.fileName = fileName;
            this.traceSize = traceSize;
        }

        private string RandomHexNumberGenerator(int size)
        {
            var bytes = new byte[size != 0 ? size : 8];
            csprng.NextBytes(bytes);

            return string.Join("", bytes.Select(x => x.ToString("x")));
        }

        private string RandomAddressInRangeGenerator(int ramSize, int dataBlockSize)
        {
            return csprng.Next(0, ramSize - dataBlockSize).ToString("x");
        }

        private int RandomIntegerGenerator(int upperLimit, int lowerLimit = 0)
        {
            return csprng.Next(lowerLimit, upperLimit + 1);
        }

        private string GenerateTraceLine(int ramSize, int dataBlockSize)
        {
            var size = RandomIntegerGenerator(dataBlockSize);
            return $"{instructions[RandomIntegerGenerator(2)]}\t0x{RandomAddressInRangeGenerator(ramSize, dataBlockSize)},\t{size},\t0x{RandomHexNumberGenerator(size)}";
        }

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
                return false;
            }
        }
    }
}
