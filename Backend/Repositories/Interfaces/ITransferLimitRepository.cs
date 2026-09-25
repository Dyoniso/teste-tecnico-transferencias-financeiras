using Backend.Models;

namespace Backend.Repositories.Interfaces;

public interface ITransferLimitRepository
{
    Task<TransferLimit?> GetByPersonIdAsync(
        int personId,
        CancellationToken cancellationToken = default);
}