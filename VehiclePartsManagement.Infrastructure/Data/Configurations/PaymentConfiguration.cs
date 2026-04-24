using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VehiclePartsManagement.Domain.Entities;

namespace VehiclePartsManagement.Infrastructure.Data.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payment", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint("CK_Payment_PaymentAmount_Positive", "\"PaymentAmount\" > 0");
            tableBuilder.HasCheckConstraint(
                "CK_Payment_PaymentMethod_Valid",
                "\"PaymentMethod\" IN ('Cash','Card','BankTransfer','MobileWallet','Cheque','Other')");
        });

        builder.HasKey(p => p.PaymentId);

        builder.Property(p => p.PaymentMethod)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(p => p.PaymentAmount)
            .HasColumnType("numeric(18,2)");

        builder.Property(p => p.PaymentReference)
            .HasMaxLength(100);

        builder.Property(p => p.PaymentNote)
            .HasMaxLength(500);

        builder.Property(p => p.PaymentDate)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasOne(p => p.Sale)
            .WithMany(s => s.Payments)
            .HasForeignKey(p => p.SaleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => p.SaleId);
        builder.HasIndex(p => p.PaymentDate);
    }
}
