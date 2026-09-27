using Backend.Data;
using Backend.DTOs.Requests;
using Backend.DTOs.Responses;
using Backend.Middlewares;
using Backend.Models;
using Backend.Models.Enums;
using Backend.Repositories.Interfaces;
using Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public class TransferService :
    ITransferService
{
    private const int MaxConcurrencyRetries = 3;

    private readonly AppDbContext _context;

    private readonly IAccountRepository
        _accountRepository;

    private readonly ITransferRepository
        _transferRepository;

    private readonly ITransferAttemptRepository
        _attemptRepository;

    private readonly ITransferLimitService
        _limitService;

    public TransferService(
        AppDbContext context,
        IAccountRepository accountRepository,
        ITransferRepository transferRepository,
        ITransferAttemptRepository attemptRepository,
        ITransferLimitService limitService)
    {
        _context =
            context;

        _accountRepository =
            accountRepository;

        _transferRepository =
            transferRepository;

        _attemptRepository =
            attemptRepository;

        _limitService =
            limitService;
    }

    public async Task<TransferResponse>
        TransferAsync(
            CreateTransferRequest request,
            string? idempotencyKey,
            CancellationToken cancellationToken = default)
    {
        ValidateBasicRules(
            request.SourceAccountId,
            request.DestinationAccountId,
            request.Amount);

        var existing =
            await GetExistingIdempotentTransferAsync(
                idempotencyKey,
                cancellationToken);

        if (existing is not null)
        {
            return Map(existing);
        }

        /*
         * Precisamos garantir que as contas existem
         * antes de criar Transfer, pois existem FKs.
         */
        var source =
            await _accountRepository
                .GetByIdAsync(
                    request.SourceAccountId,
                    cancellationToken);

        if (source is null)
        {
            throw new NotFoundException(
                "Conta de origem não encontrada.");
        }

        var destination =
            await _accountRepository
                .GetByIdAsync(
                    request.DestinationAccountId,
                    cancellationToken);

        if (destination is null)
        {
            throw new NotFoundException(
                "Conta de destino não encontrada.");
        }

        var transfer =
            new Transfer
            {
                Id = Guid.NewGuid(),

                SourceAccountId =
                    request.SourceAccountId,

                DestinationAccountId =
                    request.DestinationAccountId,

                Amount =
                    request.Amount,

                Status =
                    TransferStatus.Processing,

                CreatedAt =
                    DateTime.UtcNow,

                IdempotencyKey =
                    NormalizeIdempotencyKey(
                        idempotencyKey)
            };

        _transferRepository.Add(
            transfer);

        try
        {
            await _context.SaveChangesAsync(
                cancellationToken);
        }
        catch (DbUpdateException)
            when (!string.IsNullOrWhiteSpace(
                idempotencyKey))
        {
            /*
             * Pode acontecer de duas requisições
             * iguais chegarem simultaneamente.
             *
             * O índice UNIQUE garante a idempotência
             * também no banco.
             */
            _context.ChangeTracker.Clear();

            var concurrentExisting =
                await GetExistingIdempotentTransferAsync(
                    idempotencyKey,
                    cancellationToken);

            if (concurrentExisting is not null)
            {
                return Map(
                    concurrentExisting);
            }

            throw;
        }

        await ExecuteTransferAsync(
            transfer.Id,
            cancellationToken);

        var result =
            await _transferRepository
                .GetByIdAsync(
                    transfer.Id,
                    cancellationToken)
            ?? throw new NotFoundException(
                "Transferência não encontrada.");

        return Map(result);
    }

    public async Task<TransferResponse>
        ScheduleAsync(
            ScheduleTransferRequest request,
            string? idempotencyKey,
            CancellationToken cancellationToken = default)
    {
        ValidateBasicRules(
            request.SourceAccountId,
            request.DestinationAccountId,
            request.Amount);

        if (request.ScheduledAt.Kind ==
            DateTimeKind.Unspecified)
        {
            throw new BusinessException(
                "A data de agendamento deve informar o fuso horário ou utilizar UTC.");
        }

        var scheduledAt =
            request.ScheduledAt
                .ToUniversalTime();

        if (scheduledAt <=
            DateTime.UtcNow)
        {
            throw new BusinessException(
                "A data do agendamento deve ser futura.");
        }

        var existing =
            await GetExistingIdempotentTransferAsync(
                idempotencyKey,
                cancellationToken);

        if (existing is not null)
        {
            return Map(existing);
        }

        var source =
            await _accountRepository
                .GetByIdAsync(
                    request.SourceAccountId,
                    cancellationToken);

        var destination =
            await _accountRepository
                .GetByIdAsync(
                    request.DestinationAccountId,
                    cancellationToken);

        ValidateAccounts(
            source,
            destination);

        var transfer =
            new Transfer
            {
                Id =
                    Guid.NewGuid(),

                SourceAccountId =
                    request.SourceAccountId,

                DestinationAccountId =
                    request.DestinationAccountId,

                Amount =
                    request.Amount,

                Status =
                    TransferStatus.Scheduled,

                CreatedAt =
                    DateTime.UtcNow,

                ScheduledAt =
                    scheduledAt,

                IdempotencyKey =
                    NormalizeIdempotencyKey(
                        idempotencyKey)
            };

        _transferRepository.Add(
            transfer);

        await _context.SaveChangesAsync(
            cancellationToken);

        return Map(
            transfer);
    }

    public async Task<TransferResponse>
        GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
    {
        var transfer =
            await _transferRepository
                .GetByIdAsync(
                    id,
                    cancellationToken);

        if (transfer is null)
        {
            throw new NotFoundException(
                "Transferência não encontrada.");
        }

        return Map(
            transfer);
    }

    public async Task<IReadOnlyList<TransferResponse>>
        GetHistoryByAccountIdAsync(
            int accountId,
            CancellationToken cancellationToken = default)
    {
        var account =
            await _accountRepository
                .GetByIdAsync(
                    accountId,
                    cancellationToken);

        if (account is null)
        {
            throw new NotFoundException(
                "Conta não encontrada.");
        }

        var transfers =
            await _transferRepository
                .GetHistoryByAccountIdAsync(
                    accountId,
                    cancellationToken);

        return transfers
            .Select(Map)
            .ToList();
    }

    public async Task CancelAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var transfer =
            await _transferRepository
                .GetByIdAsync(
                    id,
                    cancellationToken);

        if (transfer is null)
        {
            throw new NotFoundException(
                "Transferência não encontrada.");
        }

        if (transfer.Status !=
            TransferStatus.Scheduled)
        {
            throw new BusinessException(
                "Somente transferências agendadas podem ser canceladas.");
        }

        transfer.Status =
            TransferStatus.Cancelled;

        transfer.CancelledAt =
            DateTime.UtcNow;

        try
        {
            await _context.SaveChangesAsync(
                cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new BusinessException(
                "A transferência não pode mais ser cancelada porque seu estado foi alterado.");
        }
    }

    public async Task ProcessScheduledAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var transfer =
            await _transferRepository
                .GetByIdAsync(
                    id,
                    cancellationToken);

        if (transfer is null)
        {
            return;
        }

        /*
         * Kafka possui entrega pelo menos uma vez.
         *
         * Se a mesma mensagem chegar novamente,
         * uma transferência já concluída/falhada/
         * cancelada não será executada novamente.
         */
        if (transfer.Status !=
            TransferStatus.Scheduled)
        {
            return;
        }

        transfer.Status =
            TransferStatus.Processing;

        try
        {
            await _context.SaveChangesAsync(
                cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            /*
             * Outro consumer ou cancelamento
             * alterou a transferência.
             */
            return;
        }

        try
        {
            await ExecuteTransferAsync(
                id,
                cancellationToken);
        }
        catch (BusinessException)
        {
            /*
             * É uma falha de negócio definitiva.
             *
             * O status Failed já foi gravado.
             * Portanto não queremos que Kafka
             * execute infinitamente.
             */
        }
        catch (NotFoundException)
        {
            /*
             * Mesma ideia: falha definitiva.
             */
        }
    }

    private async Task ExecuteTransferAsync(
        Guid transferId,
        CancellationToken cancellationToken)
    {
        /*
         * Criamos UMA tentativa.
         *
         * Em caso de retry por concorrência,
         * não criaremos outra tentativa.
         */
        var transfer =
            await _transferRepository
                .GetByIdAsync(
                    transferId,
                    cancellationToken)
            ?? throw new NotFoundException(
                "Transferência não encontrada.");

        var attempt =
            new TransferAttempt
            {
                AccountId =
                    transfer.SourceAccountId,

                TransferId =
                    transfer.Id,

                Amount =
                    transfer.Amount,

                CreatedAt =
                    DateTime.UtcNow,

                Success =
                    false
            };

        _attemptRepository.Add(
            attempt);

        await _context.SaveChangesAsync(
            cancellationToken);

        try
        {
            await ExecuteWithConcurrencyRetryAsync(
                transferId,
                attempt.Id,
                cancellationToken);
        }
        catch (BusinessException ex)
        {
            await MarkAsFailedAsync(
                transferId,
                attempt.Id,
                ex.Message,
                cancellationToken);

            throw;
        }
        catch (NotFoundException ex)
        {
            await MarkAsFailedAsync(
                transferId,
                attempt.Id,
                ex.Message,
                cancellationToken);

            throw;
        }
    }

    private async Task
        ExecuteWithConcurrencyRetryAsync(
            Guid transferId,
            long attemptId,
            CancellationToken cancellationToken)
    {
        for (var retry = 1;
             retry <= MaxConcurrencyRetries;
             retry++)
        {
            try
            {
                await ExecuteAtomicTransferAsync(
                    transferId,
                    attemptId,
                    cancellationToken);

                return;
            }
            catch (DbUpdateConcurrencyException)
                when (retry <
                      MaxConcurrencyRetries)
            {
                /*
                 * Outro request modificou a conta.
                 *
                 * Limpamos o tracking e fazemos
                 * TODAS as validações novamente.
                 */
                _context.ChangeTracker.Clear();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new BusinessException(
                    "Não foi possível concluir a transferência devido a operações simultâneas. Tente novamente.");
            }
        }
    }

    private async Task
        ExecuteAtomicTransferAsync(
            Guid transferId,
            long attemptId,
            CancellationToken cancellationToken)
    {
        var transfer =
            await _transferRepository
                .GetByIdAsync(
                    transferId,
                    cancellationToken)
            ?? throw new NotFoundException(
                "Transferência não encontrada.");

        var source =
            await _accountRepository
                .GetByIdAsync(
                    transfer.SourceAccountId,
                    cancellationToken);

        var destination =
            await _accountRepository
                .GetByIdAsync(
                    transfer.DestinationAccountId,
                    cancellationToken);

        ValidateAccounts(
            source,
            destination);

        await _limitService.ValidateAsync(
            source!.Id,
            source.PersonId,
            transfer.Amount,
            cancellationToken);

        var availableBalance =
            source.Balance +
            source.OverdraftLimit;

        if (availableBalance <
            transfer.Amount)
        {
            throw new BusinessException(
                "Saldo e cheque especial insuficientes.");
        }

        var attempt =
            await _attemptRepository
                .GetByIdAsync(
                    attemptId,
                    cancellationToken)
            ?? throw new NotFoundException(
                "Tentativa de transferência não encontrada.");

        await using var transaction =
            await _context.Database
                .BeginTransactionAsync(
                    cancellationToken);

        try
        {
            /*
             * Débito.
             */
            source.Balance -=
                transfer.Amount;

            /*
             * Crédito.
             *
             * Se destino estiver -800 e receber
             * 1000:
             *
             * -800 + 1000 = 200
             *
             * O cheque utilizado é naturalmente
             * coberto.
             */
            destination!.Balance +=
                transfer.Amount;

            transfer.Status =
                TransferStatus.Completed;

            transfer.ProcessedAt =
                DateTime.UtcNow;

            transfer.FailureReason =
                null;

            attempt.Success =
                true;

            attempt.FailureReason =
                null;

            await _context.SaveChangesAsync(
                cancellationToken);

            await transaction.CommitAsync(
                cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(
                cancellationToken);

            throw;
        }
    }

    private async Task MarkAsFailedAsync(
        Guid transferId,
        long attemptId,
        string reason,
        CancellationToken cancellationToken)
    {
        /*
         * Pode ter ocorrido Clear() devido
         * à concorrência.
         */
        _context.ChangeTracker.Clear();

        var transfer =
            await _transferRepository
                .GetByIdAsync(
                    transferId,
                    cancellationToken);

        var attempt =
            await _attemptRepository
                .GetByIdAsync(
                    attemptId,
                    cancellationToken);

        if (transfer is not null &&
            transfer.Status !=
                TransferStatus.Completed)
        {
            transfer.Status =
                TransferStatus.Failed;

            transfer.FailureReason =
                reason;

            transfer.ProcessedAt =
                DateTime.UtcNow;
        }

        if (attempt is not null)
        {
            attempt.Success =
                false;

            attempt.FailureReason =
                reason;
        }

        await _context.SaveChangesAsync(
            cancellationToken);
    }

    private async Task<Transfer?>
        GetExistingIdempotentTransferAsync(
            string? idempotencyKey,
            CancellationToken cancellationToken)
    {
        var normalized =
            NormalizeIdempotencyKey(
                idempotencyKey);

        if (normalized is null)
        {
            return null;
        }

        return await _transferRepository
            .GetByIdempotencyKeyAsync(
                normalized,
                cancellationToken);
    }

    private static string?
        NormalizeIdempotencyKey(
            string? key)
    {
        if (string.IsNullOrWhiteSpace(
                key))
        {
            return null;
        }

        return key.Trim();
    }

    private static void ValidateBasicRules(
        int sourceAccountId,
        int destinationAccountId,
        decimal amount)
    {
        if (sourceAccountId <= 0)
        {
            throw new BusinessException(
                "A conta de origem é obrigatória.");
        }

        if (destinationAccountId <= 0)
        {
            throw new BusinessException(
                "A conta de destino é obrigatória.");
        }

        if (sourceAccountId ==
            destinationAccountId)
        {
            throw new BusinessException(
                "A conta de origem e destino devem ser diferentes.");
        }

        if (amount <= 0)
        {
            throw new BusinessException(
                "O valor da transferência deve ser maior que zero.");
        }
    }

    private static void ValidateAccounts(
        Account? source,
        Account? destination)
    {
        if (source is null)
        {
            throw new NotFoundException(
                "Conta de origem não encontrada.");
        }

        if (destination is null)
        {
            throw new NotFoundException(
                "Conta de destino não encontrada.");
        }

        if (source.Status !=
            AccountStatus.Active)
        {
            throw new BusinessException(
                "A conta de origem não está ativa.");
        }

        if (destination.Status !=
            AccountStatus.Active)
        {
            throw new BusinessException(
                "A conta de destino não está ativa.");
        }
    }

    private static TransferResponse Map(
        Transfer transfer)
    {
        return new TransferResponse
        {
            Id =
                transfer.Id,

            SourceAccountId =
                transfer.SourceAccountId,

            DestinationAccountId =
                transfer.DestinationAccountId,

            SourceAccountName =
                transfer.SourceAccount?.Person?.Name,

            DestinationAccountName =
                transfer.DestinationAccount?.Person?.Name,

            Amount =
                transfer.Amount,

            Status =
                transfer.Status.ToString(),

            CreatedAt =
                transfer.CreatedAt,

            ScheduledAt =
                transfer.ScheduledAt,

            ProcessedAt =
                transfer.ProcessedAt,

            CancelledAt =
                transfer.CancelledAt,

            FailureReason =
                transfer.FailureReason
        };
    }
}