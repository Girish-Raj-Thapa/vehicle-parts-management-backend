using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VehiclePartsManagement.Domain.Enums;

namespace VehiclePartsManagement.Domain.Entities;

public class StockMovement
{
    [Key]
    public int StockMovementId { get; set; }

    [Required]
    public int PartId { get; set; }

    [Required]
    public StockMovementType MovementType { get; set; } = StockMovementType.Adjustment;

    [Required]
    public int MovementQuantity { get; set; }

    public DateTime MovementDate { get; set; } = DateTime.UtcNow;

    [Required]
    public StockMovementReferenceType ReferenceType { get; set; } = StockMovementReferenceType.ManualAdjustment;

    [Required]
    public int ReferenceId { get; set; }

    [ForeignKey(nameof(PartId))]
    public Part Part { get; set; } = null!;
}
