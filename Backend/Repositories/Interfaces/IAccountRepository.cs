using Backend.Models;

namespace Backend.Repositories.Interfaces;

public interface IAccountRepository
{
    Task<List<Account>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<Account?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<Account?> GetByPersonIdAsync(
        int personId,
        CancellationToken cancellationToken = default);

    void Add(
        Account account);

    void Remove(
        Account account);
}