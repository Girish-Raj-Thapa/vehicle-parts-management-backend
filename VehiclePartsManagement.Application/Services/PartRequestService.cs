using VehiclePartsManagement.Application.Common;
using VehiclePartsManagement.Application.DTOs.PartRequest;
using VehiclePartsManagement.Application.Interfaces.Repositories;
using VehiclePartsManagement.Application.Interfaces.Services;
using VehiclePartsManagement.Domain.Entities;
using VehiclePartsManagement.Domain.Enums;

namespace VehiclePartsManagement.Application.Services;

public class PartRequestService(IPartRequestRepository partRequestRepository) : IPartRequestService
{
    private static readonly Dictionary<PartRequestStatus, PartRequestStatus[]> AllowedTransitions = new()
    {
        [PartRequestStatus.Pending] = [PartRequestStatus.Ordered, PartRequestStatus.Cancelled],
        [PartRequestStatus.Ordered] = [PartRequestStatus.Received, PartRequestStatus.Cancelled],
        [PartRequestStatus.Received] = [PartRequestStatus.Completed, PartRequestStatus.Cancelled],
        [PartRequestStatus.Completed] = [],
        [PartRequestStatus.Cancelled] = []
    };


    /// <summary>
    /// Creates a customer part request after validating vehicle ownership.
    /// </summary>
    public async Task<ApiResponse<PartRequestCreateResponseDto>> CreateAsync(
        CreatePartRequestDto dto,
        string applicationUserId)
    {
        if (!await partRequestRepository.IsVehicleOwnedByUserAsync(dto.VehicleId, applicationUserId))
        {
            return ApiResponse<PartRequestCreateResponseDto>.FailureResponse(
                "Selected vehicle does not belong to the current user.",
                new Dictionary<string, List<string>>
                {
                    { "vehicleId", ["Selected vehicle does not belong to the current user."] }
                });
        }

        var partRequest = new PartRequest
        {
            ApplicationUserId = applicationUserId,
            VehicleId = dto.VehicleId,
            RequestedPartName = dto.RequestedPartName.Trim(),
            RequestDescription = dto.RequestDescription.Trim(),
            RequestDate = DateTime.UtcNow,
            RequestStatus = PartRequestStatus.Pending
        };

        partRequestRepository.Create(partRequest);
        await partRequestRepository.SaveChangesAsync();

        return ApiResponse<PartRequestCreateResponseDto>.SuccessResponse(
            new PartRequestCreateResponseDto
            {
                PartRequestId = partRequest.PartRequestId,
                RequestStatus = partRequest.RequestStatus.ToString()
            },
            "Part request submitted successfully");
    }


    /// <summary>
    /// Retrieves paged part requests with optional owner and status filters.
    /// </summary>
    public async Task<PagedResponse<List<PartRequestListItemDto>>> GetPagedAsync(
        PartRequestQueryDto query,
        string? applicationUserId)
    {
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize is < 1 or > 100 ? 10 : query.PageSize;

        PartRequestStatus? status = null;
        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            if (!Enum.TryParse<PartRequestStatus>(query.Status, true, out var parsedStatus))
            {
                return PagedFailure("Part request status is invalid.", "status");
            }

            status = parsedStatus;
        }

        var scopedUserId = query.Mine ? applicationUserId : null;
        var (items, totalItems) = await partRequestRepository.GetPagedAsync(page, pageSize, scopedUserId, status);

        var data = items.Select(MapListItem).ToList();

