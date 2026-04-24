using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VehiclePartsManagement.Domain.Entities;

public class ServiceRecord
{
    [Key]
    public int ServiceRecordId { get; set; }

    [Required]
    public int ServiceAppointmentId { get; set; }

    [Required]
    public int VehicleId { get; set; }

    public DateTime ServiceDate { get; set; } = DateTime.UtcNow;

    [Required]
    [MaxLength(50)]
    public string ServiceType { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string Diagnosis { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string WorkDone { get; set; } = string.Empty;

    public int MileageAtService { get; set; } = 0;

    [Column(TypeName = "numeric(18,2)")]
    public decimal LaborCharge { get; set; } = 0;

    public DateTime? NextServiceDueDate { get; set; }

    [ForeignKey(nameof(ServiceAppointmentId))]
    public ServiceAppointment ServiceAppointment { get; set; } = null!;

    [ForeignKey(nameof(VehicleId))]
    public Vehicle Vehicle { get; set; } = null!;

    public Review? Review { get; set; }
}
