namespace VehiclePartsManagement.Application.DTOs.Customer;

public class CustomerPurchaseItemDto
{
    public int SaleItemId { get; set; }
    public int PartId { get; set; }
    public string PartName { get; set; } = string.Empty;
    public int QuantitySold { get; set; }
    public decimal UnitPriceAtSale { get; set; }
    public decimal LineTotal { get; set; }
}
