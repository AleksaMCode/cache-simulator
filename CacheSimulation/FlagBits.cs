namespace CacheSimulation
{
    public enum Validity
    {
        Invalid = 0,
        Valid = 1
    }

    public class FlagBits
    {
        public Validity Valid { get; set; } = Validity.Invalid;
        public bool Dirty { get; set; } = false;
    }
}
