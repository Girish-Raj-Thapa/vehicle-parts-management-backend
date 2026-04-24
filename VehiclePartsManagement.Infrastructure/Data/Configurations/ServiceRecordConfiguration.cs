using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VehiclePartsManagement.Domain.Entities;

namespace VehiclePartsManagement.Infrastructure.Data.Configurations;

public class ServiceRecordConfiguration : IEntityTypeConfiguration<ServiceRecord>
{
    public void Configure(EntityTypeBuilder<ServiceRecord> builder)
    {
        builder.ToTable("ServiceRecord", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint("CK_ServiceRecord_MileageAtService_NonNegative", "\"MileageAtService\" >= 0");
            tableBuilder.HasCheckConstraint("CK_ServiceRecord_LaborCharge_NonNegative", "\"LaborCharge\" >= 0");
        });

        builder.HasKey(sr => sr.ServiceRecordId);

        builder.Property(sr => sr.ServiceType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(sr => sr.Diagnosis)
            .HasMaxLength(1000);

        builder.Property(sr => sr.WorkDone)
            .HasMaxLength(2000);

        builder.Property(sr => sr.LaborCharge)
            .HasColumnType("numeric(18,2)")
            .HasDefaultValue(0);

        builder.Property(sr => sr.ServiceDate)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasOne(sr => sr.ServiceAppointment)
            .WithOne(sa => sa.ServiceRecord)
            .HasForeignKey<ServiceRecord>(sr => sr.ServiceAppointmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(sr => sr.Vehicle)
            .WithMany(v => v.ServiceRecords)
            .HasForeignKey(sr => sr.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(sr => sr.ServiceAppointmentId)
            .IsUnique();
    }
}
