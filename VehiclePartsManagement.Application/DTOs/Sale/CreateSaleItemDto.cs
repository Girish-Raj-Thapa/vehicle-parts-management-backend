using System.ComponentModel.DataAnnotations;

namespace VehiclePartsManagement.Application.DTOs.Sale;

/// <summary>
/// Line item payload for creating a sale.
/// </summary>
public class CreateSaleItemDto
{
    [Required]
    public int PartId { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }
}
