namespace Domain.Sessions;

public class CommandHistory
{
    private readonly Dictionary<ulong, List<IGameCommand>> _history = new();

    public void AddCommand(IGameCommand command) { /* ... */ }
    public IReadOnlyList<IGameCommand> GetFrameCommands(ulong frame) { return default; }
    public IEnumerable<IGameCommand> GetCommandsFromFrame(ulong startFrame) { return default; }
}

