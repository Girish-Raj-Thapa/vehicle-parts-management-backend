using VehiclePartsManagement.Application.Common;
using VehiclePartsManagement.Application.DTOs.Vehicle;

namespace VehiclePartsManagement.Application.Interfaces.Services;

public interface IVehicleService
{
    Task<ApiResponse<VehicleRegistrationResponseDto>> RegisterAsync(
        VehicleRegistrationDto dto,
        string userId);
}
