namespace VehiclePartsManagement.Application.DTOs.Part;

public class PartQueryDto
{
    public string? Search { get; set; }
    public int? CategoryId { get; set; }
    public int? ModelId { get; set; }
    public bool? InStock { get; set; }
    public bool? IsActive { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
}
