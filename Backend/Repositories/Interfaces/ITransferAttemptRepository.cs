using Backend.Models;

namespace Backend.Repositories.Interfaces;

public interface ITransferAttemptRepository
{
    Task<TransferAttempt?> GetByIdAsync(
        long id,
        CancellationToken cancellationToken = default);

    Task<int> CountSinceAsync(
        int accountId,
        DateTime since,
        CancellationToken cancellationToken = default);

    void Add(
        TransferAttempt attempt);
}