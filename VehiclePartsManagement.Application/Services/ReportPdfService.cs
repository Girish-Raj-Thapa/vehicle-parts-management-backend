using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using VehiclePartsManagement.Application.DTOs.Report;
using VehiclePartsManagement.Application.Interfaces.Services;
using VehiclePartsManagement.Application.Services.PdfService.Common;

namespace VehiclePartsManagement.Application.Services;

public class ReportPdfService : IReportPdfService
{
    private const string PrimaryColor = "#1D4ED8";
    private const string SecondaryTextColor = "#475569";
    private const string MutedBackgroundColor = "#F8FAFC";
    private const string BorderColor = "#E2E8F0";
    private const string HeaderBackgroundColor = "#EEF2FF";
    private const string ZebraBackgroundColor = "#F8FAFC";

    static ReportPdfService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public Task<byte[]> GenerateSalesRevenueReportPdfAsync(SalesRevenueReportResponseDto report)
    {
        ArgumentNullException.ThrowIfNull(report);
        return Task.FromResult(GenerateDocument(report.Meta, container =>
        {
            container.Column(column =>
            {
                column.Spacing(12);
                column.Item().ShowEntire().Element(c => ComposeKpiCardGrid(c, new[]
                {
                    ("Total Sales", PdfFormatService.FormatCurrency(report.TotalSalesAmount), (string?)null),
                    ("Total Invoices", report.TotalInvoices.ToString(), null),
                    ("Cash Sales", PdfFormatService.FormatCurrency(report.CashSalesAmount), null),
                    ("Credit Sales", PdfFormatService.FormatCurrency(report.CreditSalesAmount), null),
                    ("Total Discount", PdfFormatService.FormatCurrency(report.TotalDiscountAmount), null)
                }));

                column.Item().Element(c => ComposeTable(
                    c,
                    "Period Breakdown",
                    new[] { "Period", "Sales Amount", "Invoices", "Discount" },
                    report.Collections.Select(x => new[]
                    {
                        x.Period,
                        PdfFormatService.FormatCurrency(x.SalesAmount),
                        x.InvoiceCount.ToString(),
                        PdfFormatService.FormatCurrency(x.DiscountAmount)
                    }).ToList(),
                    new[] { 2f, 2f, 1f, 2f },
                    new[] { false, true, true, true }));
            });
        }));
    }

    public Task<byte[]> GeneratePurchaseExpenseReportPdfAsync(PurchaseExpenseReportResponseDto report)
    {
        ArgumentNullException.ThrowIfNull(report);
        return Task.FromResult(GenerateDocument(report.Meta, container =>
        {
            container.Column(column =>
            {
                column.Spacing(12);
                column.Item().ShowEntire().Element(c => ComposeKpiCardGrid(c, new[]
                {
                    ("Total Purchase", PdfFormatService.FormatCurrency(report.TotalPurchaseAmount), (string?)null),
                    ("Total Invoices", report.TotalPurchaseInvoices.ToString(), null)
                }));

                column.Item().Element(c => ComposeTable(
                    c, "Vendor Summary", new[] { "Vendor", "Purchase Amount", "Invoices" },
                    report.VendorSummaries.Select(x => new[] { $"{x.VendorName} (#{x.VendorId})", PdfFormatService.FormatCurrency(x.PurchaseAmount), x.InvoiceCount.ToString() }).ToList(),
                    new[] { 3f, 2f, 1f }, new[] { false, true, true }));

                column.Item().Element(c => ComposeTable(
                    c, "Part Summary", new[] { "Part", "Qty Purchased", "Purchase Amount" },
                    report.PartSummaries.Select(x => new[] { $"{x.PartName} (#{x.PartId})", x.QuantityPurchased.ToString(), PdfFormatService.FormatCurrency(x.PurchaseAmount) }).ToList(),
                    new[] { 3f, 1.5f, 2f }, new[] { false, true, true }));

                column.Item().Element(c => ComposeTable(
                    c, "Period Breakdown", new[] { "Period", "Purchase Amount", "Invoices" },
                    report.Collections.Select(x => new[] { x.Period, PdfFormatService.FormatCurrency(x.PurchaseAmount), x.InvoiceCount.ToString() }).ToList(),
                    new[] { 2f, 2f, 1f }, new[] { false, true, true }));
            });
        }));
    }

