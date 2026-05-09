namespace VehiclePartsManagement.Application.DTOs.Customer;

public class CustomerServiceRecordDetailDto
{
    public int ServiceRecordId { get; set; }
    public int ServiceAppointmentId { get; set; }
    public DateOnly ServiceDate { get; set; }
    public CustomerHistoryVehicleDto Vehicle { get; set; } = new();
    public CustomerServiceAppointmentHistoryDto Appointment { get; set; } = new();
    public string ServiceType { get; set; } = string.Empty;
    public string Diagnosis { get; set; } = string.Empty;
    public string WorkDone { get; set; } = string.Empty;
    public int MileageAtService { get; set; }
    public decimal LaborCharge { get; set; }
    public DateOnly? NextServiceDueDate { get; set; }
}
