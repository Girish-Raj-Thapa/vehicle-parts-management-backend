using System.ComponentModel.DataAnnotations;

namespace VehiclePartsManagement.Application.DTOs.ServiceAppointment;

/// <summary>
/// Request contract for booking a new service appointment.
/// </summary>
public class CreateServiceAppointmentDto
{
    [Required]
    public int VehicleId { get; set; }

    public int? AppointmentSlotId { get; set; }

    public DateOnly? AppointmentDate { get; set; }

    [MaxLength(20)]
    public string? AppointmentTime { get; set; }

    [Required]
    [MaxLength(50)]
    public string AppointmentType { get; set; } = string.Empty;

    [MaxLength(500)]
    public string AppointmentNote { get; set; } = string.Empty;
}
