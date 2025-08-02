using Domain.Players;

namespace Application.Sessions.Shared;

public record PlayerDto(long Id, string DisplayName, PlayerStatus Status);
