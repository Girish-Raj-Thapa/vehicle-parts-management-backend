namespace VehiclePartsManagement.Application.DTOs.PartRequest;

/// <summary>
/// Response payload for successful part-request update.
/// </summary>
public class PartRequestUpdateResponseDto
{
    public int PartRequestId { get; set; }
    public string RequestStatus { get; set; } = string.Empty;
    public int? FulfilledPartId { get; set; }
}
