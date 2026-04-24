using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VehiclePartsManagement.Domain.Entities;

namespace VehiclePartsManagement.Infrastructure.Data.Configurations;

public class PartRequestImageConfiguration : IEntityTypeConfiguration<PartRequestImage>
{
    public void Configure(EntityTypeBuilder<PartRequestImage> builder)
    {
        builder.ToTable("PartRequestImage");

        builder.HasKey(pri => pri.PartRequestImageId);

        builder.Property(pri => pri.ImagePath)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(pri => pri.DisplayOrder)
            .HasDefaultValue(0);

        builder.Property(pri => pri.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasOne(pri => pri.PartRequest)
            .WithMany(pr => pr.PartRequestImages)
            .HasForeignKey(pri => pri.PartRequestId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(pri => pri.PartRequestId);
    }
}
