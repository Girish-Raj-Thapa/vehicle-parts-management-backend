namespace VehiclePartsManagement.Application.DTOs.Review;

/// <summary>
/// List row for featured review responses.
/// </summary>
public class FeaturedReviewListItemDto
{
    public int ReviewId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string VehicleName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string ReviewComment { get; set; } = string.Empty;
}
