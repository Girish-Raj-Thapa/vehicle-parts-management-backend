using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VehiclePartsManagement.Domain.Entities;

namespace VehiclePartsManagement.Infrastructure.Data.Configurations;

public class PurchaseItemConfiguration : IEntityTypeConfiguration<PurchaseItem>
{
    public void Configure(EntityTypeBuilder<PurchaseItem> builder)
    {
        builder.ToTable("PurchaseItem", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint("CK_PurchaseItem_QuantityPurchased_Positive", "\"QuantityPurchased\" > 0");
            tableBuilder.HasCheckConstraint("CK_PurchaseItem_UnitPriceAtPurchase_Positive", "\"UnitPriceAtPurchase\" > 0");
            tableBuilder.HasCheckConstraint("CK_PurchaseItem_PurchaseLineTotal_NonNegative", "\"PurchaseLineTotal\" >= 0");
        });

        builder.HasKey(pi => pi.PurchaseItemId);

        builder.Property(pi => pi.UnitPriceAtPurchase)
            .HasColumnType("numeric(18,2)");

        builder.Property(pi => pi.PurchaseLineTotal)
            .HasColumnType("numeric(18,2)");

        builder.HasOne(pi => pi.Purchase)
            .WithMany(p => p.PurchaseItems)
            .HasForeignKey(pi => pi.PurchaseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(pi => pi.Part)
            .WithMany(p => p.PurchaseItems)
            .HasForeignKey(pi => pi.PartId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(pi => pi.PurchaseId);
        builder.HasIndex(pi => pi.PartId);
    }
}
