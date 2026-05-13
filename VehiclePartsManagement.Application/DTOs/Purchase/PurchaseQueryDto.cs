using System.ComponentModel.DataAnnotations;

namespace VehiclePartsManagement.Application.DTOs.Purchase;

public class PurchaseQueryDto
{
    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;

    [Range(1, 100)]
    public int PageSize { get; set; } = 10;

    public int? VendorId { get; set; }

    public DateOnly? FromDate { get; set; }

    public DateOnly? ToDate { get; set; }
}
