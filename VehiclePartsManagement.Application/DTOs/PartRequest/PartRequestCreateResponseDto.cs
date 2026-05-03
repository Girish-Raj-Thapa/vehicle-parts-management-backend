namespace VehiclePartsManagement.Application.DTOs.PartRequest;

/// <summary>
/// Response payload for successful part-request creation.
/// </summary>
public class PartRequestCreateResponseDto
{
    public int PartRequestId { get; set; }
    public string RequestStatus { get; set; } = string.Empty;
}
