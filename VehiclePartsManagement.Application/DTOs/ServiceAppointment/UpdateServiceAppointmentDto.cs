using System.ComponentModel.DataAnnotations;

namespace VehiclePartsManagement.Application.DTOs.ServiceAppointment;

/// <summary>
/// Request payload for staff/admin appointment status updates.
/// </summary>
public class UpdateServiceAppointmentDto
{
    [Required]
    [MaxLength(50)]
    public string AppointmentStatus { get; set; } = string.Empty;

    [MaxLength(500)]
    public string AppointmentNote { get; set; } = string.Empty;
}
