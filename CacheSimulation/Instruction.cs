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
        public readonly string Data;

        public Instruction(MemoryRelatedInstructions instructionType, string memoryAddress, string data = null)
        {
            InstructionType = instructionType;
            MemoryAddress = memoryAddress;
            Data = data;
        }
    }
}
