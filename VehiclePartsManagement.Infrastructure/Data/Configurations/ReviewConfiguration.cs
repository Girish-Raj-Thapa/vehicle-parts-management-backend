using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VehiclePartsManagement.Domain.Entities;

namespace VehiclePartsManagement.Infrastructure.Data.Configurations;

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.ToTable("Review", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint("CK_Review_Rating_Range", "\"Rating\" >= 1 AND \"Rating\" <= 5");
        });

        builder.HasKey(r => r.ReviewId);

        builder.Property(r => r.ApplicationUserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(r => r.ReviewComment)
            .HasMaxLength(1000);

        builder.Property(r => r.ReviewDate)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasOne(r => r.ServiceRecord)
            .WithOne(sr => sr.Review)
            .HasForeignKey<Review>(r => r.ServiceRecordId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.User)
            .WithMany(u => u.Reviews)
            .HasForeignKey(r => r.ApplicationUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(r => r.ServiceRecordId)
            .IsUnique();
    }
}
