namespace VehiclePartsManagement.Application.DTOs.Report;

public class DiscountCustomerItemDto
{
    public string CustomerId { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public decimal DiscountAmount { get; set; }
    public int InvoiceCount { get; set; }
}