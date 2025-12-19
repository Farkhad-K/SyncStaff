using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SyncStaff.MVC.Models;
using System;

namespace SyncStaff.MVC.Models.Configurations;

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("Employees");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.PayrollNumber)
               .IsRequired()
               .HasMaxLength(50)
               .HasColumnType("varchar(50)");
        builder.HasIndex(e => e.PayrollNumber).IsUnique();

        builder.Property(e => e.Forenames)
               .IsRequired()
               .HasMaxLength(100)
               .HasColumnType("varchar(100)");

        builder.Property(e => e.Surname)
               .IsRequired()
               .HasMaxLength(100)
               .HasColumnType("varchar(100)");

        builder.Property(e => e.Email)
               .IsRequired()
               .HasMaxLength(100)
               .HasColumnType("varchar(100)");
        builder.HasIndex(e => e.Email);

        builder.Property(e => e.Telephone)
               .HasMaxLength(30)
               .HasColumnType("varchar(30)");

        builder.Property(e => e.Mobile)
               .HasMaxLength(30)
               .HasColumnType("varchar(30)");

        builder.Property(e => e.Address)
               .HasMaxLength(200)
               .HasColumnType("varchar(200)");

        builder.Property(e => e.Address2)
               .HasMaxLength(200)
               .HasColumnType("varchar(200)");

        builder.Property(e => e.Postcode)
               .HasMaxLength(20)
               .HasColumnType("varchar(20)");

        // DateOnly -> SQL date mapping (with conversion to/from DateTime)
        builder.Property(e => e.DateofBirth)
               .IsRequired()
               .HasConversion(
                   v => v.ToDateTime(TimeOnly.MinValue),
                   v => DateOnly.FromDateTime(v))
               .HasColumnType("date");

        builder.Property(e => e.StartDate)
               .IsRequired()
               .HasConversion(
                   v => v.ToDateTime(TimeOnly.MinValue),
                   v => DateOnly.FromDateTime(v))
               .HasColumnType("date");

        // Timestamps: use datetimeoffset in SQL Server, default to current offset time
        builder.Property(e => e.CreatedAt)
               .IsRequired()
               .HasColumnType("datetimeoffset")
               .HasDefaultValueSql("SYSDATETIMEOFFSET()");
        builder.HasIndex(e => e.CreatedAt);

        builder.Property(e => e.ModifiedAt)
               .IsRequired()
               .HasColumnType("datetimeoffset")
               .ValueGeneratedOnAddOrUpdate()
               .HasDefaultValueSql("SYSDATETIMEOFFSET()");

        // Optional: index on Surname for quick lookups
        builder.HasIndex(e => e.Surname);
    }
}

