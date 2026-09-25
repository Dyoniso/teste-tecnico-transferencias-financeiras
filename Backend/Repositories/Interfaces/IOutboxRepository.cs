using Backend.Models;

namespace Backend.Repositories.Interfaces;

public interface IOutboxRepository
{
    Task<List<OutboxMessage>>
        GetPendingAsync(
            int limit,
            CancellationToken cancellationToken = default);

    void Add(
        OutboxMessage message);
}