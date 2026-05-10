using System.ComponentModel.DataAnnotations;

namespace VehiclePartsManagement.Application.DTOs.Part;

public class CreatePartDto
{
    [Required]
    public int PartCategoryId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Sku { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string PartName { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string PartDescription { get; set; } = string.Empty;

    [Range(0, 9999999999999999.99)]
    public decimal SellingPrice { get; set; }

    [Range(0, int.MaxValue)]
    public int LowStockThreshold { get; set; } = 10;
}
