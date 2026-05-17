using VehiclePartsManagement.Application.Common;

namespace VehiclePartsManagement.Application.DTOs.Report;

public class ReceivableReportResponseDto
{
    public ReportMetaDto Meta { get; set; } = new();
    public decimal TotalCreditOutstanding { get; set; }
    public decimal OverdueCreditAmount { get; set; }
    public List<ReceivableCustomerBalanceItemDto> CustomerBalances { get; set; } = [];
    public List<OverdueInvoiceItemDto> OverdueInvoices { get; set; } = [];
    public PaginationMeta CustomerBalancesPagination { get; set; } = new();
    public PaginationMeta OverdueInvoicesPagination { get; set; } = new();
}
