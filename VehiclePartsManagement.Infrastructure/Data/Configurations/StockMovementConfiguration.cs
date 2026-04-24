using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VehiclePartsManagement.Domain.Entities;

namespace VehiclePartsManagement.Infrastructure.Data.Configurations;

public class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
{
    public void Configure(EntityTypeBuilder<StockMovement> builder)
    {
        builder.ToTable("StockMovement", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint("CK_StockMovement_MovementQuantity_Positive", "\"MovementQuantity\" > 0");
            tableBuilder.HasCheckConstraint(
                "CK_StockMovement_MovementType_Valid",
                "\"MovementType\" IN ('In','Out','Adjustment')");
            tableBuilder.HasCheckConstraint(
                "CK_StockMovement_ReferenceType_Valid",
                "\"ReferenceType\" IN ('Purchase','Sale','ManualAdjustment','Return')");
        });

        builder.HasKey(sm => sm.StockMovementId);

        builder.Property(sm => sm.MovementType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(sm => sm.ReferenceType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(sm => sm.MovementDate)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasOne(sm => sm.Part)
            .WithMany(p => p.StockMovements)
            .HasForeignKey(sm => sm.PartId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(sm => sm.PartId);
        builder.HasIndex(sm => sm.MovementDate);
    }
}
