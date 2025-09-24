using Session.Domain;
using SharedKernel.Primitives;

namespace Session.Contracts;

public interface ISessionNotifier
{
    Task SessionCreated(SessionEntity session, CancellationToken ct);
    Task SessionCreateFail(string reasonCode, string message, PlayerId[] recipients, CancellationToken ct);
}
