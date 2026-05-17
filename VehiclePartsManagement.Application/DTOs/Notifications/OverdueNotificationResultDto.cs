namespace VehiclePartsManagement.Application.DTOs.Notifications;

public class OverdueNotificationResultDto
{
    public int TotalOverdueFound { get; set; }
    public int TotalEmailsSent { get; set; }
    public int FailedCount { get; set; }
}
