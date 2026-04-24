using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VehiclePartsManagement.Domain.Entities;

public class SaleItem
{
    [Key]
    public int SaleItemId { get; set; }

    [Required]
    public int SaleId { get; set; }

    [Required]
    public int PartId { get; set; }

    [Required]
    public int QuantitySold { get; set; }

    [Column(TypeName = "numeric(18,2)")]
    public decimal UnitPriceAtSale { get; set; } = 0;

    [Column(TypeName = "numeric(18,2)")]
    public decimal LineTotal { get; set; } = 0;

    [ForeignKey(nameof(SaleId))]
    public Sale Sale { get; set; } = null!;

    [ForeignKey(nameof(PartId))]
    public Part Part { get; set; } = null!;
}
