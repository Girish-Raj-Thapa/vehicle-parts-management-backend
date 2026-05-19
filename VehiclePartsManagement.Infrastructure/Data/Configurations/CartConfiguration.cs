using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VehiclePartsManagement.Domain.Entities;

namespace VehiclePartsManagement.Infrastructure.Data.Configurations;

public class CartConfiguration : IEntityTypeConfiguration<Cart>
{
    public void Configure(EntityTypeBuilder<Cart> builder)
    {
        builder.ToTable("Cart");

        builder.HasKey(c => c.CartId);

        builder.Property(c => c.CustomerApplicationUserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(c => c.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(c => c.UpdatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasOne(c => c.CustomerUser)
            .WithMany(u => u.Carts)
            .HasForeignKey(c => c.CustomerApplicationUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(c => c.CustomerApplicationUserId)
            .IsUnique();
    }
}
