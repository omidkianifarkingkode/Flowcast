using Domain.Sessions;

namespace Application.Sessions.Shared;

public record SessionSummaryDto(string SessionId, string Mode, SessionStatus Status, int PlayerCount, DateTime CreatedAtUtc);
