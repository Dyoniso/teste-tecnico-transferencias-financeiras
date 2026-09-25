using Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Data.Configurations;

public class TransferAttemptConfiguration :
    IEntityTypeConfiguration<TransferAttempt>
{
    public void Configure(
        EntityTypeBuilder<TransferAttempt> builder)
    {
        builder.ToTable("transfer_attempts");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Amount)
            .HasPrecision(18, 2);

        builder.Property(x => x.FailureReason)
            .HasMaxLength(500);

        builder.HasOne(x => x.Account)
            .WithMany()
            .HasForeignKey(x => x.AccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new
        {
            x.AccountId,
            x.CreatedAt
        });
    }
}