        return PagedResponse<List<PartRequestListItemDto>>.SuccessPagedResponse(
            data,
            new PaginationMeta
            {
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
            },
            "Part requests retrieved successfully");
    }


    /// <summary>
    /// Retrieves a single part request, optionally constrained to owner scope.
    /// </summary>
    public async Task<ApiResponse<PartRequestDetailDto>> GetByIdAsync(
        int partRequestId,
        string? applicationUserId)
    {
        var partRequest = string.IsNullOrWhiteSpace(applicationUserId)
            ? await partRequestRepository.GetByIdWithDetailsAsync(partRequestId)
            : await partRequestRepository.GetByIdForUserAsync(partRequestId, applicationUserId);

        if (partRequest is null)
        {
            return ApiResponse<PartRequestDetailDto>.FailureResponse("Part request not found.");
        }

        return ApiResponse<PartRequestDetailDto>.SuccessResponse(
            MapDetail(partRequest),
            "Part request retrieved successfully");
    }


    /// <summary>
    /// Updates part-request status and fulfilled-part linkage for staff/admin processing.
    /// </summary>
    public async Task<ApiResponse<PartRequestUpdateResponseDto>> UpdateAsync(
        int partRequestId,
        UpdatePartRequestDto dto)
    {
        if (!Enum.TryParse<PartRequestStatus>(dto.RequestStatus, true, out var newStatus))
        {
            return ApiResponse<PartRequestUpdateResponseDto>.FailureResponse(
                "Part request status is invalid.",
                new Dictionary<string, List<string>>
                {
                    { "requestStatus", ["Part request status is invalid."] }
                });
        }

        var partRequest = partRequestRepository.GetById(partRequestId);
        if (partRequest is null)
        {
            return ApiResponse<PartRequestUpdateResponseDto>.FailureResponse("Part request not found.");
        }

        if (!IsTransitionAllowed(partRequest.RequestStatus, newStatus))
        {
            return ApiResponse<PartRequestUpdateResponseDto>.FailureResponse(
                $"Invalid status transition from {partRequest.RequestStatus} to {newStatus}.",
                new Dictionary<string, List<string>>
                {
                    { "requestStatus", [$"Invalid status transition from {partRequest.RequestStatus} to {newStatus}."] }
                });
        }

        // Completed requests must point to a concrete inventory part.
        if (newStatus == PartRequestStatus.Completed)
        {
            if (!dto.FulfilledPartId.HasValue)
            {
                return ApiResponse<PartRequestUpdateResponseDto>.FailureResponse(
                    "Fulfilled part is required when request status is Completed.",
                    new Dictionary<string, List<string>>
                    {
                        { "fulfilledPartId", ["Fulfilled part is required when request status is Completed."] }
                    });
            }

            if (!await partRequestRepository.FulfilledPartExistsAsync(dto.FulfilledPartId.Value))
            {
                return ApiResponse<PartRequestUpdateResponseDto>.FailureResponse(
                    "Fulfilled part not found.",
                    new Dictionary<string, List<string>>
                    {
                        { "fulfilledPartId", ["Fulfilled part not found."] }
                    });
            }
        }

        // Non-completed states should not keep fulfilled-part links.
        if (newStatus != PartRequestStatus.Completed)
        {
            dto.FulfilledPartId = null;
        }

        partRequest.RequestStatus = newStatus;
        partRequest.FulfilledPartId = dto.FulfilledPartId;

        partRequestRepository.Update(partRequest);
        await partRequestRepository.SaveChangesAsync();

        return ApiResponse<PartRequestUpdateResponseDto>.SuccessResponse(
            new PartRequestUpdateResponseDto
            {
                PartRequestId = partRequest.PartRequestId,
                RequestStatus = partRequest.RequestStatus.ToString(),
                FulfilledPartId = partRequest.FulfilledPartId
            },
            "Part request updated successfully");
    }


    private static bool IsTransitionAllowed(PartRequestStatus currentStatus, PartRequestStatus newStatus)
    {
        if (currentStatus == newStatus)
        {
            return true;
        }

        return AllowedTransitions.TryGetValue(currentStatus, out var allowedStatuses) &&
               allowedStatuses.Contains(newStatus);
    }


    private static PartRequestListItemDto MapListItem(PartRequest partRequest) =>
        new()
        {
            PartRequestId = partRequest.PartRequestId,
            RequestedPartName = partRequest.RequestedPartName,
            CustomerName = partRequest.User.FullName,
            VehicleName = partRequest.Vehicle is null
                ? string.Empty
                : $"{partRequest.Vehicle.Model.Brand.BrandName} {partRequest.Vehicle.Model.ModelName}",
            RequestDate = DateOnly.FromDateTime(partRequest.RequestDate),
            RequestStatus = partRequest.RequestStatus.ToString(),
            FulfilledPartId = partRequest.FulfilledPartId
        };


    private static PartRequestDetailDto MapDetail(PartRequest partRequest) =>
        new()
        {
            PartRequestId = partRequest.PartRequestId,
            UserId = partRequest.ApplicationUserId,
            CustomerName = partRequest.User.FullName,
            VehicleId = partRequest.VehicleId,
            VehicleName = partRequest.Vehicle is null
                ? string.Empty
                : $"{partRequest.Vehicle.Model.Brand.BrandName} {partRequest.Vehicle.Model.ModelName}",
            RequestedPartName = partRequest.RequestedPartName,
            RequestDescription = partRequest.RequestDescription,
            RequestDate = DateOnly.FromDateTime(partRequest.RequestDate),
            RequestStatus = partRequest.RequestStatus.ToString(),
            FulfilledPartId = partRequest.FulfilledPartId
        };


    private static PagedResponse<List<PartRequestListItemDto>> PagedFailure(
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
            Data = new List<PartRequestListItemDto>(),
            Pagination = new PaginationMeta()
        };
}
