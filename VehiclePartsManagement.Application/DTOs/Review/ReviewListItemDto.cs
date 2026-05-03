namespace VehiclePartsManagement.Application.DTOs.Review;

/// <summary>
/// List row for review listing responses.
/// </summary>
public class ReviewListItemDto
{
    public int ReviewId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string ReviewComment { get; set; } = string.Empty;
    public DateOnly ReviewDate { get; set; }
}