    public Task<byte[]> GenerateProfitLossReportPdfAsync(ProfitLossReportResponseDto report)
    {
        ArgumentNullException.ThrowIfNull(report);
        return Task.FromResult(GenerateDocument(report.Meta, container =>
        {
            container.Column(column =>
            {
                column.Spacing(12);
                column.Item().ShowEntire().Element(c => ComposeKpiCardGrid(c, new[]
                {
                    ("Sales Revenue", PdfFormatService.FormatCurrency(report.TotalSalesRevenue), (string?)null),
                    ("Purchase Cost", PdfFormatService.FormatCurrency(report.TotalPurchaseCost), null),
                    ("Service Income", PdfFormatService.FormatCurrency(report.ServiceLaborIncome), null),
                    ("Total Discount", PdfFormatService.FormatCurrency(report.TotalDiscountAmount), null),
                    ("Gross Profit", PdfFormatService.FormatCurrency(report.EstimatedGrossProfit), null),
                    ("Net Profit", PdfFormatService.FormatCurrency(report.EstimatedNetProfit), null)
                }));

                column.Item().Element(c => ComposeTable(
                    c, "Period Breakdown", new[] { "Period", "Sales", "Purchase", "Service", "Discount", "Gross", "Net" },
                    report.Collections.Select(x => new[]
                    {
                        x.Period,
                        PdfFormatService.FormatCurrency(x.SalesRevenue),
                        PdfFormatService.FormatCurrency(x.PurchaseCost),
                        PdfFormatService.FormatCurrency(x.ServiceLaborIncome),
                        PdfFormatService.FormatCurrency(x.DiscountAmount),
                        PdfFormatService.FormatCurrency(x.EstimatedGrossProfit),
                        PdfFormatService.FormatCurrency(x.EstimatedNetProfit)
                    }).ToList(),
                    new[] { 1.7f, 1.3f, 1.3f, 1.2f, 1.2f, 1.2f, 1.2f },
                    new[] { false, true, true, true, true, true, true }));
            });
        }));
    }

    public Task<byte[]> GeneratePaymentCollectionReportPdfAsync(PaymentCollectionReportResponseDto report)
    {
        ArgumentNullException.ThrowIfNull(report);
        return Task.FromResult(GenerateDocument(report.Meta, container =>
        {
            container.Column(column =>
            {
                column.Spacing(12);
                column.Item().ShowEntire().Element(c => ComposeKpiCardGrid(c, new[]
                {
                    ("Total Collected", PdfFormatService.FormatCurrency(report.TotalCollectedAmount), (string?)null),
                    ("Cash", PdfFormatService.FormatCurrency(report.CashPaymentsAmount), null),
                    ("Online", PdfFormatService.FormatCurrency(report.OnlinePaymentsAmount), null),
                    ("Bank", PdfFormatService.FormatCurrency(report.BankPaymentsAmount), null),
                    ("Partial", PdfFormatService.FormatCurrency(report.PartialPaymentsAmount), null),
                    ("Unpaid", PdfFormatService.FormatCurrency(report.UnpaidBalanceAmount), null)
                }));

                column.Item().Element(c => ComposeTable(
                    c, "Payment Method Breakdown",
                    new[] { "Method", "Amount", "Count" },
                    report.MethodBreakdown.Select(x => new[] { x.PaymentMethod, PdfFormatService.FormatCurrency(x.Amount), x.Count.ToString() }).ToList(),
                    new[] { 2.5f, 2f, 1f }, new[] { false, true, true }));

                column.Item().Element(c => ComposeTable(
                    c, "Period Breakdown",
                    new[] { "Period", "Collected Amount", "Payments" },
                    report.Collections.Select(x => new[] { x.Period, PdfFormatService.FormatCurrency(x.CollectedAmount), x.PaymentCount.ToString() }).ToList(),
                    new[] { 2f, 2f, 1f }, new[] { false, true, true }));
            });
        }));
    }

