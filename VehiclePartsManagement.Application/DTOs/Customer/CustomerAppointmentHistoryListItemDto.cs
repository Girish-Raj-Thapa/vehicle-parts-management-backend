namespace VehiclePartsManagement.Application.DTOs.Customer;

public class CustomerAppointmentHistoryListItemDto
{
    public int AppointmentId { get; set; }
    public int VehicleId { get; set; }
    public string VehicleNumber { get; set; } = string.Empty;
    public string VehicleName { get; set; } = string.Empty;
    public DateOnly AppointmentDate { get; set; }
    public string AppointmentTime { get; set; } = string.Empty;
    public string AppointmentType { get; set; } = string.Empty;
    public string AppointmentStatus { get; set; } = string.Empty;
    public string AppointmentNote { get; set; } = string.Empty;
}
