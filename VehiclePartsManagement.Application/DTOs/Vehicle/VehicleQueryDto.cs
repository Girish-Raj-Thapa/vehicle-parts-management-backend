using System.ComponentModel.DataAnnotations;

namespace VehiclePartsManagement.Application.DTOs.Vehicle;

/// <summary>
/// Query contract for the vehicle listing endpoint.
/// </summary>
public class VehicleQueryDto
{
    /// <summary>
    /// When true, limits results to the authenticated user's vehicles.
    /// </summary>
    public bool Mine { get; set; } = false;

    /// <summary>
    /// Optional search term applied to vehicle number, brand, or model.
    /// </summary>
    [MaxLength(100)]
    public string? Search { get; set; }

    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;

    [Range(1, 100)]
    public int PageSize { get; set; } = 10;
}
