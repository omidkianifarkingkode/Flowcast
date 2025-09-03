namespace Realtime.TestHost.Authentication;

public interface IUserContext
{
    Guid UserId { get; }

    Guid? GetUserId(HttpContext context);
}
