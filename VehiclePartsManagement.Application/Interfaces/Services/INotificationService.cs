using VehiclePartsManagement.Application.Common;
using VehiclePartsManagement.Application.DTOs.Notifications;

namespace VehiclePartsManagement.Application.Interfaces.Services;

public interface INotificationService
{
    Task<ApiResponse<OverdueNotificationResultDto>> SendOverdueCreditEmailsAsync();
    Task<ApiResponse<LowStockNotificationResultDto>> SendLowStockNotificationsAsync();
}
