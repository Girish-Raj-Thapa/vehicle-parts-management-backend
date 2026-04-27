namespace VehiclePartsManagement.Application.DTOs.Notifications;

public class LowStockNotificationResultDto
{
    public int TotalLowStockPartsFound { get; set; }
    public bool EmailSent { get; set; }
}
