namespace Flowcast.Commons
{
    public class SequentialIdGenerator : IIdGenerator
    {
        private long _nextId;

        public SequentialIdGenerator(long start = 1)
        {
            _nextId = start;
        }

        public long Generate()
        {
            return _nextId++;
        }

        public void Reset()
        {
            _nextId = 1;
        }

        public void ResetTo(long nextId)
        {
            _nextId = nextId;
        }
    }
}
