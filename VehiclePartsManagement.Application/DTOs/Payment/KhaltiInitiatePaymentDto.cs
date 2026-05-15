using System.ComponentModel.DataAnnotations;

namespace VehiclePartsManagement.Application.DTOs.Payment;

public class KhaltiInitiatePaymentDto
{
    [Required]
    public int SaleId { get; set; }
}
