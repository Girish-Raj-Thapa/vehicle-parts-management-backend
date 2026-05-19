using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VehiclePartsManagement.Domain.Entities;

public class CartItem
{
    [Key]
    public int CartItemId { get; set; }

    [Required]
    public int CartId { get; set; }

    [Required]
    public int PartId { get; set; }

    [Required]
    public int Quantity { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(CartId))]
    public Cart Cart { get; set; } = null!;

    [ForeignKey(nameof(PartId))]
    public Part Part { get; set; } = null!;
}
