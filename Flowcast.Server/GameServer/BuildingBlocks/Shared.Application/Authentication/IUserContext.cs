using Microsoft.AspNetCore.Http;

namespace Shared.Application.Authentication;

public interface IUserContext
{
    Guid UserId { get; }

    Guid? GetUserId(HttpContext context);
}
