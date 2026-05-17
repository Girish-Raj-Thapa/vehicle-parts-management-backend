namespace VehiclePartsManagement.Application.DTOs.Sale;

/// <summary>
/// Invoice line item for sale invoice responses.
/// </summary>
public class SaleInvoiceItemDto
{
    public string PartName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}
