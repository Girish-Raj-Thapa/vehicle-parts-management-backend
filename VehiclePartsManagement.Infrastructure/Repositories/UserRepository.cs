using Microsoft.EntityFrameworkCore;
using VehiclePartsManagement.Application.Interfaces.Repositories;
using VehiclePartsManagement.Domain.Entities;
using VehiclePartsManagement.Infrastructure.Data;

namespace VehiclePartsManagement.Infrastructure.Repositories;

public class UserRepository(AppDbContext context)
    : RepositoryBase<ApplicationUser>(context), IUserRepository
{
    public Task<ApplicationUser?> GetCustomerWithVehiclesAsync(string customerId) =>
        Context.Users
            .Include(user => user.Vehicles)
                .ThenInclude(vehicle => vehicle.Model)
                    .ThenInclude(model => model.Brand)
            .FirstOrDefaultAsync(user => user.Id == customerId);

    private const string CustomerRoleName = "Customer";
    private const string SearchByName = "name";
    private const string SearchByPhone = "phone";
    private const string SearchById = "id";
    private const string SearchByVehicle = "vehicle";


    /// <summary>
    /// Applies customer-role filtering, then search filtering, and finally server-side paging.
    /// </summary>
    public async Task<(List<ApplicationUser> Customers, int TotalItems)> SearchCustomersAsync(
        string query,
        string type,
        int page,
        int pageSize)
    {
        var normalizedQuery = query.Trim();
        var normalizedType = type.Trim().ToLowerInvariant();

        var customerUsers = GetCustomerUsersQuery();
        var filteredUsers = ApplySearchFilter(customerUsers, normalizedQuery, normalizedType);

        var totalItems = await filteredUsers.CountAsync();
        if (totalItems == 0)
        {
            return ([], 0);
        }

        var pagedUserIds = await filteredUsers
            .OrderBy(u => u.FullName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => u.Id)
            .ToListAsync();

        if (pagedUserIds.Count == 0)
        {
            return ([], totalItems);
        }

        // Second query loads requested page with navigation graph for response mapping.
        var users = await Context.Users
            .AsNoTracking()
            .Where(u => pagedUserIds.Contains(u.Id))
            .Include(u => u.Vehicles)
            .ThenInclude(v => v.Model)
            .ThenInclude(m => m.Brand)
            .ToListAsync();

        var usersById = users.ToDictionary(u => u.Id);
        var orderedUsers = pagedUserIds
            .Where(usersById.ContainsKey)
            .Select(id => usersById[id])
            .ToList();

        return (orderedUsers, totalItems);
    }


    private IQueryable<ApplicationUser> GetCustomerUsersQuery()
    {
        // IdentityRole tables are used to keep customer-only results without schema changes.
        var customerRoleIds = Context.Roles
            .AsNoTracking()
            .Where(r => r.Name == CustomerRoleName)
            .Select(r => r.Id);

        var customerUserIds = Context.UserRoles
            .AsNoTracking()
            .Where(ur => customerRoleIds.Contains(ur.RoleId))
            .Select(ur => ur.UserId);

        return Context.Users
            .AsNoTracking()
            .Where(u => customerUserIds.Contains(u.Id));
    }


    private static IQueryable<ApplicationUser> ApplySearchFilter(
        IQueryable<ApplicationUser> users,
        string query,
        string type)
    {
        // ILIKE keeps matching case-insensitive on PostgreSQL.
        var pattern = $"%{query}%";
        return type switch
        {
            SearchByName => FilterByName(users, pattern),
            SearchByPhone => FilterByPhone(users, pattern),
            SearchById => FilterById(users, pattern),
            SearchByVehicle => FilterByVehicle(users, pattern),
            _ => FilterByAll(users, pattern)
        };
    }


    private static IQueryable<ApplicationUser> FilterByName(IQueryable<ApplicationUser> users, string pattern) =>
        users.Where(u => EF.Functions.ILike(u.FullName, pattern));


    private static IQueryable<ApplicationUser> FilterByPhone(IQueryable<ApplicationUser> users, string pattern) =>
        users.Where(u => EF.Functions.ILike(u.PhoneNumber ?? string.Empty, pattern));


    private static IQueryable<ApplicationUser> FilterById(IQueryable<ApplicationUser> users, string pattern) =>
        users.Where(u => EF.Functions.ILike(u.Id, pattern));


    private static IQueryable<ApplicationUser> FilterByVehicle(IQueryable<ApplicationUser> users, string pattern) =>
        users.Where(u => u.Vehicles.Any(v =>
            EF.Functions.ILike(v.VehicleNumber, pattern) ||
            EF.Functions.ILike(v.Model.ModelName, pattern) ||
            EF.Functions.ILike(v.Model.Brand.BrandName, pattern)));


    private static IQueryable<ApplicationUser> FilterByAll(IQueryable<ApplicationUser> users, string pattern) =>
        users.Where(u =>
            EF.Functions.ILike(u.FullName, pattern) ||
            EF.Functions.ILike(u.PhoneNumber ?? string.Empty, pattern) ||
            EF.Functions.ILike(u.Id, pattern) ||
            u.Vehicles.Any(v =>
                EF.Functions.ILike(v.VehicleNumber, pattern) ||
                EF.Functions.ILike(v.Model.ModelName, pattern) ||
                EF.Functions.ILike(v.Model.Brand.BrandName, pattern)));

    public async Task<(int TotalSalesCount, decimal TotalAmountSpent)> GetCustomerSalesSummaryAsync(string customerId)
    {
        var query = Context.Sales
            .AsNoTracking()
            .Where(s => s.CustomerApplicationUserId == customerId && s.SaleStatus != VehiclePartsManagement.Domain.Enums.SaleStatus.Cancelled);

        var totalCount = await query.CountAsync();
        var totalAmount = await query.SumAsync(s => (decimal?)s.TotalAmount) ?? 0m;

        return (totalCount, Math.Round(totalAmount, 2, MidpointRounding.AwayFromZero));
    }
}
