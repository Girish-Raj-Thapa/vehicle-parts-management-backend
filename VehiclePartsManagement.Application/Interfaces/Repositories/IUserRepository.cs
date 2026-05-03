using VehiclePartsManagement.Domain.Entities;

namespace VehiclePartsManagement.Application.Interfaces.Repositories;

public interface IUserRepository : IRepositoryBase<ApplicationUser>
{
    /// <summary>
    /// Searches customer-role users by a specific search strategy and returns paged users with total count.
    /// </summary>
    Task<(List<ApplicationUser> Customers, int TotalItems)> SearchCustomersAsync(
        string query,
        string type,
        int page,
        int pageSize);
}
