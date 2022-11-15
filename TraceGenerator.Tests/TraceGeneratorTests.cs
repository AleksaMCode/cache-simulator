using System;
using System.IO;
using Xunit;

namespace TraceGenerator.Tests
{

    public class TraceGeneratorFixture : IDisposable
    {
        public int RamSizeInMB { get; set; } = 1_000;
        public TraceGenerator TraceGenerator { get; private set; }

        public TraceGeneratorFixture()
        {
            TraceGenerator = new TraceGenerator("medium");
        }

        public void Dispose()
        {
            var filename = "instructions-*.trace";
            var dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            var files = dir.GetFiles(filename);

            foreach (var file in files)
            {
                File.Delete(file.FullName);
            }
        }
    }

    public class TraceGeneratorTests : IClassFixture<TraceGeneratorFixture>
    {
        readonly TraceGeneratorFixture fixture;

        private readonly int ramSizeInMB = 1_000;
        private readonly int dataBlockSize = 32;

        public TraceGeneratorTests(TraceGeneratorFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact]
        public void Generating_Trace_File()
        {
            fixture.TraceGenerator.GenerateTraceFile(ramSizeInMB, dataBlockSize);
            var expectedNumberOfRamInstances = 1;
            var filename = "instructions-*.trace";
            var actualNumberOfRamInstances = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, filename).Length;

            Assert.Equal(expectedNumberOfRamInstances, actualNumberOfRamInstances);
        }

        [Fact]
        public void Generating_Trace_File_Twice()
        {
            fixture.TraceGenerator.GenerateTraceFile(ramSizeInMB, dataBlockSize);
            fixture.TraceGenerator.GenerateTraceFile(ramSizeInMB, dataBlockSize);
            var filename = "instructions-*.trace";
            var expectedNumberOfRamInstances = 1;
            var actualNumberOfRamInstances = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, filename).Length;

            Assert.Equal(expectedNumberOfRamInstances, actualNumberOfRamInstances);
        }

        [Fact]
        public void Generating_Trace_File_With_Wrong_Size()
        {
            Assert.Throws<Exception>(() => new TraceGenerator("tiny"));
        }

        [Theory]
        [InlineData("small", 100)]
        [InlineData("medium", 1_000)]
        [InlineData("large", 10_000)]
        public void Checking_Trace_File_Line_Number(string size, int expectedNumberOfLines)
        {
            var trace = new TraceGenerator(size);
            trace.GenerateTraceFile(ramSizeInMB, dataBlockSize);

            using var stream = File.OpenRead(trace.FileName);

            var lineCount = 0;
            const char CR = '\r';
            const char LF = '\n';
            const char NULL = (char)0;
            var byteBuffer = new byte[1024 * 1024];
            const int bytesAtTheTime = 4;
            var detectedEOL = NULL;
            var currentChar = NULL;

            int bytesRead;
            while ((bytesRead = stream.Read(byteBuffer, 0, byteBuffer.Length)) > 0)
            {
                var i = 0;
                for (; i <= bytesRead - bytesAtTheTime; i += bytesAtTheTime)
                {
                    currentChar = (char)byteBuffer[i];

                    if (detectedEOL != NULL)
                    {
                        if (currentChar == detectedEOL)
                        { lineCount++; }

                        currentChar = (char)byteBuffer[i + 1];
                        if (currentChar == detectedEOL)
                        { lineCount++; }

                        currentChar = (char)byteBuffer[i + 2];
                        if (currentChar == detectedEOL)
                        { lineCount++; }

                        currentChar = (char)byteBuffer[i + 3];
                        if (currentChar == detectedEOL)
                        { lineCount++; }
                    }
                    else
                    {
                        if (currentChar == LF || currentChar == CR)
                        {
                            detectedEOL = currentChar;
                            lineCount++;
                        }
                        i -= bytesAtTheTime - 1;
                    }
                }

                for (; i < bytesRead; i++)
                {
                    currentChar = (char)byteBuffer[i];

                    if (detectedEOL != NULL)
                    {
                        if (currentChar == detectedEOL)
                        { lineCount++; }
                    }
                    else
                    {
                        if (currentChar == LF || currentChar == CR)
                        {
                            detectedEOL = currentChar;
                            lineCount++;
                        }
                    }
                }
            }

            if (currentChar != LF && currentChar != CR && currentChar != NULL)
            {
                lineCount++;
            }

            Assert.Equal(expectedNumberOfLines, lineCount);
        }
    }
}

