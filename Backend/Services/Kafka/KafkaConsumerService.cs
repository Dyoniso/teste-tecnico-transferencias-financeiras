using System.Text.Json;
using Backend.Services.Interfaces;
using Confluent.Kafka;

namespace Backend.Services.Kafka;

public class KafkaConsumerService :
    BackgroundService
{
    private readonly IServiceScopeFactory
        _scopeFactory;

    private readonly IConfiguration
        _configuration;

    private readonly ILogger<KafkaConsumerService>
        _logger;

    public KafkaConsumerService(
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        ILogger<KafkaConsumerService> logger)
    {
        _scopeFactory =
            scopeFactory;

        _configuration =
            configuration;

        _logger =
            logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        var bootstrapServers =
            _configuration[
                "Kafka:BootstrapServers"];

        if (string.IsNullOrWhiteSpace(
                bootstrapServers))
        {
            throw new InvalidOperationException(
                "Kafka:BootstrapServers não foi configurado.");
        }

        var config =
            new ConsumerConfig
            {
                BootstrapServers =
                    bootstrapServers,

                GroupId =
                    "scheduled-transfer-workers",

                AutoOffsetReset =
                    AutoOffsetReset.Earliest,

                EnableAutoCommit =
                    false
            };

        using var consumer =
            new ConsumerBuilder<string, string>(
                    config)
                .Build();

        consumer.Subscribe(
            "scheduled-transfers");

        try
        {
            while (!stoppingToken
                       .IsCancellationRequested)
            {
                var result =
                    consumer.Consume(
                        stoppingToken);

                try
                {
                    var message =
                        JsonSerializer.Deserialize<
                            ScheduledTransferMessage>(
                            result.Message.Value);

                    if (message is null)
                    {
                        consumer.Commit(
                            result);

                        continue;
                    }

                    using var scope =
                        _scopeFactory.CreateScope();

                    var service =
                        scope.ServiceProvider
                            .GetRequiredService<
                                ITransferService>();

                    await service
                        .ProcessScheduledAsync(
                            message.TransferId,
                            stoppingToken);

                    /*
                     * Commit somente depois do
                     * processamento.
                     */
                    consumer.Commit(
                        result);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Erro processando mensagem Kafka.");

                    /*
                     * Sem commit.
                     *
                     * Kafka poderá entregar
                     * novamente.
                     */
                    await Task.Delay(
                        TimeSpan.FromSeconds(2),
                        stoppingToken);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // aplicação encerrando
        }
        finally
        {
            consumer.Close();
        }
    }
}