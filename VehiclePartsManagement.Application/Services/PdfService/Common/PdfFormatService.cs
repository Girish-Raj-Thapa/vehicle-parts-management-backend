using System.Globalization;

namespace VehiclePartsManagement.Application.Services.PdfService.Common;

public static class PdfFormatService
{
    private static readonly CultureInfo EnIn = CultureInfo.GetCultureInfo("en-IN");

    public static string FormatCurrency(decimal value) => $"NPR {value.ToString("N2", EnIn)}";

    public static string FormatPercent(decimal value) => $"{value.ToString("0.00", CultureInfo.InvariantCulture)}%";

    public static string FormatDateTimeUtc(DateTime value) =>
        value.ToUniversalTime().ToString("dd MMM yyyy, hh:mm tt 'UTC'", CultureInfo.InvariantCulture);

    public static string FormatDateUtc(DateTime value) =>
        value.ToUniversalTime().ToString("dd MMM yyyy", CultureInfo.InvariantCulture);

    public static string ValueOrDash(string? value) => string.IsNullOrWhiteSpace(value) ? "-" : value;
}
