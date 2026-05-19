using System.ComponentModel.DataAnnotations;

namespace VehiclePartsManagement.Application.DTOs.Cart;

public class AddCartItemDto
{
    [Required]
    public int PartId { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }
}
