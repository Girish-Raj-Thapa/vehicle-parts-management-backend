using System.ComponentModel.DataAnnotations;

namespace VehiclePartsManagement.Domain.Entities;

public class Vendor
{
    [Key]
    public int VendorId { get; set; }

    [Required]
    [MaxLength(150)]
    public string VendorName { get; set; } = string.Empty;

    [MaxLength(150)]
    public string ContactPerson { get; set; } = string.Empty;

    [EmailAddress]
    [MaxLength(256)]
    public string VendorEmail { get; set; } = string.Empty;

    [Phone]
    [MaxLength(20)]
    public string VendorPhone { get; set; } = string.Empty;

    [MaxLength(300)]
    public string VendorAddress { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public ICollection<Purchase> Purchases { get; set; } = new List<Purchase>();
}
