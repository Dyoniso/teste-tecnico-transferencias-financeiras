using Backend.Models;

namespace Backend.Repositories.Interfaces;

public interface IAccountRepository
{
    Task<Account?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);
}