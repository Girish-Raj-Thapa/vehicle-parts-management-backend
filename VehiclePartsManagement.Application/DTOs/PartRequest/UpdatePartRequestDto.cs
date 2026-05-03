using System.ComponentModel.DataAnnotations;

namespace VehiclePartsManagement.Application.DTOs.PartRequest;

/// <summary>
/// Request payload for staff/admin part-request status update.
/// </summary>
public class UpdatePartRequestDto
{
    [Required]
    [MaxLength(50)]
    public string RequestStatus { get; set; } = string.Empty;

    public int? FulfilledPartId { get; set; }
}
