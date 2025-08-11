using Infrastructure.Realtime;

namespace Presentation.Extensions
{
    public static class WebSocketExtensions
    {
        public static WebApplication UseWebSocketsAndMapEndpoint(
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
}
