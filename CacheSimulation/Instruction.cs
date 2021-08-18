namespace CacheSimulation
{
    public enum MemoryRelatedInstructions
    {
        Load,
        Store
    }

    public class Instruction
    {
        private readonly MemoryRelatedInstructions instructionType;
        private string memoryAddress;
        private string data;

        public Instruction(MemoryRelatedInstructions instructionType, string memoryAddress, string data = null)
        {
            this.instructionType = instructionType;
            this.memoryAddress = memoryAddress;
            this.data = data;
        }
    }
}
