using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VehiclePartsManagement.Domain.Entities;

namespace VehiclePartsManagement.Infrastructure.Data.Configurations;

public class VehicleBrandConfiguration : IEntityTypeConfiguration<VehicleBrand>
{
    public void Configure(EntityTypeBuilder<VehicleBrand> builder)
    {
        builder.ToTable("VehicleBrand");

        builder.HasKey(vb => vb.VehicleBrandId);

        builder.Property(vb => vb.BrandName)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(vb => vb.BrandName)
            .IsUnique();
    }
}
