using System.Text;
using VehiclePartsManagement.Application.Common;
using VehiclePartsManagement.Application.DTOs.Sales;
using VehiclePartsManagement.Application.Interfaces.Repositories;
using VehiclePartsManagement.Application.Interfaces.Services;

namespace VehiclePartsManagement.Application.Services;

public class SaleService(ISaleRepository saleRepository, IEmailService emailService) : ISaleService
{
    public async Task<ApiResponse<IEnumerable<SaleSummaryDto>>> GetAllSalesAsync()
    {
        var sales = await saleRepository.GetAllSalesAsync();
        var data = sales.Select(s => new SaleSummaryDto
        {
            SaleId = s.SaleId,
            InvoiceNo = s.SaleInvoiceNo,
            CustomerName = s.CustomerUser?.FullName ?? string.Empty,
            CustomerEmail = s.CustomerUser?.Email ?? string.Empty,
            TotalAmount = s.TotalAmount,
            BalanceDue = s.BalanceDue,
            SaleDate = s.SaleDate,
            PaymentStatus = s.PaymentStatus.ToString()
        });
        return ApiResponse<IEnumerable<SaleSummaryDto>>.SuccessResponse(data, "Sales retrieved successfully");
    }

    public async Task<ApiResponse> SendInvoiceEmailAsync(int saleId)
    {
        var sale = await saleRepository.GetSaleWithDetailsAsync(saleId);
        if (sale == null)
            return ApiResponse.FailureResponse("Sale not found");

        if (string.IsNullOrEmpty(sale.CustomerUser?.Email))
            return ApiResponse.FailureResponse("Customer email not found");

        var subject = $"Invoice for Sale #{sale.SaleInvoiceNo}";
        var body = GenerateInvoiceEmailBody(sale);

        var sent = await emailService.SendEmailAsync(sale.CustomerUser.Email, subject, body);

        return sent 
            ? ApiResponse.SuccessResponse("Invoice email sent successfully")
            : ApiResponse.FailureResponse("Failed to send invoice email");
    }

    private string GenerateInvoiceEmailBody(Domain.Entities.Sale sale)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<div style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>");
        sb.AppendLine($"<h3>Hello {sale.CustomerUser.FullName},</h3>");
        sb.AppendLine("<p>Thank you for your purchase! Here are your invoice details:</p>");
        
        sb.AppendLine("<div style='background-color: #f9f9f9; padding: 15px; border-radius: 5px; border: 1px solid #ddd;'>");
        sb.AppendLine($"<p><strong>Invoice No:</strong> {sale.SaleInvoiceNo}<br/>");
        sb.AppendLine($"<strong>Date:</strong> {sale.SaleDate:yyyy-MM-dd HH:mm}</p>");
        
        sb.AppendLine("<table style='width: 100%; border-collapse: collapse; margin-top: 10px;'>");
        sb.AppendLine("<tr style='background-color: #eee;'>");
        sb.AppendLine("<th style='text-align: left; padding: 8px; border: 1px solid #ddd;'>Part</th>");
        sb.AppendLine("<th style='text-align: center; padding: 8px; border: 1px solid #ddd;'>Qty</th>");
        sb.AppendLine("<th style='text-align: right; padding: 8px; border: 1px solid #ddd;'>Price</th>");
        sb.AppendLine("<th style='text-align: right; padding: 8px; border: 1px solid #ddd;'>Total</th>");
        sb.AppendLine("</tr>");

        foreach (var item in sale.SaleItems)
        {
            sb.AppendLine("<tr>");
            sb.AppendLine($"<td style='padding: 8px; border: 1px solid #ddd;'>{item.Part.PartName}</td>");
            sb.AppendLine($"<td style='text-align: center; padding: 8px; border: 1px solid #ddd;'>{item.QuantitySold}</td>");
            sb.AppendLine($"<td style='text-align: right; padding: 8px; border: 1px solid #ddd;'>{item.UnitPriceAtSale:C}</td>");
            sb.AppendLine($"<td style='text-align: right; padding: 8px; border: 1px solid #ddd;'>{item.LineTotal:C}</td>");
            sb.AppendLine("</tr>");
        }
        sb.AppendLine("</table>");

        sb.AppendLine("<div style='text-align: right; margin-top: 15px;'>");
        sb.AppendLine($"<p><strong>SubTotal:</strong> {sale.SubTotal:C}<br/>");
        if (sale.DiscountAmount > 0)
            sb.AppendLine($"<strong>Discount:</strong> {sale.DiscountAmount:C}<br/>");
        sb.AppendLine($"<span style='font-size: 1.2em; color: #2c3e50;'><strong>Total Amount:</strong> {sale.TotalAmount:C}</span><br/>");
        sb.AppendLine($"<span style='color: #e74c3c;'><strong>Balance Due:</strong> {sale.BalanceDue:C}</span></p>");
        sb.AppendLine("</div>");

        if (sale.DueDate.HasValue)
        {
            sb.AppendLine($"<p style='color: #7f8c8d;'><em>Payment Due Date: {sale.DueDate.Value.ToShortDateString()}</em></p>");
        }
        
        sb.AppendLine("</div>");
        sb.AppendLine("<p>Regards,<br/><strong>Vehicle Parts Management Team</strong></p>");
        sb.AppendLine("</div>");

        return sb.ToString();
    }
}
