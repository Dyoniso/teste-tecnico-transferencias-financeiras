namespace Backend.Models;

public class TransferLimit
{
    public int Id { get; set; }

    public int PersonId { get; set; }

    public decimal DayHourlyAmountLimit { get; set; }

    public int DayHourlyAttemptLimit { get; set; }

    public decimal NightHourlyAmountLimit { get; set; }

    public int NightHourlyAttemptLimit { get; set; }

    public Person Person { get; set; } = null!;
}