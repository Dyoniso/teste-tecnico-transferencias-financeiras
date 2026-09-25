using Backend.Data;
using Backend.Models;
using Backend.Models.Enums;
using Backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories;

public class TransferRepository :
    ITransferRepository
{
    private readonly AppDbContext _context;

    public TransferRepository(
        AppDbContext context)
    {
        _context = context;
    }

    public Task<Transfer?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return _context.Transfers
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);
    }

    public Task<Transfer?>
        GetByIdempotencyKeyAsync(
            string key,
            CancellationToken cancellationToken = default)
    {
        return _context.Transfers
            .FirstOrDefaultAsync(
                x => x.IdempotencyKey == key,
                cancellationToken);
    }

    public Task<List<Transfer>>
        GetDueScheduledAsync(
            DateTime now,
            int limit,
            CancellationToken cancellationToken = default)
    {
        return _context.Transfers
            .Where(x =>
                x.Status ==
                    TransferStatus.Scheduled &&
                x.ScheduledAt <= now &&
                x.DispatchRequestedAt == null)
            .OrderBy(x => x.ScheduledAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<decimal>
        GetCompletedAmountSinceAsync(
            int accountId,
            DateTime since,
            CancellationToken cancellationToken = default)
    {
        return await _context.Transfers
            .Where(x =>
                x.SourceAccountId == accountId &&
                x.Status ==
                    TransferStatus.Completed &&
                x.ProcessedAt >= since)
            .SumAsync(
                x => (decimal?)x.Amount,
                cancellationToken)
            ?? 0;
    }

    public void Add(
        Transfer transfer)
    {
        _context.Transfers.Add(transfer);
    }
}