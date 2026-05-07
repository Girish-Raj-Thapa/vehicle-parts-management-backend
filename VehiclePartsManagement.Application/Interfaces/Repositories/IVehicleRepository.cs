using VehiclePartsManagement.Domain.Entities;

namespace VehiclePartsManagement.Application.Interfaces.Repositories;

public interface IVehicleRepository : IRepositoryBase<Vehicle>
{
    /// <summary>
    /// Returns paged vehicles with optional owner-scope and search filtering.
    /// </summary>
    Task<(List<Vehicle> Items, int TotalItems)> GetPagedAsync(
        int page,
        int pageSize,
        string? applicationUserId,
        string? search);

    /// <summary>
    /// Returns a single vehicle with full details including owner, model, brand, and images.
    /// </summary>
    Task<Vehicle?> GetByIdWithDetailsAsync(int vehicleId);

    /// <summary>
    /// Returns a single vehicle for a specific user scope.
    /// </summary>
    Task<Vehicle?> GetByIdForUserAsync(int vehicleId, string applicationUserId);

    /// <summary>
    /// Returns service records for a vehicle with optional pagination.
    /// </summary>
    Task<(List<ServiceRecord> Items, int TotalItems)> GetServiceHistoryAsync(
        int vehicleId,
        int page,
        int pageSize);

    /// <summary>
    /// Returns sales for a vehicle with items and optional pagination.
    /// </summary>
    Task<(List<Sale> Items, int TotalItems)> GetSaleHistoryAsync(
        int vehicleId,
        int page,
        int pageSize);

    /// <summary>
    /// Checks whether the vehicle belongs to the provided user.
    /// </summary>
    Task<bool> IsVehicleOwnedByUserAsync(int vehicleId, string applicationUserId);
}
