namespace VehiclePartsManagement.Application.DTOs.Customer;

public class CustomerServiceAppointmentHistoryDto
{
    public DateOnly AppointmentDate { get; set; }
    public string AppointmentTime { get; set; } = string.Empty;
    public string AppointmentType { get; set; } = string.Empty;
    public string AppointmentStatus { get; set; } = string.Empty;
    public string AppointmentNote { get; set; } = string.Empty;
}
