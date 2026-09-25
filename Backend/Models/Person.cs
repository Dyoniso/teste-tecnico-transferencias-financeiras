namespace Backend.Models;

public class Person
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public Account? Account { get; set; }

    public TransferLimit? TransferLimit { get; set; }
}