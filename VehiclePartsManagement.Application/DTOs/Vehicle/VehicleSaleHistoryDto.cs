namespace VehiclePartsManagement.Application.DTOs.Vehicle;

/// <summary>
/// Sale history row for a specific vehicle.
/// </summary>
public class VehicleSaleHistoryDto
{
    public int SaleId { get; set; }
    public string SaleInvoiceNo { get; set; } = string.Empty;
    public DateTime SaleDate { get; set; }
    public string SaleStatus { get; set; } = string.Empty;
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string PaymentStatus { get; set; } = string.Empty;
    public string ProcessedBy { get; set; } = string.Empty;
    public List<VehicleSaleItemDto> Items { get; set; } = [];
}

/// <summary>
/// Individual part line item within a sale.
/// </summary>
public class VehicleSaleItemDto
{
    public int PartId { get; set; }
    public string PartName { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public int QuantitySold { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}
