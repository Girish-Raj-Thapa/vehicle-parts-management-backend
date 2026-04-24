using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VehiclePartsManagement.Domain.Enums;

namespace VehiclePartsManagement.Domain.Entities;

public class Vehicle
{
    [Key]
    public int VehicleId { get; set; }

    [Required]
    [MaxLength(450)]
    public string ApplicationUserId { get; set; } = string.Empty;

    [Required]
    public int VehicleModelId { get; set; }

    [Required]
    [MaxLength(50)]
    public string VehicleNumber { get; set; } = string.Empty;

    public int? ManufactureYear { get; set; }

    [MaxLength(100)]
    public string EngineNumber { get; set; } = string.Empty;

    [MaxLength(100)]
    public string ChassisNumber { get; set; } = string.Empty;

    [Required]
    public FuelType FuelType { get; set; } = FuelType.Petrol;

    [MaxLength(30)]
    public string Color { get; set; } = string.Empty;

    public int CurrentMileage { get; set; } = 0;

    public DateTime VehicleRegisteredAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(ApplicationUserId))]
    public ApplicationUser User { get; set; } = null!;

    [ForeignKey(nameof(VehicleModelId))]
    public VehicleModel Model { get; set; } = null!;

    public ICollection<Sale> Sales { get; set; } = new List<Sale>();
    public ICollection<ServiceAppointment> ServiceAppointments { get; set; } = new List<ServiceAppointment>();
    public ICollection<ServiceRecord> ServiceRecords { get; set; } = new List<ServiceRecord>();
    public ICollection<PartRequest> PartRequests { get; set; } = new List<PartRequest>();
}
