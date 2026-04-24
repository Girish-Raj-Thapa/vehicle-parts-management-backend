using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VehiclePartsManagement.Domain.Entities;

namespace VehiclePartsManagement.Infrastructure.Data.Configurations;

public class PartImageConfiguration : IEntityTypeConfiguration<PartImage>
{
    public void Configure(EntityTypeBuilder<PartImage> builder)
    {
        builder.ToTable("PartImage");

        builder.HasKey(pi => pi.PartImageId);

        builder.Property(pi => pi.ImagePath)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(pi => pi.DisplayOrder)
            .HasDefaultValue(0);

        builder.Property(pi => pi.IsPrimary)
            .HasDefaultValue(false);

        builder.Property(pi => pi.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasOne(pi => pi.Part)
            .WithMany(p => p.PartImages)
            .HasForeignKey(pi => pi.PartId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(pi => pi.PartId);
        builder.HasIndex(pi => new { pi.PartId, pi.IsPrimary });
    }
}
