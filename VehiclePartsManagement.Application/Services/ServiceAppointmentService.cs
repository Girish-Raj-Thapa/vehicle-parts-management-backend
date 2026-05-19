using VehiclePartsManagement.Application.Common;
using VehiclePartsManagement.Application.DTOs.ServiceAppointment;
using VehiclePartsManagement.Application.Interfaces.Repositories;
using VehiclePartsManagement.Application.Interfaces.Services;
using VehiclePartsManagement.Domain.Entities;
using VehiclePartsManagement.Domain.Enums;

namespace VehiclePartsManagement.Application.Services;

public class ServiceAppointmentService(
    IServiceAppointmentRepository serviceAppointmentRepository,
    IAppointmentSlotRepository appointmentSlotRepository) : IServiceAppointmentService
{
    private static readonly ServiceAppointmentStatus[] AllowedStaffStatuses =
    [
        ServiceAppointmentStatus.Pending,
        ServiceAppointmentStatus.Confirmed,
        ServiceAppointmentStatus.Completed,
        ServiceAppointmentStatus.Cancelled
    ];

    /// <summary>
    /// Creates a new service appointment under the provided customer scope.
    /// </summary>
    public async Task<ApiResponse<ServiceAppointmentCreateResponseDto>> CreateAsync(
        CreateServiceAppointmentDto dto,
        string applicationUserId)
    {
        if (!await serviceAppointmentRepository.IsVehicleOwnedByUserAsync(dto.VehicleId, applicationUserId))
        {
            return ApiResponse<ServiceAppointmentCreateResponseDto>.FailureResponse(
                "Selected vehicle does not belong to the current user.",
                new Dictionary<string, List<string>>
                {
                    { "vehicleId", ["Selected vehicle does not belong to the current user."] }
                });
        }

        var selectedDate = dto.AppointmentDate;
        var selectedTime = dto.AppointmentTime?.Trim();

        // When slot id is provided, slot metadata becomes the source of truth for date/time.
        if (dto.AppointmentSlotId.HasValue)
        {
            var slot = await appointmentSlotRepository.GetByIdAsync(dto.AppointmentSlotId.Value);
            if (slot is null)
            {
                return ApiResponse<ServiceAppointmentCreateResponseDto>.FailureResponse(
                    "Appointment slot not found.",
                    new Dictionary<string, List<string>>
                    {
                        { "appointmentSlotId", ["Appointment slot not found."] }
                    });
            }

            if (!slot.IsActive)
            {
                return ApiResponse<ServiceAppointmentCreateResponseDto>.FailureResponse(
                    "Appointment slot is inactive.",
                    new Dictionary<string, List<string>>
                    {
                        { "appointmentSlotId", ["Appointment slot is inactive."] }
                    });
            }

            selectedDate = DateOnly.FromDateTime(slot.SlotDate);
            selectedTime = slot.SlotTime;

            var activeCount = await serviceAppointmentRepository.CountActiveByDateTimeAsync(slot.SlotDate, slot.SlotTime);
            if (activeCount >= slot.MaxCapacity)
            {
                return ApiResponse<ServiceAppointmentCreateResponseDto>.FailureResponse(
                    "Appointment slot is full.",
                    new Dictionary<string, List<string>>
                    {
                        { "appointmentSlotId", ["Appointment slot is full."] }
                    });
            }
        }

        if (!dto.AppointmentSlotId.HasValue && !selectedDate.HasValue)
        {
            return ApiResponse<ServiceAppointmentCreateResponseDto>.FailureResponse(
                "Appointment date is required when appointment slot is not selected.",
                new Dictionary<string, List<string>>
                {
                    { "appointmentDate", ["Appointment date is required when appointment slot is not selected."] }
                });
        }

        if (!dto.AppointmentSlotId.HasValue && string.IsNullOrWhiteSpace(selectedTime))
        {
            return ApiResponse<ServiceAppointmentCreateResponseDto>.FailureResponse(
                "Appointment time is required when appointment slot is not selected.",
                new Dictionary<string, List<string>>
                {
                    { "appointmentTime", ["Appointment time is required when appointment slot is not selected."] }
                });
        }

        if (selectedDate.HasValue && selectedDate.Value < DateOnly.FromDateTime(DateTime.UtcNow))
        {
            return ApiResponse<ServiceAppointmentCreateResponseDto>.FailureResponse(
                "Appointment date cannot be in the past.",
                new Dictionary<string, List<string>>
                {
                    { "appointmentDate", ["Appointment date cannot be in the past."] }
                });
        }

        if (!string.IsNullOrWhiteSpace(selectedTime) && !TimeOnly.TryParse(selectedTime, out _))
        {
            return ApiResponse<ServiceAppointmentCreateResponseDto>.FailureResponse(
                "Appointment time is invalid.",
                new Dictionary<string, List<string>>
                {
                    { "appointmentTime", ["Appointment time is invalid."] }
                });
        }

        if (!Enum.TryParse<ServiceAppointmentType>(dto.AppointmentType, true, out var appointmentType))
        {
            return ApiResponse<ServiceAppointmentCreateResponseDto>.FailureResponse(
                "Appointment type is invalid.",
                new Dictionary<string, List<string>>
                {
                    { "appointmentType", ["Appointment type is invalid."] }
                });
        }

        var appointment = new ServiceAppointment
        {
            ApplicationUserId = applicationUserId,
            VehicleId = dto.VehicleId,
            AppointmentDate = DateTime.SpecifyKind(
                selectedDate!.Value.ToDateTime(TimeOnly.MinValue),
                DateTimeKind.Utc),
            AppointmentTime = selectedTime!,
            AppointmentType = appointmentType,
            AppointmentStatus = ServiceAppointmentStatus.Pending,
            AppointmentNote = dto.AppointmentNote
        };

        serviceAppointmentRepository.Create(appointment);
        await serviceAppointmentRepository.SaveChangesAsync();

        return ApiResponse<ServiceAppointmentCreateResponseDto>.SuccessResponse(
            new ServiceAppointmentCreateResponseDto
            {
                AppointmentId = appointment.ServiceAppointmentId,
                AppointmentStatus = appointment.AppointmentStatus.ToString()
            },
            "Appointment booked successfully");
    }


    /// <summary>
    /// Returns paged appointments with optional owner and status filtering.
    /// </summary>
    public async Task<PagedResponse<List<ServiceAppointmentListItemDto>>> GetPagedAsync(
        ServiceAppointmentQueryDto query,
        string? applicationUserId)
    {
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize is < 1 or > 100 ? 10 : query.PageSize;

        ServiceAppointmentStatus? status = null;
        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            if (!Enum.TryParse<ServiceAppointmentStatus>(query.Status, true, out var parsedStatus))
            {
                return PagedFailure(
                    "Appointment status is invalid.",
                    "status");
            }

            status = parsedStatus;
        }

        var scopedUserId = query.Mine ? applicationUserId : null;
        var (items, totalItems) = await serviceAppointmentRepository.GetPagedAsync(
            page,
            pageSize,
            scopedUserId,
            status,
            query.Date);

        var data = items.Select(MapListItem).ToList();

        return PagedResponse<List<ServiceAppointmentListItemDto>>.SuccessPagedResponse(
            data,
            new PaginationMeta
            {
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
            },
            "Appointments retrieved successfully");
    }


    /// <summary>
    /// Returns one appointment detail, optionally constrained to a specific owner scope.
    /// </summary>
    public async Task<ApiResponse<ServiceAppointmentDetailDto>> GetByIdAsync(
        int appointmentId,
        string? applicationUserId)
    {
        var appointment = string.IsNullOrWhiteSpace(applicationUserId)
            ? await serviceAppointmentRepository.GetByIdWithDetailsAsync(appointmentId)
            : await serviceAppointmentRepository.GetByIdForUserAsync(appointmentId, applicationUserId);

        if (appointment is null)
        {
            return ApiResponse<ServiceAppointmentDetailDto>.FailureResponse("Appointment not found.");
        }

        return ApiResponse<ServiceAppointmentDetailDto>.SuccessResponse(
            MapDetail(appointment),
            "Appointment retrieved successfully");
    }


    /// <summary>
    /// Updates appointment status and note for staff/admin appointment management.
    /// </summary>
    public async Task<ApiResponse<ServiceAppointmentUpdateResponseDto>> UpdateAsync(
        int appointmentId,
        UpdateServiceAppointmentDto dto)
    {
        if (!Enum.TryParse<ServiceAppointmentStatus>(dto.AppointmentStatus, true, out var newStatus))
        {
            return ApiResponse<ServiceAppointmentUpdateResponseDto>.FailureResponse(
                "Appointment status is invalid.",
                new Dictionary<string, List<string>>
                {
                    { "appointmentStatus", ["Appointment status is invalid."] }
                });
        }

        if (!AllowedStaffStatuses.Contains(newStatus))
        {
            return ApiResponse<ServiceAppointmentUpdateResponseDto>.FailureResponse(
                "Appointment status is not allowed in this update flow.",
                new Dictionary<string, List<string>>
                {
                    { "appointmentStatus", ["Appointment status is not allowed in this update flow."] }
                });
        }

        var appointment = await serviceAppointmentRepository.GetByIdForUpdateAsync(appointmentId);
        if (appointment is null)
        {
            return ApiResponse<ServiceAppointmentUpdateResponseDto>.FailureResponse("Appointment not found.");
        }

        appointment.AppointmentStatus = newStatus;
        appointment.AppointmentNote = dto.AppointmentNote.Trim();

        serviceAppointmentRepository.Update(appointment);
        await serviceAppointmentRepository.SaveChangesAsync();

        return ApiResponse<ServiceAppointmentUpdateResponseDto>.SuccessResponse(
            new ServiceAppointmentUpdateResponseDto
            {
                AppointmentId = appointment.ServiceAppointmentId,
                AppointmentStatus = appointment.AppointmentStatus.ToString()
            },
            "Appointment updated successfully");
    }


    private static ServiceAppointmentListItemDto MapListItem(ServiceAppointment appointment) =>
        new()
        {
            AppointmentId = appointment.ServiceAppointmentId,
            CustomerName = appointment.User.FullName,
            VehicleNumber = appointment.Vehicle.VehicleNumber,
            VehicleName = $"{appointment.Vehicle.Model.Brand.BrandName} {appointment.Vehicle.Model.ModelName}",
            AppointmentDate = DateOnly.FromDateTime(appointment.AppointmentDate),
            AppointmentTime = appointment.AppointmentTime,
            AppointmentType = appointment.AppointmentType.ToString(),
            AppointmentStatus = appointment.AppointmentStatus.ToString(),
            AppointmentNote = appointment.AppointmentNote
        };


    private static ServiceAppointmentDetailDto MapDetail(ServiceAppointment appointment) =>
        new()
        {
            AppointmentId = appointment.ServiceAppointmentId,
            UserId = appointment.ApplicationUserId,
            CustomerName = appointment.User.FullName,
            VehicleId = appointment.VehicleId,
            VehicleNumber = appointment.Vehicle.VehicleNumber,
            VehicleName = $"{appointment.Vehicle.Model.Brand.BrandName} {appointment.Vehicle.Model.ModelName}",
            AppointmentDate = DateOnly.FromDateTime(appointment.AppointmentDate),
            AppointmentTime = appointment.AppointmentTime,
            AppointmentType = appointment.AppointmentType.ToString(),
            AppointmentStatus = appointment.AppointmentStatus.ToString(),
            AppointmentNote = appointment.AppointmentNote
        };


    private static PagedResponse<List<ServiceAppointmentListItemDto>> PagedFailure(
        string message,
        string errorKey) =>
        new()
        {
            Success = false,
            Message = message,
            Errors = new Dictionary<string, List<string>>
            {
                { errorKey, [message] }
            },
            Data = new List<ServiceAppointmentListItemDto>(),
            Pagination = new PaginationMeta()
        };
}
