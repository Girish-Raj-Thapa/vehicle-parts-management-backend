namespace VehiclePartsManagement.Application.DTOs.Review;

/// <summary>
/// Detailed review payload for single-review retrieval.
/// </summary>
public class ReviewDetailDto
{
    public int ReviewId { get; set; }
    public int ServiceRecordId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string VehicleName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string ReviewComment { get; set; } = string.Empty;
    public DateOnly ReviewDate { get; set; }
}
