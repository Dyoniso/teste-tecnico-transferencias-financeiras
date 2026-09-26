using Backend.Data;
using Backend.Models;
using Backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories;

public class AccountRepository :
    IAccountRepository
{
    private readonly AppDbContext _context;

    public AccountRepository(
        AppDbContext context)
    {
        _context = context;
    }

    public Task<List<Account>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return _context.Accounts
            .Include(x => x.Person)
            .OrderBy(x => x.Id)
            .ToListAsync(cancellationToken);
    }

    public Task<Account?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return _context.Accounts
            .Include(x => x.Person)
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);
    }

    public Task<Account?> GetByPersonIdAsync(
        int personId,
        CancellationToken cancellationToken = default)
    {
        return _context.Accounts
            .Include(x => x.Person)
            .FirstOrDefaultAsync(
                x => x.PersonId == personId,
                cancellationToken);
    }

    public void Add(
        Account account)
    {
        _context.Accounts.Add(
            account);
    }

    public void Remove(
        Account account)
    {
        _context.Accounts.Remove(
            account);
    }
}