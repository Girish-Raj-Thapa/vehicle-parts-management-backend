using Microsoft.AspNetCore.Mvc;
using VehiclePartsManagement.Application.Interfaces.Services;

namespace VehiclePartsManagement.Presentation.Controllers;

[ApiController]
[Route("api/notifications")]
public class NotificationsController(INotificationService notificationService) : ControllerBase
{
    [HttpPost("send-overdue-credit-emails")]
    public async Task<IActionResult> SendOverdueCreditEmails()
    {
        var response = await notificationService.SendOverdueCreditEmailsAsync();
        return Ok(response);
    }

    [HttpPost("send-low-stock-emails")]
    public async Task<IActionResult> SendLowStockEmails()
    {
        var response = await notificationService.SendLowStockNotificationsAsync();
        return Ok(response);
    }
}
