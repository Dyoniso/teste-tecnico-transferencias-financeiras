using Backend.DTOs.Requests;
using Backend.DTOs.Responses;

namespace Backend.Services.Interfaces;

public interface ITransferService
{
    Task<TransferResponse> TransferAsync(
        CreateTransferRequest request,
        string? idempotencyKey,
        CancellationToken cancellationToken = default);

    Task<TransferResponse> ScheduleAsync(
        ScheduleTransferRequest request,
        string? idempotencyKey,
        CancellationToken cancellationToken = default);

    Task<TransferResponse> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task CancelAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task ProcessScheduledAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}