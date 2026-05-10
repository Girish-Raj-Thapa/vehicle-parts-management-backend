namespace VehiclePartsManagement.Application.DTOs.Vehicle;

/// <summary>
/// Service history row for a specific vehicle.
/// </summary>
public class VehicleServiceHistoryDto
{
    public int ServiceRecordId { get; set; }
    public int AppointmentId { get; set; }
    public DateTime ServiceDate { get; set; }
    public string ServiceType { get; set; } = string.Empty;
    public string Diagnosis { get; set; } = string.Empty;
    public string WorkDone { get; set; } = string.Empty;
    public int MileageAtService { get; set; }
    public decimal LaborCharge { get; set; }
    public DateTime? NextServiceDueDate { get; set; }
    public int? ReviewRating { get; set; }
    public string? ReviewComment { get; set; }
}
