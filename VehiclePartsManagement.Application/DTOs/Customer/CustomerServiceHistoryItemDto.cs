namespace VehiclePartsManagement.Application.DTOs.Customer;

public class CustomerServiceHistoryItemDto
{
    public int ServiceRecordId { get; set; }
    public DateTime ServiceDate { get; set; }
    public string ServiceType { get; set; } = string.Empty;
    public string Diagnosis { get; set; } = string.Empty;
    public string WorkDone { get; set; } = string.Empty;
    public int MileageAtService { get; set; }
    public decimal LaborCharge { get; set; }
    public DateTime? NextServiceDueDate { get; set; }
    public int VehicleId { get; set; }
    public string VehicleNumber { get; set; } = string.Empty;
}
