using System.ComponentModel.DataAnnotations;

namespace VehiclePartsManagement.Application.DTOs.PartRequest;

/// <summary>
/// Request payload for creating a customer part request.
/// </summary>
public class CreatePartRequestDto
{
    [Required]
    public int VehicleId { get; set; }

    [Required]
    [MaxLength(150)]
    public string RequestedPartName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string RequestDescription { get; set; } = string.Empty;
}
