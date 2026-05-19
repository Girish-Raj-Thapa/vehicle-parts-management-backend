using System.ComponentModel.DataAnnotations;

namespace VehiclePartsManagement.Application.DTOs.Sale;

/// <summary>
/// Query parameters for filtering paged sales list.
/// </summary>
public class SaleListQueryDto
{
    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;

    [Range(1, 100)]
    public int PageSize { get; set; } = 10;

    public bool? Mine { get; set; }

    [MaxLength(30)]
    public string? PaymentStatus { get; set; }
    [MaxLength(30)]
    public string? SaleChannel { get; set; }

    public string? CustomerUserId { get; set; }

    public DateOnly? FromDate { get; set; }

    public DateOnly? ToDate { get; set; }
}
