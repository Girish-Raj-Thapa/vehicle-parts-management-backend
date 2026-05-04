using System.Globalization;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using VehiclePartsManagement.Application.DTOs.Report;
using VehiclePartsManagement.Application.Interfaces.Services;

namespace VehiclePartsManagement.Application.Services;

public class ReportPdfService : IReportPdfService
{
    private static readonly CultureInfo InvariantCulture = CultureInfo.InvariantCulture;

    static ReportPdfService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public Task<byte[]> GenerateSalesRevenueReportPdfAsync(SalesRevenueReportResponseDto report)
    {
        ArgumentNullException.ThrowIfNull(report);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(24);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Element(c => ComposeHeader(c, report));
                page.Content().Element(c => ComposeContent(c, report));
                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("Vehicle Parts Management | Page ");
                    text.CurrentPageNumber();
                    text.Span(" of ");
                    text.TotalPages();
                });
            });
        });

        return Task.FromResult(document.GeneratePdf());
    }

    private static void ComposeHeader(IContainer container, SalesRevenueReportResponseDto report)
    {
        container.Column(column =>
        {
            column.Spacing(4);
            column.Item().Text(report.Meta.ReportTitle).SemiBold().FontSize(16).FontColor(Colors.Blue.Darken2);
            column.Item().Text($"Generated (UTC): {FormatDateTime(report.Meta.GeneratedAtUtc)}");
            column.Item().Text(
                $"Range (UTC): {FormatDateTime(report.Meta.DateRange.FromUtc)} - {FormatDateTime(report.Meta.DateRange.ToUtc)}");
            column.Item().Text(
                $"Group By: {report.Meta.GroupBy} | Include Cancelled: {report.Meta.IncludeCancelled} | Customer Filter: {report.Meta.CustomerId ?? "All"}");
            column.Item().Text($"Note: {report.Meta.CalculationNote}").Italic();
            column.Item().PaddingTop(6).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
        });
    }

    private static void ComposeContent(IContainer container, SalesRevenueReportResponseDto report)
    {
        container.Column(column =>
        {
            column.Spacing(12);
            column.Item().Element(c => ComposeKpiGrid(c, report));
            column.Item().Element(c => ComposeCollectionsTable(c, report.Collections));
        });
    }

    private static void ComposeKpiGrid(IContainer container, SalesRevenueReportResponseDto report)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn();
                columns.RelativeColumn();
            });

            table.Cell().Element(CellStyle).Text($"Total Sales: {FormatMoney(report.TotalSalesAmount)}");
            table.Cell().Element(CellStyle).Text($"Total Invoices: {report.TotalInvoices}");
            table.Cell().Element(CellStyle).Text($"Cash Sales: {FormatMoney(report.CashSalesAmount)}");
            table.Cell().Element(CellStyle).Text($"Credit Sales: {FormatMoney(report.CreditSalesAmount)}");
            table.Cell().Element(CellStyle).Text($"Total Discount: {FormatMoney(report.TotalDiscountAmount)}");
            table.Cell().Element(CellStyle).Text(string.Empty);
        });

        static IContainer CellStyle(IContainer container) =>
            container.Padding(8).Border(1).BorderColor(Colors.Grey.Lighten2).Background(Colors.Grey.Lighten5);
    }

    private static void ComposeCollectionsTable(IContainer container, IReadOnlyList<SalesRevenuePeriodItemDto> collections)
    {
        container.Column(column =>
        {
            column.Spacing(6);
            column.Item().Text("Period Breakdown").SemiBold().FontSize(12);
            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(1);
                    columns.RelativeColumn(2);
                });

                table.Header(header =>
                {
                    header.Cell().Element(HeaderCellStyle).Text("Period").SemiBold();
                    header.Cell().Element(HeaderCellStyle).AlignRight().Text("Sales Amount").SemiBold();
                    header.Cell().Element(HeaderCellStyle).AlignRight().Text("Invoices").SemiBold();
                    header.Cell().Element(HeaderCellStyle).AlignRight().Text("Discount").SemiBold();
                });

                foreach (var item in collections)
                {
                    table.Cell().Element(BodyCellStyle).Text(item.Period);
                    table.Cell().Element(BodyCellStyle).AlignRight().Text(FormatMoney(item.SalesAmount));
                    table.Cell().Element(BodyCellStyle).AlignRight().Text(item.InvoiceCount.ToString(InvariantCulture));
                    table.Cell().Element(BodyCellStyle).AlignRight().Text(FormatMoney(item.DiscountAmount));
                }
            });
        });

        static IContainer HeaderCellStyle(IContainer container) =>
            container.Padding(6).Background(Colors.Grey.Lighten3).BorderBottom(1).BorderColor(Colors.Grey.Lighten1);

        static IContainer BodyCellStyle(IContainer container) =>
            container.PaddingVertical(5).PaddingHorizontal(6).BorderBottom(1).BorderColor(Colors.Grey.Lighten3);
    }

    private static string FormatMoney(decimal value) => value.ToString("0.00", InvariantCulture);

    private static string FormatDateTime(DateTime value) =>
        value.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss", InvariantCulture);
}
