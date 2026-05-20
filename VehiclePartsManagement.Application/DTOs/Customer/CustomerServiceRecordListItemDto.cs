namespace VehiclePartsManagement.Application.DTOs.Customer;

public class CustomerServiceRecordListItemDto
{
    public int ServiceRecordId { get; set; }
    public int ServiceAppointmentId { get; set; }
    public DateOnly ServiceDate { get; set; }
    public int VehicleId { get; set; }
    public string VehicleNumber { get; set; } = string.Empty;
    public string VehicleName { get; set; } = string.Empty;
    public string ServiceType { get; set; } = string.Empty;
    public string Diagnosis { get; set; } = string.Empty;
    public string WorkDone { get; set; } = string.Empty;
    public int MileageAtService { get; set; }
    public decimal LaborCharge { get; set; }
    public DateOnly? NextServiceDueDate { get; set; }
    public bool HasReview { get; set; }
    public int? ReviewId { get; set; }
}
