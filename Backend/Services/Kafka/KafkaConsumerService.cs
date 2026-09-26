using System.Text.Json;
using Backend.Services.Interfaces;
using Confluent.Kafka;

namespace Backend.Services.Kafka;

public class KafkaConsumerService :
    BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<KafkaConsumerService> _logger;

    public KafkaConsumerService(
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        ILogger<KafkaConsumerService> logger)
    {
        _scopeFactory = scopeFactory;
        _configuration = configuration;
        _logger = logger;
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
            _logger.LogError(
                "Kafka:BootstrapServers não foi configurado.");

            return;
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
                .SetErrorHandler(
                    (_, error) =>
                    {
                        _logger.LogWarning(
                            "Kafka Consumer: {Reason}",
                            error.Reason);
                    })
                .Build();

        consumer.Subscribe(
            "scheduled-transfers");

        _logger.LogInformation(
            "Kafka Consumer inscrito no tópico scheduled-transfers.");

        try
        {
            while (!stoppingToken
                       .IsCancellationRequested)
            {
                try
                {
                    var result =
                        consumer.Consume(
                            stoppingToken);

                    if (result?.Message?.Value
                        is null)
                    {
                        continue;
                    }

                    var message =
                        JsonSerializer.Deserialize<
                            ScheduledTransferMessage>(
                            result.Message.Value);

                    if (message is null)
                    {
                        _logger.LogWarning(
                            "Mensagem Kafka inválida.");

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

                    consumer.Commit(
                        result);
                }
                catch (ConsumeException ex)
                {
                    _logger.LogWarning(
                        ex,
                        "Erro temporário ao consumir Kafka. Nova tentativa em 5 segundos.");

                    await Task.Delay(
                        TimeSpan.FromSeconds(5),
                        stoppingToken);
                }
                catch (JsonException ex)
                {
                    _logger.LogError(
                        ex,
                        "Mensagem Kafka com JSON inválido.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Erro ao processar mensagem Kafka.");

                    await Task.Delay(
                        TimeSpan.FromSeconds(5),
                        stoppingToken);
                }
            }
        }
        catch (OperationCanceledException)
            when (stoppingToken
                .IsCancellationRequested)
        {
            _logger.LogInformation(
                "Kafka Consumer finalizado.");
        }
        finally
        {
            consumer.Close();
        }
    }
}