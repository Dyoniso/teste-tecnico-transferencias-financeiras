using Backend.Data;
using Backend.Models;
using Backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories;

public class TransferAttemptRepository :
    ITransferAttemptRepository
{
    private readonly AppDbContext _context;

    public TransferAttemptRepository(
        AppDbContext context)
    {
        _context = context;
    }

    public Task<TransferAttempt?> GetByIdAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        return _context.TransferAttempts
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);
    }

    public Task<int> CountSinceAsync(
        int accountId,
        DateTime since,
        CancellationToken cancellationToken = default)
    {
        return _context.TransferAttempts
            .CountAsync(
                x =>
                    x.AccountId == accountId &&
                    x.CreatedAt >= since,
                cancellationToken);
    }

    public Task<DateTime?> GetOldestCreatedAtSinceAsync(
        int accountId,
        DateTime since,
        CancellationToken cancellationToken = default)
    {
        return _context.TransferAttempts
            .Where(x =>
                x.AccountId == accountId &&
                x.CreatedAt >= since)
            .MinAsync(
                x => (DateTime?)x.CreatedAt,
                cancellationToken);
    }

    public void Add(
        TransferAttempt attempt)
    {
        _context.TransferAttempts.Add(
            attempt);
    }
}