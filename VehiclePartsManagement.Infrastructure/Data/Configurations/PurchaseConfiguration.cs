using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VehiclePartsManagement.Domain.Entities;

namespace VehiclePartsManagement.Infrastructure.Data.Configurations;

public class PurchaseConfiguration : IEntityTypeConfiguration<Purchase>
{
    public void Configure(EntityTypeBuilder<Purchase> builder)
    {
        builder.ToTable("Purchase", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint("CK_Purchase_TotalAmount_NonNegative", "\"TotalAmount\" >= 0");
            tableBuilder.HasCheckConstraint(
                "CK_Purchase_PurchaseStatus_Valid",
                "\"PurchaseStatus\" IN ('Pending','Ordered','Received','Completed','Cancelled')");
        });

        builder.HasKey(p => p.PurchaseId);

        builder.Property(p => p.CreatedByApplicationUserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(p => p.PurchaseInvoiceNo)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.PurchaseStatus)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(p => p.PurchaseDate)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(p => p.TotalAmount)
            .HasColumnType("numeric(18,2)")
            .HasDefaultValue(0);

        builder.HasOne(p => p.Vendor)
            .WithMany(v => v.Purchases)
            .HasForeignKey(p => p.VendorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.CreatedByUser)
            .WithMany(u => u.CreatedPurchases)
            .HasForeignKey(p => p.CreatedByApplicationUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => p.PurchaseInvoiceNo)
            .IsUnique();
    }
}
