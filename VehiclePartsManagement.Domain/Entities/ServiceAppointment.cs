using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VehiclePartsManagement.Domain.Enums;

namespace VehiclePartsManagement.Domain.Entities;

public class ServiceAppointment
{
    [Key]
    public int ServiceAppointmentId { get; set; }

    [Required]
    [MaxLength(450)]
    public string ApplicationUserId { get; set; } = string.Empty;

    [Required]
    public int VehicleId { get; set; }

    public DateTime AppointmentDate { get; set; }

    [Required]
    [MaxLength(20)]
    public string AppointmentTime { get; set; } = string.Empty;

    [Required]
    public ServiceAppointmentStatus AppointmentStatus { get; set; } = ServiceAppointmentStatus.Pending;

    [Required]
    public ServiceAppointmentType AppointmentType { get; set; } = ServiceAppointmentType.GeneralService;

    [MaxLength(500)]
    public string AppointmentNote { get; set; } = string.Empty;

    [ForeignKey(nameof(ApplicationUserId))]
    public ApplicationUser User { get; set; } = null!;

    [ForeignKey(nameof(VehicleId))]
    public Vehicle Vehicle { get; set; } = null!;

    public ServiceRecord? ServiceRecord { get; set; }
}
