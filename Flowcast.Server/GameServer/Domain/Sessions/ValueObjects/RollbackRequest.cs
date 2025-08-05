namespace Domain.Sessions.ValueObjects;

public record RollbackRequest(ulong CurrentServerFrame, string Reason);