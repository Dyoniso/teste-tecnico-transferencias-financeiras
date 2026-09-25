using Backend.DTOs.Responses;

namespace Backend.Services.Interfaces;

public interface IAccountService
{
    Task<AccountResponse> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);
}