    public Task<byte[]> GenerateReceivableReportPdfAsync(ReceivableReportResponseDto report)
    {
        ArgumentNullException.ThrowIfNull(report);
        return Task.FromResult(GenerateDocument(report.Meta, container =>
        {
            container.Column(column =>
            {
                column.Spacing(12);
                column.Item().ShowEntire().Element(c => ComposeKpiCardGrid(c, new[]
                {
                    ("Total Credit Outstanding", PdfFormatService.FormatCurrency(report.TotalCreditOutstanding), (string?)null),
                    ("Overdue Credit", PdfFormatService.FormatCurrency(report.OverdueCreditAmount), null)
                }));

                column.Item().Element(c => ComposeTable(
                    c, "Customer Balances",
                    new[] { "Customer", "Pending", "Overdue", "Invoices" },
                    report.CustomerBalances.Select(x => new[] { $"{x.CustomerName} ({x.CustomerId})", PdfFormatService.FormatCurrency(x.PendingBalance), PdfFormatService.FormatCurrency(x.OverdueBalance), x.InvoiceCount.ToString() }).ToList(),
                    new[] { 2.8f, 1.7f, 1.7f, 1f }, new[] { false, true, true, true }));

                column.Item().Element(c => ComposeTable(
                    c, "Overdue Invoices",
                    new[] { "Invoice", "Customer", "Due Date", "Balance", "Days" },
                    report.OverdueInvoices.Select(x => new[] { x.InvoiceNo, x.CustomerName, PdfFormatService.FormatDateTimeUtc(x.DueDate), PdfFormatService.FormatCurrency(x.BalanceDue), x.DaysOverdue.ToString() }).ToList(),
                    new[] { 1.4f, 2.2f, 1.8f, 1.4f, 1.2f }, new[] { false, false, true, true, true }));
            });
        }));
    }

    public Task<byte[]> GenerateDailyCashFlowReportPdfAsync(DailyCashFlowReportResponseDto report)
    {
        ArgumentNullException.ThrowIfNull(report);
        return Task.FromResult(GenerateDocument(report.Meta, container =>
        {
            container.Column(column =>
            {
                column.Spacing(12);
                column.Item().ShowEntire().Element(c => ComposeKpiCardGrid(c, new[]
                {
                    ("Opening", PdfFormatService.FormatCurrency(report.OpeningBalance), (string?)null),
                    ("Received", PdfFormatService.FormatCurrency(report.MoneyReceived), null),
                    ("Spent", PdfFormatService.FormatCurrency(report.MoneySpentOnPurchases), null),
                    ("Closing", PdfFormatService.FormatCurrency(report.ClosingBalance), null)
                }));

                column.Item().Element(c => ComposeTable(
                    c, "Payment Method Breakdown",
                    new[] { "Method", "Amount", "Count" },
                    report.PaymentMethodBreakdown.Select(x => new[] { x.PaymentMethod, PdfFormatService.FormatCurrency(x.Amount), x.Count.ToString() }).ToList(),
                    new[] { 2.5f, 2f, 1f }, new[] { false, true, true }));

                column.Item().Element(c => ComposeTable(
                    c, "Daily Flows",
                    new[] { "Date", "Received", "Spent", "Net" },
                    report.DailyFlows.Select(x => new[] { PdfFormatService.FormatDateUtc(x.DateUtc), PdfFormatService.FormatCurrency(x.Received), PdfFormatService.FormatCurrency(x.Spent), PdfFormatService.FormatCurrency(x.NetFlow) }).ToList(),
                    new[] { 2f, 1.6f, 1.6f, 1.6f }, new[] { false, true, true, true }));
            });
        }));
    }

    public Task<byte[]> GenerateDiscountReportPdfAsync(DiscountReportResponseDto report)
    {
        ArgumentNullException.ThrowIfNull(report);
        return Task.FromResult(GenerateDocument(report.Meta, container =>
        {
            container.Column(column =>
            {
                column.Spacing(12);
                column.Item().ShowEntire().Element(c => ComposeKpiCardGrid(c, new[]
                {
                    ("Total Discount", PdfFormatService.FormatCurrency(report.TotalDiscountGiven), (string?)null),
                    ("Discounted Invoices", report.DiscountedInvoiceCount.ToString(), null),
                    ("Loyalty 10%", report.Loyalty10PercentInvoiceCount.ToString(), null),
                    ("Impact on Revenue", PdfFormatService.FormatPercent(report.ImpactOnRevenuePercent), null)
                }));

                column.Item().Element(c => ComposeTable(
                    c, "Customer Discounts",
                    new[] { "Customer", "Discount", "Invoices" },
                    report.CustomerDiscounts.Select(x => new[] { $"{x.CustomerName} ({x.CustomerId})", PdfFormatService.FormatCurrency(x.DiscountAmount), x.InvoiceCount.ToString() }).ToList(),
                    new[] { 3f, 2f, 1f }, new[] { false, true, true }));

                column.Item().Element(c => ComposeTable(
                    c, "Period Breakdown",
                    new[] { "Period", "Discount", "Discounted Invoices" },
                    report.Collections.Select(x => new[] { x.Period, PdfFormatService.FormatCurrency(x.DiscountAmount), x.DiscountedInvoiceCount.ToString() }).ToList(),
                    new[] { 2f, 2f, 1.4f }, new[] { false, true, true }));
            });
        }));
    }

