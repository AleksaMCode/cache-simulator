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
        private readonly string fileName;

        /// <summary>
        /// Size of RAM in megabytes.
        /// </summary>
        private readonly int ramSize;

        private const int blockSize = 1024 * 8;

        /// <summary>
        /// Generates object of class RamGenerator which can then be used to create a RAM file on HDD filled with random data..
        /// </summary>
        /// <param name="fileName">Name of the ram file.</param>
        /// <param name="ramSize">Size of RAM file in megabytes.</param>
        public RamGenerator(string fileName = "RAM.dat", int ramSize)
        {
            this.fileName = fileName;
            this.ramSize = ramSize;
        }

        /// <summary>
        /// Creates a RAM file on HDD filled with random data.
        /// </summary>
        /// <returns>true if the RAM creation process is successful; otherwise false.</returns>
        public bool GenerateRam()
        {
            var blocksPerMb = (1_024 * 1_024) / blockSize;
            var data = new byte[blockSize];

            try
            {

                using var stream = File.OpenWrite(fileName);
                for (var i = 0; i < ramSize * blocksPerMb; ++i)
                {
                    rngCsp.GetBytes(data);
                    stream.Write(data, 0, data.Length);
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
