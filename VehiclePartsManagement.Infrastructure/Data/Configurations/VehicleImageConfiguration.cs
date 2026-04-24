using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VehiclePartsManagement.Domain.Entities;

namespace VehiclePartsManagement.Infrastructure.Data.Configurations;

public class VehicleImageConfiguration : IEntityTypeConfiguration<VehicleImage>
{
    public void Configure(EntityTypeBuilder<VehicleImage> builder)
    {
        builder.ToTable("VehicleImage");

        builder.HasKey(vi => vi.VehicleImageId);

        builder.Property(vi => vi.ImagePath)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(vi => vi.DisplayOrder)
            .HasDefaultValue(0);

        builder.Property(vi => vi.IsPrimary)
            .HasDefaultValue(false);

        builder.Property(vi => vi.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasOne(vi => vi.Vehicle)
            .WithMany(v => v.VehicleImages)
            .HasForeignKey(vi => vi.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(vi => vi.VehicleId);
        builder.HasIndex(vi => new { vi.VehicleId, vi.IsPrimary });
    }
}
