using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VehiclePartsManagement.Domain.Entities;

namespace VehiclePartsManagement.Infrastructure.Data.Configurations;

public class VehicleModelConfiguration : IEntityTypeConfiguration<VehicleModel>
{
    public void Configure(EntityTypeBuilder<VehicleModel> builder)
    {
        builder.ToTable("VehicleModel");

        builder.HasKey(vm => vm.VehicleModelId);

        builder.Property(vm => vm.ModelName)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasOne(vm => vm.Brand)
            .WithMany(vb => vb.VehicleModels)
            .HasForeignKey(vm => vm.VehicleBrandId)
            .OnDelete(DeleteBehavior.Restrict);

        // Model names can repeat across brands, but should be unique within a brand.
        builder.HasIndex(vm => new { vm.VehicleBrandId, vm.ModelName })
            .IsUnique();
    }
}
