using System.ComponentModel.DataAnnotations;

namespace VehiclePartsManagement.Application.DTOs.Vendor;

public class VendorListQueryDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Page must be greater than 0")]
    public int Page { get; set; } = 1;

    [Range(1, 200, ErrorMessage = "PageSize must be between 1 and 200")]
    public int PageSize { get; set; } = 10;

    public string? Search { get; set; }

    public bool? IsActive { get; set; }
}