using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Realtime.Transport.Http;
using Realtime.Transport.Messaging;
using Realtime.Transport.Messaging.Receiver;
using Realtime.Transport.Options;
using Realtime.Transport.Routing.Options;
using System.Threading.Channels;

namespace Realtime.Transport.Routing;

/// <summary>
/// Subscribes to IRealtimeGateway.OnFrame, routes command payloads to application handlers.
/// Preserves per-user ordering with N fixed partitions (bounded channels).
/// </summary>
public sealed class RealtimeEventRouter(
    IMessageReceiver messageReceiver,
    IServiceScopeFactory serviceScopeFactory,
    IOptions<RealtimeOptions> realtimeOptionsAccessor,
    IRealtimeCommandRouter realtimeCommandRouter,
    ILogger<RealtimeEventRouter> logger) : IHostedService
{
    private readonly RoutingOptions options = realtimeOptionsAccessor.Value.Routing;

    private record WorkItem(RealtimeContext Context, IRealtimeMessage Frame);

    private readonly List<Channel<WorkItem>> partitionChannels = [];
    private readonly List<Task> workerTasks = [];
    private CancellationTokenSource? routerCancellationTokenSource;
    private Action<RealtimeContext, IRealtimeMessage>? subscribedHandler;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        routerCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        var partitions = Math.Max(1, options.Partitions);
        var capacity = Math.Max(8, options.PartitionQueueCapacity);

        for (int i = 0; i < partitions; i++)
        {
            var channel = Channel.CreateBounded<WorkItem>(new BoundedChannelOptions(capacity)
            {
                SingleReader = true,
                SingleWriter = false,
                FullMode = BoundedChannelFullMode.Wait
            });

            partitionChannels.Add(channel);
            workerTasks.Add(Task.Run(() => WorkerAsync(channel.Reader, routerCancellationTokenSource.Token), cancellationToken));
        }

        subscribedHandler = OnMessageReceived;
        messageReceiver.OnMessageReceived += subscribedHandler;

        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (subscribedHandler is not null)
            messageReceiver.OnMessageReceived -= subscribedHandler;

        if (routerCancellationTokenSource is null)
            return;

        routerCancellationTokenSource.Cancel();

        foreach (var channel in partitionChannels)
            channel.Writer.TryComplete();

        try { await Task.WhenAll(workerTasks).ConfigureAwait(false); }
        catch { /* ignore on shutdown */ }
    }

    private void OnMessageReceived(RealtimeContext context, IRealtimeMessage frame)
    {
        if (frame is not IRealtimePayloadMessage payloadMessage)
            return;

        if (!realtimeCommandRouter.CanRoute(payloadMessage.Payload))
            return;

        var partitionIndex = ComputePartitionIndex(context.UserId, partitionChannels.Count);
        var workItem = new WorkItem(context, frame);

        _ = partitionChannels[partitionIndex].Writer.WriteAsync(workItem);
    }

    private async Task WorkerAsync(ChannelReader<WorkItem> reader, CancellationToken cancellationToken)
    {
        await foreach (var workItem in reader.ReadAllAsync(cancellationToken).ConfigureAwait(false))
        {
            try
            {
                if (workItem.Frame is not IRealtimePayloadMessage payloadMessage)
                    continue;

                using var scope = serviceScopeFactory.CreateScope();

                // Expose RealtimeContext to the application scope
                var realtimeContextAccessor = scope.ServiceProvider.GetRequiredService<IRealtimeContextAccessor>();
                realtimeContextAccessor.Current = workItem.Context;

                // Dispatch to application handler via router (resolved from ServiceProvider)
                await realtimeCommandRouter
                    .RouteAsync(scope.ServiceProvider, workItem.Context, (IPayload)payloadMessage.Payload, cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
            catch (Exception exception)
            {
                logger.LogError(exception,
                    "[realtime-router] Error handling message for user {UserId}, type={Type}",
                    workItem.Context.UserId, workItem.Frame.Header.Type);
            }
        }
    }

    private static int ComputePartitionIndex(string userId, int partitionCount)
    {
        // FNV-1a 32-bit hash
        unchecked
        {
            uint hash = 2166136261;
            for (int i = 0; i < userId.Length; i++)
                hash = (hash ^ userId[i]) * 16777619;
            return (int)(hash % (uint)partitionCount);
        }
    }
}
