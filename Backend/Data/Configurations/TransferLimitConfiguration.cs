using Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Data.Configurations;

public class TransferLimitConfiguration :
    IEntityTypeConfiguration<TransferLimit>
{
    public void Configure(
        EntityTypeBuilder<TransferLimit> builder)
    {
        builder.ToTable("transfer_limits");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.DayHourlyAmountLimit)
            .HasPrecision(18, 2);

        builder.Property(x => x.NightHourlyAmountLimit)
            .HasPrecision(18, 2);

        builder.HasOne(x => x.Person)
            .WithOne(x => x.TransferLimit)
            .HasForeignKey<TransferLimit>(
                x => x.PersonId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.PersonId)
            .IsUnique();
    }
}