using System.ComponentModel.DataAnnotations;

namespace VehiclePartsManagement.Application.DTOs.Vendor;

public class UpdateVendorDto
{
    [Required]
    [MaxLength(150)]
    public string VendorName { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string ContactPerson { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public string VendorEmail { get; set; } = string.Empty;

    [Required]
    [Phone]
    [MaxLength(20)]
    public string VendorPhone { get; set; } = string.Empty;

    [Required]
    [MaxLength(300)]
    public string VendorAddress { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}