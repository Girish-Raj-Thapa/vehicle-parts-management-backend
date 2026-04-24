using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VehiclePartsManagement.Domain.Entities;

namespace VehiclePartsManagement.Infrastructure.Data.Configurations;

public class VendorConfiguration : IEntityTypeConfiguration<Vendor>
{
    public void Configure(EntityTypeBuilder<Vendor> builder)
    {
        builder.ToTable("Vendor");

        builder.HasKey(v => v.VendorId);

        builder.Property(v => v.VendorName)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(v => v.ContactPerson)
            .HasMaxLength(150);

        builder.Property(v => v.VendorEmail)
            .HasMaxLength(256);

        builder.Property(v => v.VendorPhone)
            .HasMaxLength(20);

        builder.Property(v => v.VendorAddress)
            .HasMaxLength(300);

        builder.Property(v => v.IsActive)
            .HasDefaultValue(true);
    }
}
