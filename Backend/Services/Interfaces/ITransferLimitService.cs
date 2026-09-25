namespace Backend.Services.Interfaces;

public interface ITransferLimitService
{
    Task ValidateAsync(
        int accountId,
        int personId,
        decimal amount,
        CancellationToken cancellationToken = default);
}