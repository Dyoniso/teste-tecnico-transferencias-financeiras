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
            throw new BusinessException(
                "Limite de tentativas por hora excedido.",
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

            throw new BusinessException(
                $"Limite de transferência por hora excedido. Limite atual: {formattedMaximumAmount}.",
                StatusCodes.Status429TooManyRequests);
        }
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