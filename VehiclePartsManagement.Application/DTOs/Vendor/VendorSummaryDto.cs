namespace VehiclePartsManagement.Application.DTOs.Vendor;

public class VendorSummaryDto
{
    public int VendorId { get; set; }
    public string VendorName { get; set; } = string.Empty;
    public int TotalPurchaseInvoices { get; set; }
    public decimal TotalPurchaseAmount { get; set; }
    public DateTime? LastPurchaseDate { get; set; }
    public bool IsActive { get; set; }
}