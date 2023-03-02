using System;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Prng;
using Org.BouncyCastle.Security;

namespace CacheSimulation
{
    public class CacheRandom : Cache
    {
        private SecureRandom csprng;

        public CacheRandom(string ramFileName, CacheConfiguration config)
        {
            RamFileName = ramFileName;
            CacheConfig = config;
        }

        public override void CreateCache()
        {
            base.CreateCache();

            csprng = new(new DigestRandomGenerator(new Sha256Digest()));
            csprng.SetSeed(DateTime.Now.Ticks);
        }

        protected override int GetReplacementIndex(int index, int traceIndex)
        {
            return csprng.Next(index, index + Associativity);
        }
    }
}
