namespace VehiclePartsManagement.Application.DTOs.ServiceAppointment;

/// <summary>
/// Response payload for successful appointment update operations.
/// </summary>
public class ServiceAppointmentUpdateResponseDto
{
    public int AppointmentId { get; set; }
    public string AppointmentStatus { get; set; } = string.Empty;
}
