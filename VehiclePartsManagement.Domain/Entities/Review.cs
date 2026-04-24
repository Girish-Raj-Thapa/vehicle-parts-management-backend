using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VehiclePartsManagement.Domain.Entities;

public class Review
{
    [Key]
    public int ReviewId { get; set; }

    [Required]
    public int ServiceRecordId { get; set; }

    [Required]
    [MaxLength(450)]
    public string ApplicationUserId { get; set; } = string.Empty;

    [Range(1, 5)]
    public int Rating { get; set; }

    [MaxLength(1000)]
    public string ReviewComment { get; set; } = string.Empty;

    public DateTime ReviewDate { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(ServiceRecordId))]
    public ServiceRecord ServiceRecord { get; set; } = null!;

    [ForeignKey(nameof(ApplicationUserId))]
    public ApplicationUser User { get; set; } = null!;
}
