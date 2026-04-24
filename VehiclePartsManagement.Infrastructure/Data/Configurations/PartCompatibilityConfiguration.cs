using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VehiclePartsManagement.Domain.Entities;

namespace VehiclePartsManagement.Infrastructure.Data.Configurations;

public class PartCompatibilityConfiguration : IEntityTypeConfiguration<PartCompatibility>
{
    public void Configure(EntityTypeBuilder<PartCompatibility> builder)
    {
        builder.ToTable("PartCompatibility");

        builder.HasKey(pc => new { pc.PartId, pc.VehicleModelId });

        builder.HasOne(pc => pc.Part)
            .WithMany(p => p.PartCompatibilities)
            .HasForeignKey(pc => pc.PartId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(pc => pc.Model)
            .WithMany(vm => vm.PartCompatibilities)
            .HasForeignKey(pc => pc.VehicleModelId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
