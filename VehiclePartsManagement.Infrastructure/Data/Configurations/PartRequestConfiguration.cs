using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VehiclePartsManagement.Domain.Entities;

namespace VehiclePartsManagement.Infrastructure.Data.Configurations;

public class PartRequestConfiguration : IEntityTypeConfiguration<PartRequest>
{
    public void Configure(EntityTypeBuilder<PartRequest> builder)
    {
        builder.ToTable("PartRequest", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint(
                "CK_PartRequest_RequestStatus_Valid",
                "\"RequestStatus\" IN ('Pending','Ordered','Received','Completed','Cancelled')");
        });

        builder.HasKey(pr => pr.PartRequestId);

        builder.Property(pr => pr.ApplicationUserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(pr => pr.RequestedPartName)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(pr => pr.RequestDescription)
            .HasMaxLength(1000);

        builder.Property(pr => pr.RequestStatus)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(pr => pr.RequestDate)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasOne(pr => pr.User)
            .WithMany(u => u.PartRequests)
            .HasForeignKey(pr => pr.ApplicationUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(pr => pr.Vehicle)
            .WithMany(v => v.PartRequests)
            .HasForeignKey(pr => pr.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(pr => pr.FulfilledPart)
            .WithMany(p => p.FulfilledPartRequests)
            .HasForeignKey(pr => pr.FulfilledPartId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(pr => pr.ApplicationUserId);
        builder.HasIndex(pr => pr.RequestDate);
    }
}
