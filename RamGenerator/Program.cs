using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RamGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            var ramGn = new RamGenerator(1_000);
            ramGn.GenerateRam();
        }
    }
}
