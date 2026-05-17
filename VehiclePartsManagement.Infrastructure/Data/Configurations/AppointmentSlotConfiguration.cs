using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VehiclePartsManagement.Domain.Entities;

namespace VehiclePartsManagement.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core mapping for appointment slot table.
/// </summary>
public class AppointmentSlotConfiguration : IEntityTypeConfiguration<AppointmentSlot>
{
    public void Configure(EntityTypeBuilder<AppointmentSlot> builder)
    {
        builder.ToTable("AppointmentSlot", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint(
                "CK_AppointmentSlot_MaxCapacity_Positive",
                "\"MaxCapacity\" > 0");
        });

        builder.HasKey(s => s.AppointmentSlotId);

        builder.Property(s => s.SlotTime)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(s => s.MaxCapacity)
            .IsRequired();

        builder.Property(s => s.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(s => s.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Prevent duplicate slot definitions for same date/time.
        builder.HasIndex(s => new { s.SlotDate, s.SlotTime })
            .IsUnique();

        builder.HasIndex(s => s.IsActive);
    }
}
