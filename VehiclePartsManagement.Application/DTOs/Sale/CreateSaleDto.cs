using System.ComponentModel.DataAnnotations;

namespace VehiclePartsManagement.Application.DTOs.Sale;

public class CreateSaleDto
{
    [Required]
    [MaxLength(450)]
    public string CustomerApplicationUserId { get; set; } = string.Empty;

    [Required]
    public int VehicleId { get; set; }

    [Required]
    [MinLength(1, ErrorMessage = "At least one item is required.")]
    public List<CreateSaleItemDto> Items { get; set; } = [];
}

public class CreateSaleItemDto
{
    [Required]
    public int PartId { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
    public int Quantity { get; set; }
}
