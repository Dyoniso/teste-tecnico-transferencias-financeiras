using Backend.Services.Interfaces;
using Confluent.Kafka;

namespace Backend.Services.Kafka;

public class KafkaProducer :
    IKafkaProducer,
    IDisposable
{
    private readonly IProducer<string, string>
        _producer;

    public KafkaProducer(
        IConfiguration configuration)
    {
        var bootstrapServers =
            configuration[
                "Kafka:BootstrapServers"];

        if (string.IsNullOrWhiteSpace(
                bootstrapServers))
        {
            throw new InvalidOperationException(
                "Kafka:BootstrapServers não foi configurado.");
        }

        var config =
            new ProducerConfig
            {
                BootstrapServers =
                    bootstrapServers,

                Acks =
                    Acks.All
            };

        _producer =
            new ProducerBuilder<string, string>(
                    config)
                .Build();
    }

    public async Task PublishRawAsync(
        string topic,
        string payload,
        CancellationToken cancellationToken = default)
    {
        await _producer.ProduceAsync(
            topic,
            new Message<string, string>
            {
                Key =
                    Guid.NewGuid()
                        .ToString(),

                Value =
                    payload
            },
            cancellationToken);
    }

    public void Dispose()
    {
        _producer.Flush(
            TimeSpan.FromSeconds(5));

        _producer.Dispose();
    }
}