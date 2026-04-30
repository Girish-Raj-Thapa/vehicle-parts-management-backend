using System.ComponentModel.DataAnnotations;

namespace VehiclePartsManagement.Application.DTOs.PartRequest;

/// <summary>
/// Query payload for paged part-request listing.
/// </summary>
public class PartRequestQueryDto
{
    public bool Mine { get; set; } = false;

    [MaxLength(50)]
    public string? Status { get; set; }

    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;

    [Range(1, 100)]
    public int PageSize { get; set; } = 10;
}
