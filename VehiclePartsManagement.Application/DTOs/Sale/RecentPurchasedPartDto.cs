namespace VehiclePartsManagement.Application.DTOs.Sale;

/// <summary>
/// Customer-facing summary of recently purchased parts from online orders.
/// </summary>
public class RecentPurchasedPartDto
{
    public int PartId { get; set; }
    public string PartName { get; set; } = string.Empty;
    public DateTime LastPurchasedAt { get; set; }
    public int TotalQuantityPurchased { get; set; }
    public int PurchaseCount { get; set; }
}
