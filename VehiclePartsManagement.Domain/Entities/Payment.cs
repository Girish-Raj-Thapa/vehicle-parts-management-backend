using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VehiclePartsManagement.Domain.Enums;

namespace VehiclePartsManagement.Domain.Entities;

public class Payment
{
    [Key]
    public int PaymentId { get; set; }

    [Required]
    public int SaleId { get; set; }

    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

    [Required]
    public PaymentMethodType PaymentMethod { get; set; } = PaymentMethodType.Cash;

    [Column(TypeName = "numeric(18,2)")]
    public decimal PaymentAmount { get; set; } = 0;

    [MaxLength(100)]
    public string PaymentReference { get; set; } = string.Empty;

    [MaxLength(500)]
    public string PaymentNote { get; set; } = string.Empty;

    [ForeignKey(nameof(SaleId))]
    public Sale Sale { get; set; } = null!;
}
