using Backend.Data;
using Backend.DTOs.Requests;
using Backend.DTOs.Responses;
using Backend.Middlewares;
using Backend.Models;
using Backend.Repositories.Interfaces;
using Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public class AccountService :
    IAccountService
{
    private readonly AppDbContext _context;

    private readonly IAccountRepository
        _accountRepository;

    private readonly IConfiguration
        _configuration;

    public AccountService(
        AppDbContext context,
        IAccountRepository accountRepository,
        IConfiguration configuration)
    {
        _context = context;
        _accountRepository = accountRepository;
        _configuration = configuration;
    }

    public async Task<List<AccountResponse>>
        GetAllAsync(
            CancellationToken cancellationToken = default)
    {
        var accounts =
            await _accountRepository
                .GetAllAsync(
                    cancellationToken);

        return accounts
            .Select(Map)
            .ToList();
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

        return Map(account);
    }

    public async Task<AccountResponse>
        CreateAsync(
            CreateAccountRequest request,
            CancellationToken cancellationToken = default)
    {
        /*
         * Como o relacionamento Person -> Account
         * é 1:1, uma pessoa só pode ter uma conta.
         */
        var existingAccount =
            await _accountRepository
                .GetByPersonIdAsync(
                    request.PersonId,
                    cancellationToken);

        if (existingAccount is not null)
        {
            throw new BusinessException(
                "Esta pessoa já possui uma conta cadastrada.");
        }

        var person =
            await _context.Persons
                .FirstOrDefaultAsync(
                    x => x.Id == request.PersonId,
                    cancellationToken);

        if (person is null)
        {
            throw new NotFoundException(
                "Pessoa não encontrada.");
        }

        if (request.Balance < 0)
        {
            throw new BusinessException(
                "O saldo inicial não pode ser negativo.");
        }

        if (request.OverdraftLimit < 0)
        {
            throw new BusinessException(
                "O limite de cheque especial não pode ser negativo.");
        }

        await EnsureTransferLimitAsync(
            request.PersonId,
            cancellationToken);

        var account =
            new Account
            {
                PersonId =
                    request.PersonId,

                Balance =
                    request.Balance,

                OverdraftLimit =
                    request.OverdraftLimit,

                Status =
                    request.Status
            };

        _accountRepository.Add(
            account);

        await _context.SaveChangesAsync(
            cancellationToken);

        /*
         * Recarrega com Person incluída,
         * porque o response usa PersonName.
         */
        var createdAccount =
            await _accountRepository
                .GetByIdAsync(
                    account.Id,
                    cancellationToken)
            ?? throw new NotFoundException(
                "Conta criada, mas não foi possível carregá-la.");

        return Map(
            createdAccount);
    }

    public async Task<AccountResponse>
        UpdateAsync(
            int id,
            UpdateAccountRequest request,
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

        if (request.OverdraftLimit < 0)
        {
            throw new BusinessException(
                "O limite de cheque especial não pode ser negativo.");
        }

        var person =
            await _context.Persons
                .FirstOrDefaultAsync(
                    x => x.Id == request.PersonId,
                    cancellationToken);

        if (person is null)
        {
            throw new NotFoundException(
                "Pessoa não encontrada.");
        }

        var existingAccount =
            await _accountRepository
                .GetByPersonIdAsync(
                    request.PersonId,
                    cancellationToken);

        if (existingAccount is not null &&
            existingAccount.Id != id)
        {
            throw new BusinessException(
                "Esta pessoa já possui uma conta cadastrada.");
        }

        account.PersonId =
            request.PersonId;

        account.Person =
            person;

        account.OverdraftLimit =
            request.OverdraftLimit;

        account.Status =
            request.Status;

        await EnsureTransferLimitAsync(
            request.PersonId,
            cancellationToken);

        try
        {
            await _context.SaveChangesAsync(
                cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new BusinessException(
                "A conta foi alterada por outra operação. Atualize os dados e tente novamente.");
        }

        return Map(account);
    }

    public async Task DeleteAsync(
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

        await using var transaction =
            await _context.Database
                .BeginTransactionAsync(
                    cancellationToken);

        await AccountDeletionHelper
            .DeleteDependenciesAsync(
                _context,
                [id],
                cancellationToken);

        _accountRepository.Remove(
            account);

        await _context.SaveChangesAsync(
            cancellationToken);

        await transaction.CommitAsync(
            cancellationToken);
    }

    private async Task EnsureTransferLimitAsync(
        int personId,
        CancellationToken cancellationToken)
    {
        var hasTransferLimit =
            _context.TransferLimits
                .Local
                .Any(x => x.PersonId == personId) ||
            await _context.TransferLimits
                .AnyAsync(
                    x => x.PersonId == personId,
                    cancellationToken);

        if (hasTransferLimit)
        {
            return;
        }

        _context.TransferLimits.Add(
            new TransferLimit
            {
                PersonId = personId,
                DayHourlyAmountLimit =
                    _configuration.GetValue(
                        "TransferRules:DefaultDayHourlyAmountLimit",
                        10000m),
                DayHourlyAttemptLimit =
                    _configuration.GetValue(
                        "TransferRules:DefaultDayHourlyAttemptLimit",
                        10),
                NightHourlyAmountLimit =
                    _configuration.GetValue(
                        "TransferRules:DefaultNightHourlyAmountLimit",
                        1000m),
                NightHourlyAttemptLimit =
                    _configuration.GetValue(
                        "TransferRules:DefaultNightHourlyAttemptLimit",
                        3)
            });
    }

    private static AccountResponse Map(
        Account account)
    {
        return new AccountResponse
        {
            Id =
                account.Id,

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