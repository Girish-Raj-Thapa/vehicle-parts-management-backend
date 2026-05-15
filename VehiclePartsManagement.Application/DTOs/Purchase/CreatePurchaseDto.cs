using System.ComponentModel.DataAnnotations;

namespace VehiclePartsManagement.Application.DTOs.Purchase;

public class CreatePurchaseDto
{
    [Required]
    public int VendorId { get; set; }

    public DateOnly? PurchaseDate { get; set; }

    [MinLength(1)]
    public List<CreatePurchaseItemDto> Items { get; set; } = [];
}
