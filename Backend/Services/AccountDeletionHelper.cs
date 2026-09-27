using Backend.Data;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

internal static class AccountDeletionHelper
{
    public static async Task DeleteDependenciesAsync(
        AppDbContext context,
        IReadOnlyCollection<int> accountIds,
        CancellationToken cancellationToken)
    {
        var transferIds =
            context.Transfers
                .Where(x =>
                    accountIds.Contains(x.SourceAccountId) ||
                    accountIds.Contains(x.DestinationAccountId))
                .Select(x => x.Id);

        await context.TransferAttempts
            .Where(x =>
                accountIds.Contains(x.AccountId) ||
                (x.TransferId.HasValue &&
                 transferIds.Contains(x.TransferId.Value)))
            .ExecuteDeleteAsync(
                cancellationToken);

        await context.Transfers
            .Where(x =>
                accountIds.Contains(x.SourceAccountId) ||
                accountIds.Contains(x.DestinationAccountId))
            .ExecuteDeleteAsync(
                cancellationToken);
    }
}