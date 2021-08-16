namespace CacheSimulation
{
    public enum Validity
    {
        Valid = 0,
        Invalid = 1
    }

    public class FlagBits
    {
        public Validity Valid { get; set; } = Validity.Invalid;
        public bool Dirty { get; set; } = false;
    }
}
