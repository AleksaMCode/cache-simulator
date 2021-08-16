namespace CacheSimulation
{
    public class FlagBits
    {
        public int valid;
        public int dirty;

        public FlagBits()
        {
            valid = dirty = 0;
        }
    }
}
