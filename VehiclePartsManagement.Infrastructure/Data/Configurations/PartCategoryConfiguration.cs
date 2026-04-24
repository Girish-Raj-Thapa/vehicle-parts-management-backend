using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VehiclePartsManagement.Domain.Entities;

namespace VehiclePartsManagement.Infrastructure.Data.Configurations;

public class PartCategoryConfiguration : IEntityTypeConfiguration<PartCategory>
{
    public void Configure(EntityTypeBuilder<PartCategory> builder)
    {
        builder.ToTable("PartCategory");

        builder.HasKey(pc => pc.PartCategoryId);

        builder.Property(pc => pc.CategoryName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(pc => pc.CategoryDescription)
            .HasMaxLength(500);

        builder.HasIndex(pc => pc.CategoryName)
            .IsUnique();
    }
}
