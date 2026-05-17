using System.ComponentModel.DataAnnotations;

namespace VehiclePartsManagement.Application.DTOs.Sale;

/// <summary>
/// Optional initial payment payload submitted during sale creation.
/// </summary>
public class CreateSalePaymentDto
{
    [Required]
    [MaxLength(30)]
    public string PaymentMethod { get; set; } = string.Empty;

    [Range(0, double.MaxValue)]
    public decimal PaymentAmount { get; set; }

    [MaxLength(100)]
    public string PaymentReference { get; set; } = string.Empty;

    [MaxLength(500)]
    public string PaymentNote { get; set; } = string.Empty;
}
