using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VehiclePartsManagement.Domain.Entities;

namespace VehiclePartsManagement.Infrastructure.Data.Configurations;

public class PartConfiguration : IEntityTypeConfiguration<Part>
{
    public void Configure(EntityTypeBuilder<Part> builder)
    {
        builder.ToTable("Part", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint("CK_Part_SellingPrice_NonNegative", "\"SellingPrice\" >= 0");
            tableBuilder.HasCheckConstraint("CK_Part_CurrentStock_NonNegative", "\"CurrentStock\" >= 0");
            tableBuilder.HasCheckConstraint("CK_Part_LowStockThreshold_NonNegative", "\"LowStockThreshold\" >= 0");
        });

        builder.HasKey(p => p.PartId);

        builder.Property(p => p.Sku)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.PartName)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(p => p.PartDescription)
            .HasMaxLength(1000);

        builder.Property(p => p.SellingPrice)
            .HasColumnType("numeric(18,2)")
            .HasDefaultValue(0);

        builder.Property(p => p.CurrentStock)
            .HasDefaultValue(0);

        builder.Property(p => p.LowStockThreshold)
            .HasDefaultValue(10);

        builder.Property(p => p.IsActive)
            .HasDefaultValue(true);

        builder.HasOne(p => p.Category)
            .WithMany(pc => pc.Parts)
            .HasForeignKey(p => p.PartCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => p.Sku)
            .IsUnique();
    }
}
