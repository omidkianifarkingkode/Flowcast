namespace Domain.Sessions.ValueObjects;

public record StateHashReport(ulong Frame, uint Hash, long PlayerId);
