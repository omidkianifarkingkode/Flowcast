using Microsoft.AspNetCore.Http;

namespace Application.Abstractions.Authentication;

public interface IUserContext
{
    Guid UserId { get; }

    Guid? GetUserId(HttpContext context);
}
