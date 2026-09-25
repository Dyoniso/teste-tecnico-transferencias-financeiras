using Backend.Data;
using Backend.Repositories.Interfaces;
using Backend.Services.Interfaces;

namespace Backend.Services.Workers;

public class OutboxPublisherWorker :
    BackgroundService
{
    private readonly IServiceScopeFactory
        _scopeFactory;

    private readonly ILogger<OutboxPublisherWorker>
        _logger;

    public OutboxPublisherWorker(
        IServiceScopeFactory scopeFactory,
        ILogger<OutboxPublisherWorker> logger)
    {
        _scopeFactory =
            scopeFactory;

        _logger =
            logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        using var timer =
            new PeriodicTimer(
                TimeSpan.FromSeconds(3));

        while (await timer.WaitForNextTickAsync(
                   stoppingToken))
        {
            try
            {
                await ProcessAsync(
                    stoppingToken);
            }
            catch (OperationCanceledException)
                when (stoppingToken
                    .IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Erro no processamento do Outbox.");
            }
        }
    }

    private async Task ProcessAsync(
        CancellationToken cancellationToken)
    {
        using var scope =
            _scopeFactory.CreateScope();

        var context =
            scope.ServiceProvider
                .GetRequiredService<
                    AppDbContext>();

        var repository =
            scope.ServiceProvider
                .GetRequiredService<
                    IOutboxRepository>();

        var producer =
            scope.ServiceProvider
                .GetRequiredService<
                    IKafkaProducer>();

        var messages =
            await repository.GetPendingAsync(
                100,
                cancellationToken);

        foreach (var message in messages)
        {
            try
            {
                await producer.PublishRawAsync(
                    "scheduled-transfers",
                    message.Payload,
                    cancellationToken);

                message.ProcessedAt =
                    DateTime.UtcNow;

                message.Error =
                    null;
            }
            catch (Exception ex)
            {
                message.RetryCount++;

                message.Error =
                    ex.Message;

                _logger.LogError(
                    ex,
                    "Erro ao publicar mensagem Outbox {OutboxId}.",
                    message.Id);
            }
        }

        if (messages.Count > 0)
        {
            await context.SaveChangesAsync(
                cancellationToken);
        }
    }
}