using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VehiclePartsManagement.Domain.Entities;

public class PurchaseItem
{
    [Key]
    public int PurchaseItemId { get; set; }

    [Required]
    public int PurchaseId { get; set; }

    [Required]
    public int PartId { get; set; }

    [Required]
    public int QuantityPurchased { get; set; }

    [Column(TypeName = "numeric(18,2)")]
    public decimal UnitPriceAtPurchase { get; set; } = 0;

    [Column(TypeName = "numeric(18,2)")]
    public decimal PurchaseLineTotal { get; set; } = 0;

    [ForeignKey(nameof(PurchaseId))]
    public Purchase Purchase { get; set; } = null!;

    [ForeignKey(nameof(PartId))]
    public Part Part { get; set; } = null!;
}
