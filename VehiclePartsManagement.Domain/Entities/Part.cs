using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VehiclePartsManagement.Domain.Entities;

public class Part
{
    [Key]
    public int PartId { get; set; }

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

    [Column(TypeName = "numeric(18,2)")]
    public decimal SellingPrice { get; set; } = 0;

    public int CurrentStock { get; set; } = 0;

    public int LowStockThreshold { get; set; } = 10;

    public bool IsActive { get; set; } = true;

    [ForeignKey(nameof(PartCategoryId))]
    public PartCategory Category { get; set; } = null!;

    public ICollection<PartCompatibility> PartCompatibilities { get; set; } = new List<PartCompatibility>();
    public ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();
    public ICollection<PurchaseItem> PurchaseItems { get; set; } = new List<PurchaseItem>();
    public ICollection<SaleItem> SaleItems { get; set; } = new List<SaleItem>();
    public ICollection<PartRequest> FulfilledPartRequests { get; set; } = new List<PartRequest>();
    public ICollection<PartImage> PartImages { get; set; } = new List<PartImage>();
}
