namespace VehiclePartsManagement.Application.DTOs.ServiceAppointment;

/// <summary>
/// List row for paged appointment responses.
/// </summary>
public class ServiceAppointmentListItemDto
{
    public int AppointmentId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string VehicleNumber { get; set; } = string.Empty;
    public string VehicleName { get; set; } = string.Empty;
    public DateOnly AppointmentDate { get; set; }
    public string AppointmentTime { get; set; } = string.Empty;
    public string AppointmentType { get; set; } = string.Empty;
    public string AppointmentStatus { get; set; } = string.Empty;
    public string AppointmentNote { get; set; } = string.Empty;
}
