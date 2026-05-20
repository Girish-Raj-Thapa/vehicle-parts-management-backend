using System.Text;
using VehiclePartsManagement.Domain.Entities;

namespace VehiclePartsManagement.Application.Services.EmailTemplates;

public static class InvoiceEmailTemplateBuilder
{
    public static string Build(Sale sale)
    {
        var sb = new StringBuilder();
        var paidAmount = sale.Payments.Sum(p => p.PaymentAmount);
        var vehicleLabel = sale.Vehicle is null
            ? "N/A"
            : $"{sale.Vehicle.Model.Brand.BrandName} {sale.Vehicle.Model.ModelName} - {sale.Vehicle.VehicleNumber}";

        sb.AppendLine("<div style='margin:0;padding:0;background:#f5f5f7;'>");
        sb.AppendLine("<div style='max-width:720px;margin:24px auto;background:#ffffff;border:1px solid #d2d2d7;border-radius:12px;overflow:hidden;font-family:Inter,-apple-system,BlinkMacSystemFont,Segoe UI,Roboto,Arial,sans-serif;color:#1d1d1f;'>");
        sb.AppendLine("<div style='padding:20px 24px;border-bottom:1px solid #e5e5ea;background:#fbfbfd;'>");
        sb.AppendLine("<h2 style='margin:0;font-size:22px;line-height:1.25;'>Invoice</h2>");
        sb.AppendLine("<p style='margin:6px 0 0 0;color:#6e6e73;font-size:14px;'>Vehicle Parts Management</p>");
        sb.AppendLine("</div>");
        sb.AppendLine("<div style='padding:20px 24px;'>");
        sb.AppendLine($"<p style='margin:0 0 12px 0;font-size:15px;'>Hello {sale.CustomerUser.FullName},</p>");
        sb.AppendLine("<p style='margin:0 0 16px 0;font-size:14px;color:#3a3a3c;'>Thank you for your purchase. Your invoice details are below.</p>");

        sb.AppendLine("<table style='width:100%;border-collapse:collapse;margin-bottom:18px;'>");
        sb.AppendLine("<tr>");
        sb.AppendLine("<td style='padding:8px 0;color:#6e6e73;font-size:13px;'>Invoice No</td>");
        sb.AppendLine($"<td style='padding:8px 0;text-align:right;font-size:13px;font-weight:600;'>{sale.SaleInvoiceNo}</td>");
        sb.AppendLine("</tr>");
        sb.AppendLine("<tr>");
        sb.AppendLine("<td style='padding:8px 0;color:#6e6e73;font-size:13px;'>Date</td>");
        sb.AppendLine($"<td style='padding:8px 0;text-align:right;font-size:13px;'>{sale.SaleDate:yyyy-MM-dd HH:mm}</td>");
        sb.AppendLine("</tr>");
        sb.AppendLine("<tr>");
        sb.AppendLine("<td style='padding:8px 0;color:#6e6e73;font-size:13px;'>Channel</td>");
        sb.AppendLine($"<td style='padding:8px 0;text-align:right;font-size:13px;'>{sale.SaleChannel}</td>");
        sb.AppendLine("</tr>");
        sb.AppendLine("<tr>");
        sb.AppendLine("<td style='padding:8px 0;color:#6e6e73;font-size:13px;'>Vehicle</td>");
        sb.AppendLine($"<td style='padding:8px 0;text-align:right;font-size:13px;'>{vehicleLabel}</td>");
        sb.AppendLine("</tr>");
        sb.AppendLine("</table>");

        sb.AppendLine("<table style='width:100%;border-collapse:collapse;border:1px solid #e5e5ea;border-radius:10px;overflow:hidden;'>");
        sb.AppendLine("<thead>");
        sb.AppendLine("<tr style='background:#f5f5f7;'>");
        sb.AppendLine("<th style='text-align:left;padding:10px 12px;font-size:12px;font-weight:700;color:#6e6e73;border-bottom:1px solid #e5e5ea;'>Part</th>");
        sb.AppendLine("<th style='text-align:center;padding:10px 12px;font-size:12px;font-weight:700;color:#6e6e73;border-bottom:1px solid #e5e5ea;'>Qty</th>");
        sb.AppendLine("<th style='text-align:right;padding:10px 12px;font-size:12px;font-weight:700;color:#6e6e73;border-bottom:1px solid #e5e5ea;'>Unit Price</th>");
        sb.AppendLine("<th style='text-align:right;padding:10px 12px;font-size:12px;font-weight:700;color:#6e6e73;border-bottom:1px solid #e5e5ea;'>Line Total</th>");
        sb.AppendLine("</tr>");
        sb.AppendLine("</thead>");
        sb.AppendLine("<tbody>");
        foreach (var item in sale.SaleItems)
        {
            sb.AppendLine("<tr>");
            sb.AppendLine($"<td style='padding:10px 12px;font-size:13px;border-bottom:1px solid #f2f2f2;'>{item.Part.PartName}</td>");
            sb.AppendLine($"<td style='padding:10px 12px;font-size:13px;text-align:center;border-bottom:1px solid #f2f2f2;'>{item.QuantitySold}</td>");
            sb.AppendLine($"<td style='padding:10px 12px;font-size:13px;text-align:right;border-bottom:1px solid #f2f2f2;'>Rs. {item.UnitPriceAtSale:N2}</td>");
            sb.AppendLine($"<td style='padding:10px 12px;font-size:13px;text-align:right;border-bottom:1px solid #f2f2f2;'>Rs. {item.LineTotal:N2}</td>");
            sb.AppendLine("</tr>");
        }
        sb.AppendLine("</tbody>");
        sb.AppendLine("</table>");

        sb.AppendLine("<table style='width:100%;border-collapse:collapse;margin-top:16px;'>");
        sb.AppendLine($"<tr><td style='padding:4px 0;color:#6e6e73;font-size:13px;'>SubTotal</td><td style='padding:4px 0;text-align:right;font-size:13px;'>Rs. {sale.SubTotal:N2}</td></tr>");
        if (sale.DiscountAmount > 0)
        {
            sb.AppendLine($"<tr><td style='padding:4px 0;color:#6e6e73;font-size:13px;'>Discount ({sale.LoyaltyDiscountPercent:N0}%)</td><td style='padding:4px 0;text-align:right;font-size:13px;color:#0071e3;'>- Rs. {sale.DiscountAmount:N2}</td></tr>");
        }
        sb.AppendLine($"<tr><td style='padding:4px 0;color:#1d1d1f;font-size:14px;font-weight:700;'>Total Amount</td><td style='padding:4px 0;text-align:right;font-size:14px;font-weight:700;'>Rs. {sale.TotalAmount:N2}</td></tr>");
        sb.AppendLine($"<tr><td style='padding:4px 0;color:#6e6e73;font-size:13px;'>Paid Amount</td><td style='padding:4px 0;text-align:right;font-size:13px;color:#1d7a33;'>Rs. {paidAmount:N2}</td></tr>");
        sb.AppendLine($"<tr><td style='padding:4px 0;color:#6e6e73;font-size:13px;'>Balance Due</td><td style='padding:4px 0;text-align:right;font-size:13px;color:#b42318;'>Rs. {sale.BalanceDue:N2}</td></tr>");
        sb.AppendLine("</table>");

        if (sale.DueDate.HasValue)
        {
            sb.AppendLine($"<p style='margin:12px 0 0 0;font-size:12px;color:#6e6e73;'>Due Date: {sale.DueDate.Value:yyyy-MM-dd}</p>");
        }

        sb.AppendLine("<p style='margin:18px 0 0 0;font-size:13px;color:#3a3a3c;'>Regards,<br/><strong>Vehicle Parts Management Team</strong></p>");
        sb.AppendLine("</div></div></div>");

        return sb.ToString();
    }
}
