using Microsoft.AspNetCore.Http;

namespace Shared.Application.Authentication;

public interface IUserContext
{
    string UserId { get; }

    string? GetUserId(HttpContext context);
}
