using Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Data.Configurations;

public class PersonConfiguration :
    IEntityTypeConfiguration<Person>
{
    public void Configure(
        EntityTypeBuilder<Person> builder)
    {
        builder.ToTable("persons");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.Document)
            .HasMaxLength(20);

        builder.Property(x => x.Email)
            .HasMaxLength(150);

        builder.Property(x => x.Phone)
            .HasMaxLength(20);

        builder.Property(x => x.ZipCode)
            .HasMaxLength(10);

        builder.Property(x => x.Street)
            .HasMaxLength(150);

        builder.Property(x => x.Number)
            .HasMaxLength(20);

        builder.Property(x => x.Complement)
            .HasMaxLength(100);

        builder.Property(x => x.Neighborhood)
            .HasMaxLength(100);

        builder.Property(x => x.City)
            .HasMaxLength(100);

        builder.Property(x => x.State)
            .HasMaxLength(2);
    }
}