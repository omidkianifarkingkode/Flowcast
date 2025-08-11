using Microsoft.AspNetCore.Http;

namespace Application.Abstractions.Realtime
{
    public interface IUserConnectionManager
    {
        Task HandleConnectionAsync(HttpContext context, CancellationToken cancellationToken = default);
    }
}
