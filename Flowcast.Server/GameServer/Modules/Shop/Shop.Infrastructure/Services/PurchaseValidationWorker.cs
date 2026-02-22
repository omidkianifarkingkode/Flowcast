using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shared.Application.Messaging;
using Shop.Application.Commands;

namespace Shop.Infrastructure.Services;

public class PurchaseValidationWorker(
    IServiceProvider serviceProvider,
    ILogger<PurchaseValidationWorker> logger) : BackgroundService
{

    // Configurable (can later come from IOptions or appsettings)
    private readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(30);
    private readonly int _defaultBatchSize = 50;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Purchase Validation Worker started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunValidationBatchAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // normal shutdown
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error in purchase validation worker loop");
                // optional: backoff
                await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
            }

            await Task.Delay(_pollingInterval, stoppingToken);
        }

        logger.LogInformation("Purchase Validation Worker stopped");
    }

    private async Task RunValidationBatchAsync(CancellationToken ct)
    {
        using var scope = serviceProvider.CreateScope();
        var services = scope.ServiceProvider;

        var commandHandler = services.GetRequiredService<ICommandHandler<ValidatePurchasesCommand>>();

        var command = new ValidatePurchasesCommand(BatchSize: _defaultBatchSize);

        var result = await commandHandler.Handle(command, ct);

        if (result.IsFailure)
        {
            logger.LogWarning("Batch validation command failed: {Error}", result.Error?.Description);
        }
    }
}