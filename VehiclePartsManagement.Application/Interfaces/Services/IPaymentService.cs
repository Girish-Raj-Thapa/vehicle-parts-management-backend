using VehiclePartsManagement.Application.Common;
using VehiclePartsManagement.Application.DTOs.Payment;

namespace VehiclePartsManagement.Application.Interfaces.Services;

public interface IPaymentService
{
    Task<ApiResponse<KhaltiInitiateResponseDto>> InitiateKhaltiPaymentAsync(
        KhaltiInitiatePaymentDto dto,
        string userId,
        bool canManagePayments);

    Task<ApiResponse<KhaltiVerifyResponseDto>> VerifyKhaltiPaymentAsync(
        KhaltiVerifyPaymentDto dto,
        string? userId,
        bool canManagePayments);
}
