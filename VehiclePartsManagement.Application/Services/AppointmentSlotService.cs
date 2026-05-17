using Microsoft.EntityFrameworkCore;
using VehiclePartsManagement.Application.Common;
using VehiclePartsManagement.Application.DTOs.AppointmentSlot;
using VehiclePartsManagement.Application.Interfaces.Repositories;
using VehiclePartsManagement.Application.Interfaces.Services;
using VehiclePartsManagement.Domain.Entities;

namespace VehiclePartsManagement.Application.Services;

/// <summary>
/// Implements appointment-slot business rules and response shaping.
/// </summary>
public class AppointmentSlotService(IAppointmentSlotRepository appointmentSlotRepository) : IAppointmentSlotService
{
    /// <inheritdoc />
    public async Task<PagedResponse<List<AppointmentSlotListItemDto>>> GetSlotsAsync(AppointmentSlotListQueryDto query)
    {
        var page = query.Page <= 0 ? 1 : query.Page;
        var pageSize = query.PageSize <= 0 ? 10 : query.PageSize;

        var slotQuery = appointmentSlotRepository.GetSlotQuery(trackChanges: false);

        if (query.Date.HasValue)
        {
            var day = DateTime.SpecifyKind(query.Date.Value.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
            slotQuery = slotQuery.Where(s => s.SlotDate == day);
        }

        if (query.IsActive.HasValue)
        {
            slotQuery = slotQuery.Where(s => s.IsActive == query.IsActive.Value);
        }

        var totalItems = await slotQuery.CountAsync();

        var items = await slotQuery
            .OrderBy(s => s.SlotDate)
            .ThenBy(s => s.SlotTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new AppointmentSlotListItemDto
            {
                AppointmentSlotId = s.AppointmentSlotId,
                SlotDate = DateOnly.FromDateTime(s.SlotDate),
                SlotTime = s.SlotTime,
                MaxCapacity = s.MaxCapacity,
                IsActive = s.IsActive
            })
            .ToListAsync();

        return PagedResponse<List<AppointmentSlotListItemDto>>.SuccessPagedResponse(
            items,
            new PaginationMeta
            {
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)pageSize)
            },
            "Appointment slots retrieved successfully.");
    }


    /// <inheritdoc />
    public async Task<ApiResponse<AppointmentSlotDetailDto>> GetByIdAsync(int appointmentSlotId)
    {
        var slot = await appointmentSlotRepository.GetByIdAsync(appointmentSlotId);
        if (slot is null)
        {
            return ApiResponse<AppointmentSlotDetailDto>.FailureResponse(
                "Appointment slot not found.",
                new Dictionary<string, List<string>>
                {
                    ["appointmentSlotId"] = ["Appointment slot does not exist."]
                });
        }

        return ApiResponse<AppointmentSlotDetailDto>.SuccessResponse(MapDetail(slot), "Appointment slot retrieved successfully.");
    }


    /// <inheritdoc />
    public async Task<ApiResponse<AppointmentSlotDetailDto>> CreateAsync(CreateAppointmentSlotDto dto)
    {
        if (!TimeOnly.TryParse(dto.SlotTime, out _))
        {
            return ApiResponse<AppointmentSlotDetailDto>.FailureResponse(
                "Slot time is invalid.",
                new Dictionary<string, List<string>>
                {
                    ["slotTime"] = ["Slot time is invalid."]
                });
        }

        var slotDate = DateTime.SpecifyKind(dto.SlotDate.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
        var slotTime = dto.SlotTime.Trim();

        if (await appointmentSlotRepository.ExistsByDateAndTimeAsync(slotDate, slotTime))
        {
            return ApiResponse<AppointmentSlotDetailDto>.FailureResponse(
                "Appointment slot already exists for the same date and time.",
                new Dictionary<string, List<string>>
                {
                    ["slot"] = ["Appointment slot already exists for the same date and time."]
                });
        }

        var slot = new AppointmentSlot
        {
            SlotDate = slotDate,
            SlotTime = slotTime,
            MaxCapacity = dto.MaxCapacity,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        appointmentSlotRepository.Create(slot);
        await appointmentSlotRepository.SaveChangesAsync();

        return ApiResponse<AppointmentSlotDetailDto>.SuccessResponse(
            MapDetail(slot),
            "Appointment slot created successfully.");
    }


    /// <inheritdoc />
    public async Task<ApiResponse<AppointmentSlotUpdateResponseDto>> UpdateAsync(int appointmentSlotId, UpdateAppointmentSlotDto dto)
    {
        if (!TimeOnly.TryParse(dto.SlotTime, out _))
        {
            return ApiResponse<AppointmentSlotUpdateResponseDto>.FailureResponse(
                "Slot time is invalid.",
                new Dictionary<string, List<string>>
                {
                    ["slotTime"] = ["Slot time is invalid."]
                });
        }

        var slot = await appointmentSlotRepository.GetByIdAsync(appointmentSlotId);
        if (slot is null)
        {
            return ApiResponse<AppointmentSlotUpdateResponseDto>.FailureResponse(
                "Appointment slot not found.",
                new Dictionary<string, List<string>>
                {
                    ["appointmentSlotId"] = ["Appointment slot does not exist."]
                });
        }

        var slotDate = DateTime.SpecifyKind(dto.SlotDate.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
        var slotTime = dto.SlotTime.Trim();

        if (await appointmentSlotRepository.ExistsByDateAndTimeAsync(slotDate, slotTime, appointmentSlotId))
        {
            return ApiResponse<AppointmentSlotUpdateResponseDto>.FailureResponse(
                "Another appointment slot already exists for the same date and time.",
                new Dictionary<string, List<string>>
                {
                    ["slot"] = ["Another appointment slot already exists for the same date and time."]
                });
        }

        slot.SlotDate = slotDate;
        slot.SlotTime = slotTime;
        slot.MaxCapacity = dto.MaxCapacity;
        slot.IsActive = dto.IsActive;

        appointmentSlotRepository.Update(slot);
        await appointmentSlotRepository.SaveChangesAsync();

        return ApiResponse<AppointmentSlotUpdateResponseDto>.SuccessResponse(
            new AppointmentSlotUpdateResponseDto
            {
                AppointmentSlotId = slot.AppointmentSlotId,
                IsActive = slot.IsActive
            },
            "Appointment slot updated successfully.");
    }


    private static AppointmentSlotDetailDto MapDetail(AppointmentSlot slot) =>
        new()
        {
            AppointmentSlotId = slot.AppointmentSlotId,
            SlotDate = DateOnly.FromDateTime(slot.SlotDate),
            SlotTime = slot.SlotTime,
            MaxCapacity = slot.MaxCapacity,
            IsActive = slot.IsActive
        };
}
