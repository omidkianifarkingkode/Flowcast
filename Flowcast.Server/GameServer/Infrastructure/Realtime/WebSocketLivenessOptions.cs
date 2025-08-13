namespace Infrastructure.Realtime;

public class WebSocketLivenessOptions
{
    public int PingIntervalSeconds { get; set; } = 15;
    public int TimeoutSeconds { get; set; } = 60;
}
