namespace Application.Abstractions.Realtime;

public interface IUserConnectionSender
{
    Task SendToUserAsync(Guid userId, string message, CancellationToken cancellationToken = default);
}
