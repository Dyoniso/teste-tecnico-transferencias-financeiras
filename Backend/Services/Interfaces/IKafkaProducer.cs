namespace Backend.Services.Interfaces;

public interface IKafkaProducer
{
    Task PublishRawAsync(
        string topic,
        string payload,
        CancellationToken cancellationToken = default);
}