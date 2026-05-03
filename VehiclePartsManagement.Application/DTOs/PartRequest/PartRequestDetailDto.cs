namespace VehiclePartsManagement.Application.DTOs.PartRequest;

/// <summary>
/// Detailed payload for a single part request.
/// </summary>
public class PartRequestDetailDto
{
    public int PartRequestId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public int? VehicleId { get; set; }
    public string VehicleName { get; set; } = string.Empty;
    public string RequestedPartName { get; set; } = string.Empty;
    public string RequestDescription { get; set; } = string.Empty;
    public DateOnly RequestDate { get; set; }
    public string RequestStatus { get; set; } = string.Empty;
    public int? FulfilledPartId { get; set; }
}
