using System.Text;
using VehiclePartsManagement.Application.Common;
using VehiclePartsManagement.Application.DTOs.Notifications;
using VehiclePartsManagement.Application.Interfaces.Repositories;
using VehiclePartsManagement.Application.Interfaces.Services;

namespace VehiclePartsManagement.Infrastructure.Services;

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
            var body = $"Hello {sale.CustomerUser.FullName},\n\n" +
                       $"This is a reminder that your payment for Sale #{sale.SaleInvoiceNo} is overdue.\n" +
                       $"Balance Due: {sale.BalanceDue:C}\n" +
                       $"Due Date: {sale.DueDate?.ToShortDateString()}\n\n" +
                       $"Please settle the payment as soon as possible.\n\n" +
                       $"Regards,\nVehicle Parts Management Team";

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
            var adminEmail = "admin@vehicleparts.com"; // Targeted notification for staff/admin
            var subject = "Alert: Low Stock Inventory Report";
            var body = new StringBuilder();
            body.AppendLine("The following parts are below their minimum threshold:");
            body.AppendLine();
            
            foreach (var part in partList)
            {
                body.AppendLine($"- {part.PartName} (SKU: {part.Sku}): {part.CurrentStock} left (Threshold: {part.LowStockThreshold})");
            }
            
            body.AppendLine();
            body.AppendLine("Please restock these items soon.");

            result.EmailSent = await emailService.SendEmailAsync(adminEmail, subject, body.ToString());
        }

        return ApiResponse<LowStockNotificationResultDto>.SuccessResponse(result, "Low stock notifications processed");
    }
}
