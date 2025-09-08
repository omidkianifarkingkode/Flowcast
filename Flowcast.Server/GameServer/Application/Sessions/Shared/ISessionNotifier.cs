using Domain.Sessions;

namespace Application.Sessions.Shared;

public interface ISessionNotifier
{
    Task SessionCreated(Session session, CancellationToken ct);
    Task SessionCreateFail(string reasonCode, string message, PlayerId[] recipients, CancellationToken ct);
}
