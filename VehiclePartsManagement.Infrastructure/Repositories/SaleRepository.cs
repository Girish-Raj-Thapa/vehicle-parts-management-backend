using Microsoft.EntityFrameworkCore;
using VehiclePartsManagement.Application.Interfaces.Repositories;
using VehiclePartsManagement.Domain.Entities;
using VehiclePartsManagement.Infrastructure.Data;

namespace VehiclePartsManagement.Infrastructure.Repositories;

public class SaleRepository(AppDbContext context)
    : RepositoryBase<Sale>(context), ISaleRepository
{
    public Task<List<Sale>> GetCustomerSalesAsync(string customerId) =>
        Context.Sales
            .Include(sale => sale.Vehicle)
            .Include(sale => sale.SaleItems)
                .ThenInclude(item => item.Part)
            .Where(sale => sale.CustomerApplicationUserId == customerId)
            .OrderByDescending(sale => sale.SaleDate)
            .AsNoTracking()
            .ToListAsync();
}
