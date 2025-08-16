namespace Application.Realtime.Messaging;

public static class IdGenerator
{
    private static long _counter = 0;

    public static ulong NewId()
    {
        return (ulong)Interlocked.Increment(ref _counter);
    }
}
