using VehiclePartsManagement.Application.Common;

namespace VehiclePartsManagement.Application.DTOs.Report;

public class PurchaseExpenseReportResponseDto
{
    public ReportMetaDto Meta { get; set; } = new();
    public decimal TotalPurchaseAmount { get; set; }
    public int TotalPurchaseInvoices { get; set; }
    public List<PurchaseExpenseVendorItemDto> VendorSummaries { get; set; } = [];
    public List<PurchaseExpensePartItemDto> PartSummaries { get; set; } = [];
    public List<PurchaseExpensePeriodItemDto> Collections { get; set; } = [];
    public PaginationMeta VendorSummariesPagination { get; set; } = new();
    public PaginationMeta PartSummariesPagination { get; set; } = new();
    public PaginationMeta CollectionsPagination { get; set; } = new();
}
