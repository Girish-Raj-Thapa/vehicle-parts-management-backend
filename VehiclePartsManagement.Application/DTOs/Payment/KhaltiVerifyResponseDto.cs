namespace VehiclePartsManagement.Application.DTOs.Payment;

public class KhaltiVerifyResponseDto
{
    public int SaleId { get; set; }
    public string Pidx { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? TransactionId { get; set; }
    public int TotalAmountPaisa { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal BalanceDue { get; set; }
    public string PaymentStatus { get; set; } = string.Empty;
}
