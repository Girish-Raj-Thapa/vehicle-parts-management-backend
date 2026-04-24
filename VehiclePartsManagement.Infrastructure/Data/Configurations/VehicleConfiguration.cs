using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VehiclePartsManagement.Domain.Entities;

namespace VehiclePartsManagement.Infrastructure.Data.Configurations;

public class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        builder.ToTable("Vehicle", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint("CK_Vehicle_CurrentMileage_NonNegative", "\"CurrentMileage\" >= 0");
            tableBuilder.HasCheckConstraint("CK_Vehicle_ManufactureYear_Range", "\"ManufactureYear\" IS NULL OR (\"ManufactureYear\" >= 1900 AND \"ManufactureYear\" <= 2100)");
        });

        builder.HasKey(v => v.VehicleId);

        builder.Property(v => v.ApplicationUserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(v => v.VehicleNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(v => v.EngineNumber)
            .HasMaxLength(100);

        builder.Property(v => v.ChassisNumber)
            .HasMaxLength(100);

        builder.Property(v => v.FuelType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(v => v.Color)
            .HasMaxLength(30);

        builder.Property(v => v.CurrentMileage)
            .HasDefaultValue(0);

        builder.Property(v => v.VehicleRegisteredAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasOne(v => v.User)
            .WithMany(u => u.Vehicles)
            .HasForeignKey(v => v.ApplicationUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(v => v.Model)
            .WithMany(vm => vm.Vehicles)
            .HasForeignKey(v => v.VehicleModelId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(v => v.VehicleNumber)
            .IsUnique();
    }
}
