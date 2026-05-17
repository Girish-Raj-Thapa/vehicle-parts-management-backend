using VehiclePartsManagement.Application.Common;
using VehiclePartsManagement.Application.DTOs.AppointmentSlot;

namespace VehiclePartsManagement.Application.Interfaces.Services;

/// <summary>
/// Exposes appointment-slot management operations for staff/admin workflows.
/// </summary>
public interface IAppointmentSlotService
{
    Task<PagedResponse<List<AppointmentSlotListItemDto>>> GetSlotsAsync(AppointmentSlotListQueryDto query);
    Task<ApiResponse<AppointmentSlotDetailDto>> GetByIdAsync(int appointmentSlotId);
    Task<ApiResponse<AppointmentSlotDetailDto>> CreateAsync(CreateAppointmentSlotDto dto);
    Task<ApiResponse<AppointmentSlotUpdateResponseDto>> UpdateAsync(int appointmentSlotId, UpdateAppointmentSlotDto dto);
}
