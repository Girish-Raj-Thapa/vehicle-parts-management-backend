using VehiclePartsManagement.Application.Common;
using VehiclePartsManagement.Application.DTOs.ServiceAppointment;

namespace VehiclePartsManagement.Application.Interfaces.Services;

public interface IServiceAppointmentService
{
    Task<ApiResponse<ServiceAppointmentCreateResponseDto>> CreateAsync(
        CreateServiceAppointmentDto dto,
        string applicationUserId);

    Task<PagedResponse<List<ServiceAppointmentListItemDto>>> GetPagedAsync(
        ServiceAppointmentQueryDto query,
        string? applicationUserId);

    Task<ApiResponse<ServiceAppointmentDetailDto>> GetByIdAsync(
        int appointmentId,
        string? applicationUserId);
}
