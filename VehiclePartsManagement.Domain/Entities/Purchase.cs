using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VehiclePartsManagement.Domain.Enums;

namespace VehiclePartsManagement.Domain.Entities;

public class Purchase
{
    [Key]
    public int PurchaseId { get; set; }

    [Required]
    public int VendorId { get; set; }

    [Required]
    [MaxLength(450)]
    public string CreatedByApplicationUserId { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string PurchaseInvoiceNo { get; set; } = string.Empty;

    public DateTime PurchaseDate { get; set; } = DateTime.UtcNow;

    [Required]
    public PurchaseStatus PurchaseStatus { get; set; } = PurchaseStatus.Pending;

    [Column(TypeName = "numeric(18,2)")]
    public decimal TotalAmount { get; set; } = 0;

    [ForeignKey(nameof(VendorId))]
    public Vendor Vendor { get; set; } = null!;

    [ForeignKey(nameof(CreatedByApplicationUserId))]
    public ApplicationUser CreatedByUser { get; set; } = null!;

    public ICollection<PurchaseItem> PurchaseItems { get; set; } = new List<PurchaseItem>();
}
