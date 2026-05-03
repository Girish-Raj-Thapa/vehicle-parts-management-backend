namespace VehiclePartsManagement.Application.DTOs.ServiceAppointment;

/// <summary>
/// Response payload for successful appointment creation.
/// </summary>
public class ServiceAppointmentCreateResponseDto
{
    public int AppointmentId { get; set; }
    public string AppointmentStatus { get; set; } = string.Empty;
}
