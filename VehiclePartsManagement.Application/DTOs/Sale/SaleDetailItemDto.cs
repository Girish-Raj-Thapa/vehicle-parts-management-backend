namespace VehiclePartsManagement.Application.DTOs.Sale;

/// <summary>
/// Sale line item in detailed sale response.
/// </summary>
public class SaleDetailItemDto
{
    public int PartId { get; set; }
    public string PartName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPriceAtSale { get; set; }
    public decimal LineTotal { get; set; }
}
