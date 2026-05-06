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

                page.Header().Element(c => ComposeMetaHeader(c, report.Meta));
                page.Content().Element(c =>
                {
                    c.Column(column =>
                    {
                        column.Spacing(12);
                        column.Item().Element(k => ComposeKeyValueGrid(k, new[]
                        {
                            ("Total Sales", FormatMoney(report.TotalSalesAmount)),
                            ("Total Invoices", report.TotalInvoices.ToString(InvariantCulture)),
                            ("Cash Sales", FormatMoney(report.CashSalesAmount)),
                            ("Credit Sales", FormatMoney(report.CreditSalesAmount)),
                            ("Total Discount", FormatMoney(report.TotalDiscountAmount))
                        }));

                        column.Item().Element(t => ComposeTable(
                            t,
                            "Period Breakdown",
                            new[] { "Period", "Sales Amount", "Invoices", "Discount" },
                            report.Collections.Select(x => new[]
                            {
                                x.Period,
                                FormatMoney(x.SalesAmount),
                                x.InvoiceCount.ToString(InvariantCulture),
                                FormatMoney(x.DiscountAmount)
                            }).ToList(),
                            new[] { 2f, 2f, 1f, 2f },
                            new[] { false, true, true, true }));
                    });
                });
                page.Footer().Element(ComposeFooter);
            });
        });

        return Task.FromResult(document.GeneratePdf());
    }

    public Task<byte[]> GeneratePurchaseExpenseReportPdfAsync(PurchaseExpenseReportResponseDto report)
    {
        ArgumentNullException.ThrowIfNull(report);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(24);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Element(c => ComposeMetaHeader(c, report.Meta));
                page.Content().Element(c =>
                {
                    c.Column(column =>
                    {
                        column.Spacing(12);
                        column.Item().Element(k => ComposeKeyValueGrid(k, new[]
                        {
                            ("Total Purchase", FormatMoney(report.TotalPurchaseAmount)),
                            ("Total Invoices", report.TotalPurchaseInvoices.ToString(InvariantCulture))
                        }));

                        column.Item().Element(t => ComposeTable(
                            t,
                            "Vendor Summary",
                            new[] { "Vendor", "Purchase Amount", "Invoices" },
                            report.VendorSummaries.Select(x => new[]
                            {
                                $"{x.VendorName} (#{x.VendorId})",
                                FormatMoney(x.PurchaseAmount),
                                x.InvoiceCount.ToString(InvariantCulture)
                            }).ToList(),
                            new[] { 3f, 2f, 1f },
                            new[] { false, true, true }));

                        column.Item().Element(t => ComposeTable(
                            t,
                            "Part Summary",
                            new[] { "Part", "Qty Purchased", "Purchase Amount" },
                            report.PartSummaries.Select(x => new[]
                            {
                                $"{x.PartName} (#{x.PartId})",
                                x.QuantityPurchased.ToString(InvariantCulture),
                                FormatMoney(x.PurchaseAmount)
                            }).ToList(),
                            new[] { 3f, 1.5f, 2f },
                            new[] { false, true, true }));

                        column.Item().Element(t => ComposeTable(
                            t,
                            "Period Breakdown",
                            new[] { "Period", "Purchase Amount", "Invoices" },
                            report.Collections.Select(x => new[]
                            {
                                x.Period,
                                FormatMoney(x.PurchaseAmount),
                                x.InvoiceCount.ToString(InvariantCulture)
                            }).ToList(),
                            new[] { 2f, 2f, 1f },
                            new[] { false, true, true }));
                    });
                });
                page.Footer().Element(ComposeFooter);
            });
        });

        return Task.FromResult(document.GeneratePdf());
    }

    public Task<byte[]> GenerateProfitLossReportPdfAsync(ProfitLossReportResponseDto report)
    {
        ArgumentNullException.ThrowIfNull(report);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(24);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Element(c => ComposeMetaHeader(c, report.Meta));
                page.Content().Element(c =>
                {
                    c.Column(column =>
                    {
                        column.Spacing(12);
                        column.Item().Element(k => ComposeKeyValueGrid(k, new[]
                        {
                            ("Sales Revenue", FormatMoney(report.TotalSalesRevenue)),
                            ("Purchase Cost", FormatMoney(report.TotalPurchaseCost)),
                            ("Service Labor Income", FormatMoney(report.ServiceLaborIncome)),
                            ("Total Discount", FormatMoney(report.TotalDiscountAmount)),
                            ("Estimated Gross Profit", FormatMoney(report.EstimatedGrossProfit)),
                            ("Estimated Net Profit", FormatMoney(report.EstimatedNetProfit))
                        }));

                        column.Item().Element(t => ComposeTable(
                            t,
                            "Period Breakdown",
                            new[]
                            {
                                "Period", "Sales", "Purchase", "Service", "Discount", "Gross", "Net"
                            },
                            report.Collections.Select(x => new[]
                            {
                                x.Period,
                                FormatMoney(x.SalesRevenue),
                                FormatMoney(x.PurchaseCost),
                                FormatMoney(x.ServiceLaborIncome),
                                FormatMoney(x.DiscountAmount),
                                FormatMoney(x.EstimatedGrossProfit),
                                FormatMoney(x.EstimatedNetProfit)
                            }).ToList(),
                            new[] { 1.7f, 1.3f, 1.3f, 1.2f, 1.2f, 1.2f, 1.2f },
                            new[] { false, true, true, true, true, true, true }));
                    });
                });
                page.Footer().Element(ComposeFooter);
            });
        });

        return Task.FromResult(document.GeneratePdf());
    }

    public Task<byte[]> GeneratePaymentCollectionReportPdfAsync(PaymentCollectionReportResponseDto report)
    {
        ArgumentNullException.ThrowIfNull(report);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(24);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Element(c => ComposeMetaHeader(c, report.Meta));
                page.Content().Element(c =>
                {
                    c.Column(column =>
                    {
                        column.Spacing(12);
                        column.Item().Element(k => ComposeKeyValueGrid(k, new[]
                        {
                            ("Total Collected", FormatMoney(report.TotalCollectedAmount)),
                            ("Cash Payments", FormatMoney(report.CashPaymentsAmount)),
                            ("Online Payments", FormatMoney(report.OnlinePaymentsAmount)),
                            ("Bank Payments", FormatMoney(report.BankPaymentsAmount)),
                            ("Partial Payments", FormatMoney(report.PartialPaymentsAmount)),
                            ("Unpaid Balance", FormatMoney(report.UnpaidBalanceAmount))
                        }));

                        column.Item().Element(t => ComposeTable(
                            t,
                            "Payment Method Breakdown",
                            new[] { "Method", "Amount", "Count" },
                            report.MethodBreakdown.Select(x => new[]
                            {
                                x.PaymentMethod,
                                FormatMoney(x.Amount),
                                x.Count.ToString(InvariantCulture)
                            }).ToList(),
                            new[] { 2.5f, 2f, 1f },
                            new[] { false, true, true }));

                        column.Item().Element(t => ComposeTable(
                            t,
                            "Period Breakdown",
                            new[] { "Period", "Collected Amount", "Payments" },
                            report.Collections.Select(x => new[]
                            {
                                x.Period,
                                FormatMoney(x.CollectedAmount),
                                x.PaymentCount.ToString(InvariantCulture)
                            }).ToList(),
                            new[] { 2f, 2f, 1f },
                            new[] { false, true, true }));
                    });
                });
                page.Footer().Element(ComposeFooter);
            });
        });

        return Task.FromResult(document.GeneratePdf());
    }

    public Task<byte[]> GenerateReceivableReportPdfAsync(ReceivableReportResponseDto report)
    {
        ArgumentNullException.ThrowIfNull(report);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(24);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Element(c => ComposeMetaHeader(c, report.Meta));
                page.Content().Element(c =>
                {
                    c.Column(column =>
                    {
                        column.Spacing(12);
                        column.Item().Element(k => ComposeKeyValueGrid(k, new[]
                        {
                            ("Total Credit Outstanding", FormatMoney(report.TotalCreditOutstanding)),
                            ("Overdue Credit Amount", FormatMoney(report.OverdueCreditAmount))
                        }));

                        column.Item().Element(t => ComposeTable(
                            t,
                            "Customer Balances",
                            new[] { "Customer", "Pending Balance", "Overdue Balance", "Invoices" },
                            report.CustomerBalances.Select(x => new[]
                            {
                                $"{x.CustomerName} ({x.CustomerId})",
                                FormatMoney(x.PendingBalance),
                                FormatMoney(x.OverdueBalance),
                                x.InvoiceCount.ToString(InvariantCulture)
                            }).ToList(),
                            new[] { 2.8f, 1.7f, 1.7f, 1f },
                            new[] { false, true, true, true }));

                        column.Item().Element(t => ComposeTable(
                            t,
                            "Overdue Invoices",
                            new[] { "Invoice No", "Customer", "Due Date", "Balance Due", "Days Overdue" },
                            report.OverdueInvoices.Select(x => new[]
                            {
                                x.InvoiceNo,
                                x.CustomerName,
                                FormatDateTime(x.DueDate),
                                FormatMoney(x.BalanceDue),
                                x.DaysOverdue.ToString(InvariantCulture)
                            }).ToList(),
                            new[] { 1.4f, 2.2f, 1.8f, 1.4f, 1.2f },
                            new[] { false, false, true, true, true }));
                    });
                });
                page.Footer().Element(ComposeFooter);
            });
        });

        return Task.FromResult(document.GeneratePdf());
    }

    private static void ComposeMetaHeader(IContainer container, ReportMetaDto meta)
    {
        container.Column(column =>
        {
            column.Spacing(4);
            column.Item().Text(meta.ReportTitle).SemiBold().FontSize(16).FontColor(Colors.Blue.Darken2);
            column.Item().Text($"Generated (UTC): {FormatDateTime(meta.GeneratedAtUtc)}");
            column.Item().Text(
                $"Range (UTC): {FormatDateTime(meta.DateRange.FromUtc)} - {FormatDateTime(meta.DateRange.ToUtc)}");
            column.Item().Text(
                $"Group By: {meta.GroupBy} | Include Cancelled: {meta.IncludeCancelled} | Customer: {meta.CustomerId ?? "All"} | Vendor: {(meta.VendorId?.ToString(InvariantCulture) ?? "All")}");
            column.Item().Text($"Note: {meta.CalculationNote}").Italic();
            column.Item().PaddingTop(6).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
        });
    }

    private static void ComposeKeyValueGrid(IContainer container, IReadOnlyList<(string Label, string Value)> items)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn();
                columns.RelativeColumn();
            });

            foreach (var item in items)
            {
                table.Cell().Element(CellStyle).Text($"{item.Label}: {item.Value}");
            }

            if (items.Count % 2 != 0)
            {
                table.Cell().Element(CellStyle).Text(string.Empty);
            }
        });

        static IContainer CellStyle(IContainer cell) =>
            cell.Padding(8).Border(1).BorderColor(Colors.Grey.Lighten2).Background(Colors.Grey.Lighten5);
    }

    private static void ComposeTable(
        IContainer container,
        string title,
        IReadOnlyList<string> headers,
        IReadOnlyList<string[]> rows,
        IReadOnlyList<float> columnWeights,
        IReadOnlyList<bool> alignRight)
    {
        container.Column(column =>
        {
            column.Spacing(6);
            column.Item().Text(title).SemiBold().FontSize(12);

            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    foreach (var weight in columnWeights)
                    {
                        columns.RelativeColumn(weight);
                    }
                });

                table.Header(header =>
                {
                    for (var i = 0; i < headers.Count; i++)
                    {
                        var cell = header.Cell().Element(HeaderCellStyle);
                        if (alignRight[i])
                        {
                            cell.AlignRight().Text(headers[i]).SemiBold();
                        }
                        else
                        {
                            cell.Text(headers[i]).SemiBold();
                        }
                    }
                });

                foreach (var row in rows)
                {
                    for (var i = 0; i < row.Length; i++)
                    {
                        var cell = table.Cell().Element(BodyCellStyle);
                        if (alignRight[i])
                        {
                            cell.AlignRight().Text(row[i]);
                        }
                        else
                        {
                            cell.Text(row[i]);
                        }
                    }
                }
            });
        });

        static IContainer HeaderCellStyle(IContainer c) =>
            c.Padding(6).Background(Colors.Grey.Lighten3).BorderBottom(1).BorderColor(Colors.Grey.Lighten1);

        static IContainer BodyCellStyle(IContainer c) =>
            c.PaddingVertical(5).PaddingHorizontal(6).BorderBottom(1).BorderColor(Colors.Grey.Lighten3);
    }

    private static void ComposeFooter(IContainer container)
    {
        container.AlignCenter().Text(text =>
        {
            text.Span("Vehicle Parts Management | Page ");
            text.CurrentPageNumber();
            text.Span(" of ");
            text.TotalPages();
        });
    }

    private static string FormatMoney(decimal value) => value.ToString("0.00", InvariantCulture);

    private static string FormatDateTime(DateTime value) =>
        value.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss", InvariantCulture);
}
