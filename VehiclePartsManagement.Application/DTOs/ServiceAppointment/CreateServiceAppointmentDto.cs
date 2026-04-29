using System.ComponentModel.DataAnnotations;

namespace VehiclePartsManagement.Application.DTOs.ServiceAppointment;

/// <summary>
/// Request contract for booking a new service appointment.
/// </summary>
public class CreateServiceAppointmentDto
{
    [Required]
    public int VehicleId { get; set; }

    [Required]
    public DateOnly AppointmentDate { get; set; }

    [Required]
    [MaxLength(20)]
    public string AppointmentTime { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string AppointmentType { get; set; } = string.Empty;

    [MaxLength(500)]
    public string AppointmentNote { get; set; } = string.Empty;
}
