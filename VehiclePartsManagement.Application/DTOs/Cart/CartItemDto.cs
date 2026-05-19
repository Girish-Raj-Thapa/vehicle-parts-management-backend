namespace VehiclePartsManagement.Application.DTOs.Cart;

public class CartItemDto
{
    public int CartItemId { get; set; }
    public int PartId { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string PartName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public int CurrentStock { get; set; }
    public bool IsActive { get; set; }
    public decimal LineTotal { get; set; }
}
