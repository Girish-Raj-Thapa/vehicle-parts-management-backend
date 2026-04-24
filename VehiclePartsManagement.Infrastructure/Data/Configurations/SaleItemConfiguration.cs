using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VehiclePartsManagement.Domain.Entities;

namespace VehiclePartsManagement.Infrastructure.Data.Configurations;

public class SaleItemConfiguration : IEntityTypeConfiguration<SaleItem>
{
    public void Configure(EntityTypeBuilder<SaleItem> builder)
    {
        builder.ToTable("SaleItem", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint("CK_SaleItem_QuantitySold_Positive", "\"QuantitySold\" > 0");
            tableBuilder.HasCheckConstraint("CK_SaleItem_UnitPriceAtSale_Positive", "\"UnitPriceAtSale\" > 0");
            tableBuilder.HasCheckConstraint("CK_SaleItem_LineTotal_NonNegative", "\"LineTotal\" >= 0");
        });

        builder.HasKey(si => si.SaleItemId);

        builder.Property(si => si.UnitPriceAtSale)
            .HasColumnType("numeric(18,2)");

        builder.Property(si => si.LineTotal)
            .HasColumnType("numeric(18,2)");

        builder.HasOne(si => si.Sale)
            .WithMany(s => s.SaleItems)
            .HasForeignKey(si => si.SaleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(si => si.Part)
            .WithMany(p => p.SaleItems)
            .HasForeignKey(si => si.PartId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(si => si.SaleId);
        builder.HasIndex(si => si.PartId);
    }
}
