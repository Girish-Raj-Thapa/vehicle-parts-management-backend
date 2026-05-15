using System.ComponentModel.DataAnnotations;

namespace VehiclePartsManagement.Application.DTOs.Purchase;

public class CreatePurchaseItemDto
{
    [Required]
    public int PartId { get; set; }

    [Range(1, int.MaxValue)]
    public int QuantityPurchased { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal UnitPriceAtPurchase { get; set; }
}
