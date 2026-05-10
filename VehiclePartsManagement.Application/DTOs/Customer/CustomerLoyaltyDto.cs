namespace VehiclePartsManagement.Application.DTOs.Customer;

public class CustomerLoyaltyDto
{
    public string UserId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public int TotalSalesCount { get; set; }
    public decimal TotalAmountSpent { get; set; }
    public string LoyaltyTier { get; set; } = string.Empty;
    public decimal EligibleDiscountPercent { get; set; }
}
