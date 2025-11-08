namespace Flowcast.Commons
{
    public interface IIdGenerator
    {
        long Generate();
        void ResetTo(long nextId);
        void Reset();
    }
}
