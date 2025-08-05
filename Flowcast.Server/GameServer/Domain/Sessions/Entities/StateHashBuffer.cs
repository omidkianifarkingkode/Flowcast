using Domain.Sessions.ValueObjects;

namespace Domain.Sessions.Entities;

public class StateHashBuffer
{
    private readonly Dictionary<ulong, Dictionary<long, uint>> _hashes = new();

    public void Report(StateHashReport report) { /* ... */ }
    public bool IsFrameSynced(ulong frame) { return default; }
}


