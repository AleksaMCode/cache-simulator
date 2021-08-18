namespace CacheSimulation
{
    public enum MemoryRelatedInstructions
    {
        Load,
        Store
    }

    public class Instruction
    {
        public readonly MemoryRelatedInstructions InstructionType;
        public readonly string MemoryAddress;
        public readonly int DataSize;
        public readonly string Data;

        public Instruction(MemoryRelatedInstructions instructionType, string memoryAddress, int dataSize = 0, string data = null)
        {
            InstructionType = instructionType;
            MemoryAddress = memoryAddress;
            DataSize = dataSize;
            Data = data;
        }
    }
}
