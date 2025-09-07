namespace Realtime.Transport.Messaging.Factories;

/// 64-bit ID: [44 bits unix ms][20 bits sequence] — ~34y range, 1M ids/ms per node.
public static class IdGenerator
{
    private static long _lastMs;
    private static int _seq; // wraps at 2^20

    public static ulong NewId()
    {
        var nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var last = Interlocked.Read(ref _lastMs);
        if (nowMs == last)
        {
            var seq = Interlocked.Increment(ref _seq) & 0xFFFFF;
            return (ulong)nowMs << 20 | (uint)seq;
        }
        Interlocked.Exchange(ref _lastMs, nowMs);
        Interlocked.Exchange(ref _seq, 0);
        return (ulong)nowMs << 20;
    }
}

