using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VehiclePartsManagement.Domain.Entities;

public class Cart
{
    [Key]
    public int CartId { get; set; }

    [Required]
    [MaxLength(450)]
    public string CustomerApplicationUserId { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(CustomerApplicationUserId))]
    public ApplicationUser CustomerUser { get; set; } = null!;

    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
}
