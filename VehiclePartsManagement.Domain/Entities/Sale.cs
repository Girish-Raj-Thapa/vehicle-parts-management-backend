using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VehiclePartsManagement.Domain.Enums;

namespace VehiclePartsManagement.Domain.Entities;

public class Sale
{
    [Key]
    public int SaleId { get; set; }

    [Required]
    [MaxLength(450)]
    public string CustomerApplicationUserId { get; set; } = string.Empty;

    public int? VehicleId { get; set; }

    [MaxLength(450)]
    public string? ProcessedByApplicationUserId { get; set; }

    [Required]
    [MaxLength(100)]
    public string SaleInvoiceNo { get; set; } = string.Empty;

    public DateTime SaleDate { get; set; } = DateTime.UtcNow;

    [Required]
    public SaleStatus SaleStatus { get; set; } = SaleStatus.Draft;

    [Required]
    public SaleChannel SaleChannel { get; set; } = SaleChannel.Workshop;

    [Column(TypeName = "numeric(18,2)")]
    public decimal SubTotal { get; set; } = 0;

    [Column(TypeName = "numeric(5,2)")]
    public decimal LoyaltyDiscountPercent { get; set; } = 0;

    [Column(TypeName = "numeric(18,2)")]
    public decimal DiscountAmount { get; set; } = 0;

    [Column(TypeName = "numeric(18,2)")]
    public decimal TotalAmount { get; set; } = 0;

    [Column(TypeName = "numeric(18,2)")]
    public decimal BalanceDue { get; set; } = 0;

    [Required]
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Unpaid;

    public DateTime? DueDate { get; set; }

    [ForeignKey(nameof(CustomerApplicationUserId))]
    public ApplicationUser CustomerUser { get; set; } = null!;

    [ForeignKey(nameof(VehicleId))]
    public Vehicle? Vehicle { get; set; }

    [ForeignKey(nameof(ProcessedByApplicationUserId))]
    public ApplicationUser? ProcessedByUser { get; set; }

    public ICollection<SaleItem> SaleItems { get; set; } = new List<SaleItem>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
