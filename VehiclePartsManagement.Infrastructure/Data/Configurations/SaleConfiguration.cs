using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VehiclePartsManagement.Domain.Entities;

namespace VehiclePartsManagement.Infrastructure.Data.Configurations;

public class SaleConfiguration : IEntityTypeConfiguration<Sale>
{
    public void Configure(EntityTypeBuilder<Sale> builder)
    {
        builder.ToTable("Sale", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint("CK_Sale_SubTotal_NonNegative", "\"SubTotal\" >= 0");
            tableBuilder.HasCheckConstraint("CK_Sale_LoyaltyDiscountPercent_Valid", "\"LoyaltyDiscountPercent\" >= 0 AND \"LoyaltyDiscountPercent\" <= 100");
            tableBuilder.HasCheckConstraint("CK_Sale_DiscountAmount_NonNegative", "\"DiscountAmount\" >= 0");
            tableBuilder.HasCheckConstraint("CK_Sale_TotalAmount_NonNegative", "\"TotalAmount\" >= 0");
            tableBuilder.HasCheckConstraint("CK_Sale_BalanceDue_NonNegative", "\"BalanceDue\" >= 0");
            tableBuilder.HasCheckConstraint(
                "CK_Sale_SaleStatus_Valid",
                "\"SaleStatus\" IN ('Draft','Confirmed','Completed','Cancelled')");
            tableBuilder.HasCheckConstraint(
                "CK_Sale_PaymentStatus_Valid",
                "\"PaymentStatus\" IN ('Unpaid','PartiallyPaid','Paid','Overdue')");
            tableBuilder.HasCheckConstraint(
                "CK_Sale_SaleChannel_Valid",
                "\"SaleChannel\" IN ('Workshop','OnlineParts')");
        });

        builder.HasKey(s => s.SaleId);

        builder.Property(s => s.CustomerApplicationUserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(s => s.ProcessedByApplicationUserId)
            .HasMaxLength(450);

        builder.Property(s => s.SaleInvoiceNo)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.SaleStatus)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(s => s.SaleChannel)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30)
            .HasDefaultValue(Domain.Enums.SaleChannel.Workshop);

        builder.Property(s => s.PaymentStatus)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(s => s.SaleDate)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(s => s.SubTotal)
            .HasColumnType("numeric(18,2)")
            .HasDefaultValue(0);

        builder.Property(s => s.LoyaltyDiscountPercent)
            .HasColumnType("numeric(5,2)")
            .HasDefaultValue(0);

        builder.Property(s => s.DiscountAmount)
            .HasColumnType("numeric(18,2)")
            .HasDefaultValue(0);

        builder.Property(s => s.TotalAmount)
            .HasColumnType("numeric(18,2)")
            .HasDefaultValue(0);

        builder.Property(s => s.BalanceDue)
            .HasColumnType("numeric(18,2)")
            .HasDefaultValue(0);

        builder.HasOne(s => s.CustomerUser)
            .WithMany(u => u.CustomerSales)
            .HasForeignKey(s => s.CustomerApplicationUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.ProcessedByUser)
            .WithMany(u => u.ProcessedSales)
            .HasForeignKey(s => s.ProcessedByApplicationUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Vehicle)
            .WithMany(v => v.Sales)
            .HasForeignKey(s => s.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(s => s.SaleInvoiceNo)
            .IsUnique();
    }
}
