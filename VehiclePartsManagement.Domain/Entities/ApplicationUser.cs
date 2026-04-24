using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace VehiclePartsManagement.Domain.Entities;

public class ApplicationUser : IdentityUser
{
    [Required]
    [MaxLength(150)]
    public string FullName { get; set; } = string.Empty;

    [MaxLength(300)]
    public string Address { get; set; } = string.Empty;

    [Phone]
    [MaxLength(10)]
    public override string? PhoneNumber { get; set; }

    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;

    [MaxLength(450)]
    public string? CreatedByUserId { get; set; }

    [ForeignKey(nameof(CreatedByUserId))]
    public ApplicationUser? CreatedByUser { get; set; }

    public ICollection<ApplicationUser> CreatedUsers { get; set; } = new List<ApplicationUser>();
}
