using Microsoft.AspNetCore.SignalR;

namespace Domain.Users.Services
{
    public interface IUserContextService
    {
        Guid? GetUserId(HubCallerContext context);
    }
}