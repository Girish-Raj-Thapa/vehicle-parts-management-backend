using System.ComponentModel.DataAnnotations;

namespace VehiclePartsManagement.Application.DTOs.Vendor;

public class UpdateVendorStatusDto
{
    [Required]
    public bool? IsActive { get; set; }
}
