namespace VehiclePartsManagement.Application.DTOs.Reports;

public class PendingCreditReportDto
{
    public string CustomerId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public int SaleId { get; set; }
    public string InvoiceNo { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal BalanceDue { get; set; }
    public DateTime? DueDate { get; set; }
    public bool IsOverdue { get; set; }
}
