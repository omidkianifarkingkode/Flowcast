using Microsoft.AspNetCore.Http;

namespace Application.Realtime.Services
{
    public interface IUserConnectionManager
    {
        Task HandleConnectionAsync(HttpContext context, CancellationToken cancellationToken = default);
    }
}
