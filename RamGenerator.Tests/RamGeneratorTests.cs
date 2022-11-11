using System;
using System.IO;
using Xunit;

namespace RamGenerator.Tests
{
    public class RamGeneratorFixture : IDisposable
    {
        public int RamSizeInMB { get; set; } = 1_000;
        public RamGenerator RamGenerator { get; private set; }

        public RamGeneratorFixture()
        {
            RamGenerator = new RamGenerator(RamSizeInMB);
            RamGenerator.GenerateRam();
        }

        public void Dispose()
        {
            var filename = "ram*.dat";
            var dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            var files = dir.GetFiles(filename);

            foreach (var file in files)
            {
                File.Delete(file.FullName);
            }
        }
    }

    public class RamGeneratorTests : IClassFixture<RamGeneratorFixture>
    {
        readonly RamGeneratorFixture fixture;

        public RamGeneratorTests(RamGeneratorFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact]
        public void Generating_RAM_File()
        {
            var expectedNumberOfRamInstances = 1;
            var filename = "ram*.dat";
            var actualNumberOfRamInstances = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, filename).Length;

            Assert.Equal(expectedNumberOfRamInstances, actualNumberOfRamInstances);
        }

        [Fact]
        public void Generating_RAM_File_Twice()
        {
            fixture.RamGenerator.GenerateRam();
            var filename = "ram*.dat";
            var expectedNumberOfRamInstances = 1;
            var actualNumberOfRamInstances = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, filename).Length;

            Assert.Equal(expectedNumberOfRamInstances, actualNumberOfRamInstances);
        }

        [Fact]
        public void Checking_RAM_File_Size()
        {
            var ramSize = new FileInfo(fixture.RamGenerator.FileName).Length;

            var blockSize = 1024 * 8;
            var blocksPerMb = 1_024 * 1_024 / blockSize;
            var calculatedRamSize = blocksPerMb * fixture.RamSizeInMB * blockSize;

            Assert.Equal(calculatedRamSize, ramSize);
        }


    }
}
