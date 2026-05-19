using System.ComponentModel.DataAnnotations;

namespace VehiclePartsManagement.Application.DTOs.Part;

public class UpdatePartDto
{
    public int? PartCategoryId { get; set; }

    [MaxLength(100)]
    public string? Sku { get; set; }

    [MaxLength(150)]
    public string? PartName { get; set; }

    [MaxLength(1000)]
    public string? PartDescription { get; set; }

    [Range(0, 9999999999999999.99)]
    public decimal? SellingPrice { get; set; }

    [Range(0, int.MaxValue)]
    public int? LowStockThreshold { get; set; }

    public bool? IsActive { get; set; }

    [Range(0, int.MaxValue)]
    public int? CurrentStock { get; set; }
}
