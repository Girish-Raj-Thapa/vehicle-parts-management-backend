namespace VehiclePartsManagement.Application.DTOs.PartRequest;

/// <summary>
/// List row for part-request tracking responses.
/// </summary>
public class PartRequestListItemDto
{
    public int PartRequestId { get; set; }
    public string RequestedPartName { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string VehicleName { get; set; } = string.Empty;
    public DateOnly RequestDate { get; set; }
    public string RequestStatus { get; set; } = string.Empty;
    public int? FulfilledPartId { get; set; }
}
