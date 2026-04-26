namespace VehiclePartsManagement.Application.DTOs.Report;

public class PaymentCollectionReportResponseDto
{
    public ReportMetaDto Meta { get; set; } = new();
    public decimal TotalCollectedAmount { get; set; }
    public decimal CashPaymentsAmount { get; set; }
    public decimal OnlinePaymentsAmount { get; set; }
    public decimal BankPaymentsAmount { get; set; }
    public decimal PartialPaymentsAmount { get; set; }
    public decimal UnpaidBalanceAmount { get; set; }
    public List<PaymentMethodBreakdownItemDto> MethodBreakdown { get; set; } = [];
    public List<PaymentCollectionPeriodItemDto> Collections { get; set; } = [];
}