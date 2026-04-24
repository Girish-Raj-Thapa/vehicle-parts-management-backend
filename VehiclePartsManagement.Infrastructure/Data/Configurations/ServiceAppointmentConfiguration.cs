using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VehiclePartsManagement.Domain.Entities;

namespace VehiclePartsManagement.Infrastructure.Data.Configurations;

public class ServiceAppointmentConfiguration : IEntityTypeConfiguration<ServiceAppointment>
{
    public void Configure(EntityTypeBuilder<ServiceAppointment> builder)
    {
        builder.ToTable("ServiceAppointment", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint(
                "CK_ServiceAppointment_AppointmentStatus_Valid",
                "\"AppointmentStatus\" IN ('Pending','Confirmed','InProgress','Completed','Cancelled','NoShow')");
            tableBuilder.HasCheckConstraint(
                "CK_ServiceAppointment_AppointmentType_Valid",
                "\"AppointmentType\" IN ('GeneralService','Repair','Inspection','Maintenance','Other')");
        });

        builder.HasKey(sa => sa.ServiceAppointmentId);

        builder.Property(sa => sa.ApplicationUserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(sa => sa.AppointmentTime)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(sa => sa.AppointmentStatus)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(sa => sa.AppointmentType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(sa => sa.AppointmentNote)
            .HasMaxLength(500);

        builder.HasOne(sa => sa.User)
            .WithMany(u => u.ServiceAppointments)
            .HasForeignKey(sa => sa.ApplicationUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(sa => sa.Vehicle)
            .WithMany(v => v.ServiceAppointments)
            .HasForeignKey(sa => sa.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(sa => sa.ApplicationUserId);
        builder.HasIndex(sa => sa.VehicleId);
    }
}