    public Task<byte[]> GenerateDashboardSummaryPdfAsync(DashboardSummaryResponseDto report)
    {
        ArgumentNullException.ThrowIfNull(report);
        return Task.FromResult(GenerateDocument(report.Meta, container =>
        {
            container.Column(column =>
            {
                column.Spacing(12);
                column.Item().ShowEntire().Element(c => ComposeKpiCardGrid(c, new[]
                {
                    ("Today Sales", PdfFormatService.FormatCurrency(report.TodaySales), (string?)null),
                    ("Month Sales", PdfFormatService.FormatCurrency(report.MonthSales), null),
                    ("Month Purchases", PdfFormatService.FormatCurrency(report.MonthPurchases), null),
                    ("Payments Received", PdfFormatService.FormatCurrency(report.MonthPaymentsReceived), null),
                    ("Pending Credit", PdfFormatService.FormatCurrency(report.PendingCreditAmount), null),
                    ("Estimated Profit", PdfFormatService.FormatCurrency(report.EstimatedMonthProfit), null)
                }));

                column.Item().ShowEntire().Element(c => ComposeKpiCardGrid(c, new[]
                {
                    ("Low Stock At-Risk Parts", report.LowStockFinancialRisk.AtRiskPartCount.ToString(), (string?)null),
                    ("Low Stock Risk Value", PdfFormatService.FormatCurrency(report.LowStockFinancialRisk.EstimatedRiskValue), null)
                }));

                column.Item().Element(c => ComposeTable(
                    c, "Top Pending Receivables",
                    new[] { "Customer", "Balance Due" },
                    report.TopPendingReceivables.Select(x => new[] { $"{x.CustomerName} ({x.CustomerId})", PdfFormatService.FormatCurrency(x.BalanceDue) }).ToList(),
                    new[] { 3f, 2f }, new[] { false, true }));
            });
        }));
    }

