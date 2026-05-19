namespace VehiclePartsManagement.Application.DTOs.Cart;

public class CartDto
{
    public int CartId { get; set; }
    public string CustomerUserId { get; set; } = string.Empty;
    public List<CartItemDto> Items { get; set; } = [];
    public int TotalUniqueItems { get; set; }
    public int TotalQuantity { get; set; }
    public decimal SubTotal { get; set; }
    public decimal LoyaltyDiscountPercent { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime UpdatedAt { get; set; }
}
