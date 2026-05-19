using System.Text;
using VehiclePartsManagement.Application.Common;
using VehiclePartsManagement.Application.DTOs.Notifications;
using VehiclePartsManagement.Application.Interfaces.Repositories;
using VehiclePartsManagement.Application.Interfaces.Services;

namespace VehiclePartsManagement.Application.Services;

public class NotificationService(
    ISaleRepository saleRepository,
    IPartRepository partRepository,
    IEmailService emailService) : INotificationService
{
    public async Task<ApiResponse<OverdueNotificationResultDto>> SendOverdueCreditEmailsAsync()
    {
        var overdueSales = await saleRepository.GetOverdueSalesAsync();
        var result = new OverdueNotificationResultDto
        {
            TotalOverdueFound = overdueSales.Count(),
            TotalEmailsSent = 0,
            FailedCount = 0
        };

        foreach (var sale in overdueSales)
        {
            if (string.IsNullOrEmpty(sale.CustomerUser?.Email))
            {
                result.FailedCount++;
                continue;
            }

            var subject = "Reminder: Overdue Credit Payment";
            var body = $@"
                <div style='font-family: Arial, sans-serif;'>
                    <h3>Hello {sale.CustomerUser.FullName},</h3>
                    <p>This is a reminder that your payment for <strong>Sale #{sale.SaleInvoiceNo}</strong> is overdue.</p>
                    <p>
                        <strong>Balance Due:</strong> {sale.BalanceDue:C}<br/>
                        <strong>Due Date:</strong> {sale.DueDate?.ToShortDateString()}
                    </p>
                    <p>Please settle the payment as soon as possible to avoid any service interruptions.</p>
                    <p>Regards,<br/><strong>Vehicle Parts Management Team</strong></p>
                </div>";

            var sent = await emailService.SendEmailAsync(sale.CustomerUser.Email, subject, body);
            if (sent)
                result.TotalEmailsSent++;
            else
                result.FailedCount++;
        }

        return ApiResponse<OverdueNotificationResultDto>.SuccessResponse(result, "Overdue credit notifications processed");
    }


    public async Task<ApiResponse<LowStockNotificationResultDto>> SendLowStockNotificationsAsync()
    {
        var lowStockParts = await partRepository.GetLowStockPartsAsync();
        var partList = lowStockParts.ToList();

        var result = new LowStockNotificationResultDto
        {
            TotalLowStockPartsFound = partList.Count,
            EmailSent = false
        };

        if (partList.Count > 0)
        {
            var adminEmail = "awantikapokharel46@gmail.com"; 
            var subject = "Alert: Low Stock Inventory Report";
            var body = new StringBuilder();
            body.AppendLine("<div style='font-family: Arial, sans-serif;'>");
            body.AppendLine("<h3>Low Stock Alert</h3>");
            body.AppendLine("<p>The following parts are below their minimum threshold:</p>");
            body.AppendLine("<ul>");

            foreach (var part in partList)
            {
                body.AppendLine($"<li><strong>{part.PartName}</strong> (SKU: {part.Sku}): <span style='color: red;'>{part.CurrentStock} left</span> (Threshold: {part.LowStockThreshold})</li>");
            }

            body.AppendLine("</ul>");
            body.AppendLine("<p>Please restock these items soon.</p>");
            body.AppendLine("</div>");

            result.EmailSent = await emailService.SendEmailAsync(adminEmail, subject, body.ToString());
        }

        return ApiResponse<LowStockNotificationResultDto>.SuccessResponse(result, "Low stock notifications processed");
    }
}