    private static byte[] GenerateDocument(ReportMetaDto meta, Action<IContainer> composeContent)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(24);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header().Element(c => ComposeHeader(c, meta));
                page.Content().Element(c => composeContent(c));
                page.Footer().Element(footer =>
                {
                    footer.AlignCenter().Text(text =>
                    {
                        text.DefaultTextStyle(x => x.FontSize(9).FontColor(SecondaryTextColor));
                        text.Span($"Generated {PdfFormatService.FormatDateTimeUtc(meta.GeneratedAtUtc)} | Page ");
                        text.CurrentPageNumber();
                        text.Span(" of ");
                        text.TotalPages();
                    });
                });
            });
        });

        return document.GeneratePdf();
    }

    private static void ComposeHeader(IContainer container, ReportMetaDto meta)
    {
        container.Column(column =>
        {
            column.Spacing(8);
            column.Item().Text(meta.ReportTitle).SemiBold().FontSize(22).FontColor(PrimaryColor);
            column.Item().Text($"Generated: {PdfFormatService.FormatDateTimeUtc(meta.GeneratedAtUtc)}").FontSize(11).FontColor(SecondaryTextColor);
            column.Item().Text($"Range: {PdfFormatService.FormatDateTimeUtc(meta.DateRange.FromUtc)} - {PdfFormatService.FormatDateTimeUtc(meta.DateRange.ToUtc)}").FontSize(11).FontColor(SecondaryTextColor);

            column.Item().Row(row =>
            {
                row.Spacing(8);
                ComposeChip(row.RelativeItem(), $"Group By: {meta.GroupBy}");
                ComposeChip(row.RelativeItem(), $"Cancelled: {meta.IncludeCancelled}");
                ComposeChip(row.RelativeItem(), $"Customer: {PdfFormatService.ValueOrDash(meta.CustomerId)}");
                ComposeChip(row.RelativeItem(), $"Vendor: {(meta.VendorId?.ToString() ?? "-")}");
            });

            ComposeInfoNote(column.Item(), meta.CalculationNote);
            column.Item().PaddingTop(8).LineHorizontal(1).LineColor(BorderColor);
        });
    }

    private static void ComposeChip(IContainer container, string text)
    {
        container.PaddingVertical(4)
            .PaddingHorizontal(8)
            .Border(1)
            .BorderColor(BorderColor)
            .Background(MutedBackgroundColor)
            .Text(text)
            .FontSize(9)
            .FontColor(SecondaryTextColor);
    }

    private static void ComposeInfoNote(IContainer container, string note)
    {
        if (string.IsNullOrWhiteSpace(note))
        {
            return;
        }

        container.Padding(8)
            .Background(MutedBackgroundColor)
            .Border(1)
            .BorderColor(BorderColor)
            .Text($"Note: {note}")
            .Italic()
            .FontSize(11)
            .FontColor(SecondaryTextColor);
    }

    private static void ComposeKpiCardGrid(IContainer container, IReadOnlyList<(string Label, string Value, string? Helper)> items)
    {
        container.Row(row =>
        {
            for (var i = 0; i < items.Count; i++)
            {
                row.RelativeItem().Element(c => ComposeKpiCard(c, items[i]));
                if (i < items.Count - 1)
                {
                    row.ConstantItem(8);
                }
            }
        });
    }

    private static void ComposeKpiCard(IContainer container, (string Label, string Value, string? Helper) item)
    {
        container.Padding(12)
            .Background(MutedBackgroundColor)
            .Border(1)
            .BorderColor(BorderColor)
            .Column(column =>
            {
                column.Spacing(4);
                column.Item().Text(item.Label).FontSize(9).FontColor(SecondaryTextColor);
                column.Item().Text(item.Value).SemiBold().FontSize(16);
                if (!string.IsNullOrWhiteSpace(item.Helper))
                {
                    column.Item().Text(item.Helper).FontSize(9).FontColor(SecondaryTextColor);
                }
            });
    }

    private static void ComposeTable(
        IContainer container,
        string title,
        IReadOnlyList<string> headers,
        IReadOnlyList<string[]> rows,
        IReadOnlyList<float> columnWidths,
        IReadOnlyList<bool> alignRight)
    {
        container.Column(column =>
        {
            column.Spacing(8);
            column.Item().PaddingBottom(8).Text(title).SemiBold().FontSize(13).FontColor(PrimaryColor);

            if (rows.Count == 0)
            {
                column.Item().Padding(8).Text("No data available").FontColor(SecondaryTextColor);
                return;
            }

            column.Item().Table(table =>
            {
                table.ColumnsDefinition(cols =>
                {
                    foreach (var width in columnWidths)
                    {
                        cols.RelativeColumn(width);
                    }
                });

                table.Header(header =>
                {
                    for (var i = 0; i < headers.Count; i++)
                    {
                        var cell = header.Cell().Element(ComposeTableHeaderCell);
                        if (alignRight[i]) cell.AlignRight().Text(headers[i]).SemiBold();
                        else cell.Text(headers[i]).SemiBold();
                    }
                });

                for (var r = 0; r < rows.Count; r++)
                {
                    var row = rows[r];
                    var isZebra = r % 2 == 1;

                    for (var c = 0; c < row.Length; c++)
                    {
                        var cell = table.Cell().Element(x => ComposeTableBodyCell(x, isZebra));
                        if (alignRight[c]) cell.AlignRight().Text(row[c]);
                        else cell.Text(row[c]);
                    }
                }
            });
        });
    }

    private static IContainer ComposeTableHeaderCell(IContainer container) =>
        container.Padding(8)
            .Background(HeaderBackgroundColor)
            .BorderBottom(1)
            .BorderColor(BorderColor);

    private static IContainer ComposeTableBodyCell(IContainer container, bool zebra) =>
        container.PaddingVertical(8)
            .PaddingHorizontal(8)
            .Background(zebra ? ZebraBackgroundColor : Colors.White)
            .BorderBottom(1)
            .BorderColor(BorderColor);
}
