using Backend.DTOs.Requests;
using Backend.DTOs.Responses;

namespace Backend.Services.Interfaces;

public interface IPersonService
{
    Task<List<PersonResponse>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<PersonResponse> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<PersonResponse> CreateAsync(
        CreatePersonRequest request,
        CancellationToken cancellationToken = default);

    Task<PersonResponse> UpdateAsync(
        int id,
        UpdatePersonRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        int id,
        CancellationToken cancellationToken = default);
}