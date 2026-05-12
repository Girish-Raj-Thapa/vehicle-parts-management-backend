namespace VehiclePartsManagement.Application.DTOs.Payment;

public class KhaltiInitiateResponseDto
{
    public int SaleId { get; set; }
    public string Pidx { get; set; } = string.Empty;
    public string PaymentUrl { get; set; } = string.Empty;
    public int AmountPaisa { get; set; }
    public string PurchaseOrderId { get; set; } = string.Empty;
    public DateTime? ExpiresAt { get; set; }
    public int? ExpiresIn { get; set; }
}
