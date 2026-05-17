using System.ComponentModel.DataAnnotations;

namespace VehiclePartsManagement.Application.DTOs.Payment;

public class KhaltiVerifyPaymentDto
{
    [Required]
    public string Pidx { get; set; } = string.Empty;

    public int? SaleId { get; set; }
}
