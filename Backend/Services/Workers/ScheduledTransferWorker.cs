using System.Text.Json;
using Backend.Data;
using Backend.Models;
using Backend.Repositories.Interfaces;
using Backend.Services.Kafka;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.Workers;

public class ScheduledTransferWorker :
    BackgroundService
{
    private readonly IServiceScopeFactory
        _scopeFactory;

    private readonly ILogger<ScheduledTransferWorker>
        _logger;

    public ScheduledTransferWorker(
        IServiceScopeFactory scopeFactory,
        ILogger<ScheduledTransferWorker> logger)
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
                TimeSpan.FromSeconds(5));

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
                    "Erro ao verificar transferências agendadas.");
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

        var transferRepository =
            scope.ServiceProvider
                .GetRequiredService<
                    ITransferRepository>();

        var outboxRepository =
            scope.ServiceProvider
                .GetRequiredService<
                    IOutboxRepository>();

        var now =
            DateTime.UtcNow;

        var transfers =
            await transferRepository
                .GetDueScheduledAsync(
                    now,
                    100,
                    cancellationToken);

        if (transfers.Count == 0)
        {
            return;
        }

        await using var transaction =
            await context.Database
                .BeginTransactionAsync(
                    cancellationToken);

        try
        {
            foreach (var transfer
                     in transfers)
            {
                transfer.DispatchRequestedAt =
                    now;

                var payload =
                    JsonSerializer.Serialize(
                        new ScheduledTransferMessage(
                            transfer.Id));

                outboxRepository.Add(
                    new OutboxMessage
                    {
                        Id =
                            Guid.NewGuid(),

                        Type =
                            "ScheduledTransfer",

                        Payload =
                            payload,

                        CreatedAt =
                            DateTime.UtcNow
                    });
            }

            await context.SaveChangesAsync(
                cancellationToken);

            await transaction.CommitAsync(
                cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync(
                cancellationToken);

            /*
             * Outro worker pode ter pego
             * o mesmo registro.
             *
             * No próximo ciclo ele tenta novamente.
             */
        }
    }
}