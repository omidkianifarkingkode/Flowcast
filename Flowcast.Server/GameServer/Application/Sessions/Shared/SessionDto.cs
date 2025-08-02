using Domain.Sessions;

namespace Application.Sessions.Shared;

public record SessionDto(string SessionId, string Mode, SessionStatus Status, DateTime CreatedAtUtc, List<PlayerDto> Players);
