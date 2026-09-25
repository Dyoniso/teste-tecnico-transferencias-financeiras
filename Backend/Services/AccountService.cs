using Backend.DTOs.Responses;
using Backend.Middlewares;
using Backend.Repositories.Interfaces;
using Backend.Services.Interfaces;

namespace Backend.Services;

public class AccountService :
    IAccountService
{
    private readonly IAccountRepository
        _accountRepository;

    public AccountService(
        IAccountRepository accountRepository)
    {
        _accountRepository =
            accountRepository;
    }

    public async Task<AccountResponse>
        GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
    {
        var account =
            await _accountRepository
                .GetByIdAsync(
                    id,
                    cancellationToken);

        if (account is null)
        {
            throw new NotFoundException(
                "Conta não encontrada.");
        }

        return new AccountResponse
        {
            Id = account.Id,

            PersonId =
                account.PersonId,

            PersonName =
                account.Person.Name,

            Balance =
                account.Balance,

            OverdraftLimit =
                account.OverdraftLimit,

            AvailableBalance =
                account.Balance +
                account.OverdraftLimit,

            Status =
                account.Status.ToString()
        };
    }
}