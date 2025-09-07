using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Realtime.Transport.Gateway;

public static class WebSocketMiddleware
{
    public static WebApplication UseRealtime(
        this WebApplication app)
    {
        app.UseWebSockets();

        app.Map("/ws", async context =>
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                var wsHandler = context.RequestServices.GetRequiredService<WebSocketHandler>();
                await wsHandler.HandleConnectionAsync(context, context.RequestAborted);
            }
            else
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
        });

        return app;
    }
}
