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

    [MaxLength(500)]
    public string? ProfileImagePath { get; set; }

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

    public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
    public ICollection<Purchase> CreatedPurchases { get; set; } = new List<Purchase>();
    public ICollection<Sale> CustomerSales { get; set; } = new List<Sale>();
    public ICollection<Sale> ProcessedSales { get; set; } = new List<Sale>();
    public ICollection<Cart> Carts { get; set; } = new List<Cart>();
    public ICollection<ServiceAppointment> ServiceAppointments { get; set; } = new List<ServiceAppointment>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<PartRequest> PartRequests { get; set; } = new List<PartRequest>();
}
