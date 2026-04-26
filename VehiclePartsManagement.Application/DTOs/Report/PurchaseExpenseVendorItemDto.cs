namespace VehiclePartsManagement.Application.DTOs.Report;

public class PurchaseExpenseVendorItemDto
{
    public int VendorId { get; set; }
    public string VendorName { get; set; } = string.Empty;
    public decimal PurchaseAmount { get; set; }
    public int InvoiceCount { get; set; }
}