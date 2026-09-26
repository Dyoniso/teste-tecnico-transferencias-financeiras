using Backend.Data;
using Backend.Models;
using Backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories;

public class PersonRepository :
    IPersonRepository
{
    private readonly AppDbContext _context;

    public PersonRepository(
        AppDbContext context)
    {
        _context = context;
    }

    public Task<List<Person>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return _context.Persons
            .Include(x => x.Account)
            .OrderBy(x => x.Name)
            .ToListAsync(
                cancellationToken);
    }

    public Task<Person?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return _context.Persons
            .Include(x => x.Account)
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);
    }

    public Task<Person?> GetByDocumentAsync(
        string document,
        CancellationToken cancellationToken = default)
    {
        return _context.Persons
            .Include(x => x.Account)
            .FirstOrDefaultAsync(
                x => x.Document == document,
                cancellationToken);
    }

    public Task<Person?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        return _context.Persons
            .Include(x => x.Account)
            .FirstOrDefaultAsync(
                x => x.Email == email,
                cancellationToken);
    }

    public void Add(
        Person person)
    {
        _context.Persons.Add(
            person);
    }

    public void Remove(
        Person person)
    {
        _context.Persons.Remove(
            person);
    }
}