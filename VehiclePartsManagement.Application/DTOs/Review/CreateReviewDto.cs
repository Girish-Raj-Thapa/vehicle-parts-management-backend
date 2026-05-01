using System.ComponentModel.DataAnnotations;

namespace VehiclePartsManagement.Application.DTOs.Review;

/// <summary>
/// Request payload for customer review submission.
/// </summary>
public class CreateReviewDto
{
    [Required]
    public int ServiceRecordId { get; set; }

    [Range(1, 5)]
    public int Rating { get; set; }

    [MaxLength(500)]
    public string ReviewComment { get; set; } = string.Empty;
}
