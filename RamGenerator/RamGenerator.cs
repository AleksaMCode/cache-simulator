using System;
using System.IO;
using System.Security.Cryptography;

namespace RamGenerator
{
    public class RamGenerator
    {
        private static readonly RNGCryptoServiceProvider rngCsp = new RNGCryptoServiceProvider();

        /// <summary>
        /// Name of the RAM file.
        /// </summary>
        public readonly string FileName;

        /// <summary>
        /// Size of RAM in megabytes.
        /// </summary>
        private readonly int ramSize;

        private const int blockSize = 1024 * 8;

        /// <summary>
        /// Generates object of class RamGenerator which can then be used to create a RAM file on HDD filled with random data.
        /// </summary>
        /// <param name="fileName">Name of the ram file.</param>
        /// <param name="ramSize">Size of RAM file in megabytes.</param>
        public RamGenerator(int ramSize, string fileName = "ram")
        {
            FileName = $"{fileName}-{DateTime.Now:yyyyMMddHHmmss}.dat";
            this.ramSize = ramSize;
        }

        /// <summary>
        /// Creates a RAM file on HDD filled with random data.
        /// </summary>
        /// <returns>true if the RAM creation process is successful; otherwise false.</returns>
        public void GenerateRam()
        {
            var blocksPerMb = (1_024 * 1_024) / blockSize;
            var data = new byte[blockSize];

            try
            {

                using var stream = File.OpenWrite(FileName);
                for (var i = 0; i < ramSize * blocksPerMb; ++i)
                {
                    rngCsp.GetBytes(data);
                    stream.Write(data, 0, data.Length);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"RAM file generating failed.\n{ex.Message}");
            }
        }
    }
}
