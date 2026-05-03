namespace VehiclePartsManagement.Application.DTOs.ServiceAppointment;

/// <summary>
/// Detailed appointment payload for single appointment endpoint.
/// </summary>
public class ServiceAppointmentDetailDto
{
    public int AppointmentId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public int VehicleId { get; set; }
    public string VehicleNumber { get; set; } = string.Empty;
    public string VehicleName { get; set; } = string.Empty;
    public DateOnly AppointmentDate { get; set; }
    public string AppointmentTime { get; set; } = string.Empty;
    public string AppointmentType { get; set; } = string.Empty;
    public string AppointmentStatus { get; set; } = string.Empty;
    public string AppointmentNote { get; set; } = string.Empty;
}
