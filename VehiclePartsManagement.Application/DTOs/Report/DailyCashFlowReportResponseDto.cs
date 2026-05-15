using VehiclePartsManagement.Application.Common;

namespace VehiclePartsManagement.Application.DTOs.Report;

public class DailyCashFlowReportResponseDto
{
    public ReportMetaDto Meta { get; set; } = new();
    public decimal OpeningBalance { get; set; }
    public decimal MoneyReceived { get; set; }
    public decimal MoneySpentOnPurchases { get; set; }
    public decimal ClosingBalance { get; set; }
    public List<PaymentMethodBreakdownItemDto> PaymentMethodBreakdown { get; set; } = [];
    public List<DailyCashFlowDayItemDto> DailyFlows { get; set; } = [];
    public PaginationMeta PaymentMethodBreakdownPagination { get; set; } = new();
    public PaginationMeta DailyFlowsPagination { get; set; } = new();
}
