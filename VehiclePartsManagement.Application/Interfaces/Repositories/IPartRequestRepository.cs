using VehiclePartsManagement.Domain.Entities;
using VehiclePartsManagement.Domain.Enums;

namespace VehiclePartsManagement.Application.Interfaces.Repositories;

public interface IPartRequestRepository : IRepositoryBase<PartRequest>
{
    /// <summary>
    /// Returns paged part requests with optional owner and status filters.
    /// </summary>
    Task<(List<PartRequest> Items, int TotalItems)> GetPagedAsync(
        int page,
        int pageSize,
        string? applicationUserId,
        PartRequestStatus? status);

    /// <summary>
    /// Returns one part request with related user/vehicle/part detail.
    /// </summary>
    Task<PartRequest?> GetByIdWithDetailsAsync(int partRequestId);

    /// <summary>
    /// Returns one part request constrained to a specific owner scope.
    /// </summary>
    Task<PartRequest?> GetByIdForUserAsync(int partRequestId, string applicationUserId);

    /// <summary>
    /// Checks whether the vehicle belongs to the provided user.
    /// </summary>
    Task<bool> IsVehicleOwnedByUserAsync(int vehicleId, string applicationUserId);

    /// <summary>
    /// Checks whether the fulfilled part exists.
    /// </summary>
    Task<bool> FulfilledPartExistsAsync(int partId);
}
