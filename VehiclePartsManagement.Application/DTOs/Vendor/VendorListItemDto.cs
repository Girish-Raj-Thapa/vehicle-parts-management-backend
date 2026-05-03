namespace VehiclePartsManagement.Application.DTOs.Vendor;

public class VendorListItemDto
{
    public int VendorId { get; set; }
    public string VendorName { get; set; } = string.Empty;
    public string ContactPerson { get; set; } = string.Empty;
    public string VendorEmail { get; set; } = string.Empty;
    public string VendorPhone { get; set; } = string.Empty;
    public string VendorAddress { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}