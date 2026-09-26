using Backend.Models;

namespace Backend.Repositories.Interfaces;

public interface IPersonRepository
{
    Task<List<Person>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<Person?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<Person?> GetByDocumentAsync(
        string document,
        CancellationToken cancellationToken = default);

    Task<Person?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken = default);

    void Add(
        Person person);

    void Remove(
        Person person);
}