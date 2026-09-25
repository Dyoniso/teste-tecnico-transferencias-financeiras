using Backend.Models;

namespace Backend.Repositories.Interfaces;

public interface ITransferRepository
{
    Task<Transfer?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<Transfer?> GetByIdempotencyKeyAsync(
        string key,
        CancellationToken cancellationToken = default);

    Task<List<Transfer>> GetDueScheduledAsync(
        DateTime now,
        int limit,
        CancellationToken cancellationToken = default);

    Task<decimal> GetCompletedAmountSinceAsync(
        int accountId,
        DateTime since,
        CancellationToken cancellationToken = default);

    void Add(Transfer transfer);
}