using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VehiclePartsManagement.Domain.Entities;

namespace VehiclePartsManagement.Infrastructure.Data.Configurations;

public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.ToTable("CartItem", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint("CK_CartItem_Quantity_Positive", "\"Quantity\" > 0");
        });

        builder.HasKey(ci => ci.CartItemId);

        builder.Property(ci => ci.Quantity)
            .IsRequired();

        builder.Property(ci => ci.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(ci => ci.UpdatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasOne(ci => ci.Cart)
            .WithMany(c => c.CartItems)
            .HasForeignKey(ci => ci.CartId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ci => ci.Part)
            .WithMany(p => p.CartItems)
            .HasForeignKey(ci => ci.PartId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(ci => new { ci.CartId, ci.PartId })
            .IsUnique();
    }
}
