namespace VehiclePartsManagement.Application.DTOs.Customer;

public class CustomerPaymentHistoryListItemDto
{
    public int PaymentId { get; set; }
    public int SaleId { get; set; }
    public string SaleInvoiceNo { get; set; } = string.Empty;
    public DateOnly PaymentDate { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public decimal PaymentAmount { get; set; }
    public string PaymentReference { get; set; } = string.Empty;
    public string PaymentNote { get; set; } = string.Empty;
}
