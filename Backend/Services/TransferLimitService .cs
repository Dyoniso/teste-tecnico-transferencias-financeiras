using Backend.Middlewares;
using Backend.Repositories.Interfaces;
using Backend.Services.Interfaces;
using System.Globalization;

namespace Backend.Services;

public class TransferLimitService :
    ITransferLimitService
{
    private readonly ITransferLimitRepository
        _limitRepository;

    private readonly ITransferAttemptRepository
        _attemptRepository;

    private readonly ITransferRepository
        _transferRepository;

    private readonly IConfiguration
        _configuration;

    public TransferLimitService(
        ITransferLimitRepository limitRepository,
        ITransferAttemptRepository attemptRepository,
        ITransferRepository transferRepository,
        IConfiguration configuration)
    {
        _limitRepository =
            limitRepository;

        _attemptRepository =
            attemptRepository;

        _transferRepository =
            transferRepository;

        _configuration =
            configuration;
    }

    public async Task ValidateAsync(
        int accountId,
        int personId,
        decimal amount,
        CancellationToken cancellationToken = default)
    {
        var limit =
            await _limitRepository
                .GetByPersonIdAsync(
                    personId,
                    cancellationToken);

        if (limit is null)
        {
            throw new BusinessException(
                "Limites de transferência não configurados para esta pessoa.");
        }

        var utcNow =
            DateTime.UtcNow;

        var since =
            utcNow.AddHours(-1);

        var localTime =
            GetLocalDateTime(
                utcNow);

        var nightStart =
            _configuration.GetValue(
                "TransferRules:NightStartHour",
                22);

        var dayStart =
            _configuration.GetValue(
                "TransferRules:DayStartHour",
                6);

        var isNight =
            localTime.Hour >= nightStart ||
            localTime.Hour < dayStart;

        var maximumAmount =
            isNight
                ? limit.NightHourlyAmountLimit
                : limit.DayHourlyAmountLimit;

        var maximumAttempts =
            isNight
                ? limit.NightHourlyAttemptLimit
                : limit.DayHourlyAttemptLimit;

        var attempts =
            await _attemptRepository
                .CountSinceAsync(
                    accountId,
                    since,
                    cancellationToken);

        /*
         * A tentativa atual já foi inserida.
         *
         * Limite 5:
         * tentativa atual = 5 -> permitido
         * tentativa atual = 6 -> rejeitado
         */
        if (attempts > maximumAttempts)
        {
            var oldestAttemptAt =
                await _attemptRepository
                    .GetOldestCreatedAtSinceAsync(
                        accountId,
                        since,
                        cancellationToken);

            throw new BusinessException(
                CreateLimitMessage(
                    "Limite de tentativas por hora excedido.",
                    oldestAttemptAt?.AddHours(1)),
                StatusCodes.Status429TooManyRequests);
        }

        var transferredAmount =
            await _transferRepository
                .GetCompletedAmountSinceAsync(
                    accountId,
                    since,
                    cancellationToken);

        if (transferredAmount + amount >
            maximumAmount)
        {
            var formattedMaximumAmount =
                maximumAmount.ToString(
                    "C",
                    CultureInfo.GetCultureInfo(
                        "pt-BR"));

            var nextAvailableAt =
                await GetNextAmountAvailabilityAsync(
                    accountId,
                    since,
                    amount,
                    maximumAmount,
                    cancellationToken);

            throw new BusinessException(
                CreateLimitMessage(
                    $"Limite de transferência por hora excedido. Limite atual: {formattedMaximumAmount}.",
                    nextAvailableAt),
                StatusCodes.Status429TooManyRequests);
        }
    }

    private async Task<DateTime?> GetNextAmountAvailabilityAsync(
        int accountId,
        DateTime since,
        decimal amount,
        decimal maximumAmount,
        CancellationToken cancellationToken)
    {
        if (amount > maximumAmount)
        {
            return null;
        }

        var completedTransfers =
            await _transferRepository
                .GetCompletedSinceAsync(
                    accountId,
                    since,
                    cancellationToken);

        var remainingAmount =
            completedTransfers.Sum(x => x.Amount);

        foreach (var transfer in completedTransfers)
        {
            remainingAmount -= transfer.Amount;

            if (remainingAmount + amount <= maximumAmount)
            {
                return transfer.ProcessedAt?.AddHours(1);
            }
        }

        return null;
    }

    private string CreateLimitMessage(
        string message,
        DateTime? nextAvailableAt)
    {
        if (nextAvailableAt is null)
        {
            return message;
        }

        var localNextAvailableAt =
            GetLocalDateTime(nextAvailableAt.Value);

        return $"{message} Você poderá transferir novamente a partir de {localNextAvailableAt:dd/MM/yyyy, HH:mm}.";
    }

    private DateTime GetLocalDateTime(
        DateTime utcDateTime)
    {
        var timeZoneId =
            _configuration[
                "TransferRules:TimeZone"];

        if (string.IsNullOrWhiteSpace(
                timeZoneId))
        {
            return utcDateTime;
        }

        var timeZone =
            TimeZoneInfo
                .FindSystemTimeZoneById(
                    timeZoneId);

        return TimeZoneInfo
            .ConvertTimeFromUtc(
                utcDateTime,
                timeZone);
    }
}