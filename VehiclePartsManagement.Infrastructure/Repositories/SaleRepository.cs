using Microsoft.EntityFrameworkCore;
using VehiclePartsManagement.Application.Interfaces.Repositories;
using VehiclePartsManagement.Domain.Entities;
using VehiclePartsManagement.Infrastructure.Data;

namespace VehiclePartsManagement.Infrastructure.Repositories;

public class SaleRepository(AppDbContext context)
    : RepositoryBase<Sale>(context), ISaleRepository
{
    public async Task<Sale?> GetSaleWithDetailsAsync(int saleId)
    {
        return await Context.Sales
            .Include(s => s.CustomerUser)
            .Include(s => s.SaleItems)
                .ThenInclude(si => si.Part)
            .Include(s => s.Payments)
            .FirstOrDefaultAsync(s => s.SaleId == saleId);
    }

    public async Task<IEnumerable<Sale>> GetOverdueSalesAsync()
    {
        var today = DateTime.UtcNow.Date;
        return await Context.Sales
            .Include(s => s.CustomerUser)
            .Where(s => s.BalanceDue > 0 && s.DueDate < today)
            .ToListAsync();
    }

    public async Task<IEnumerable<Sale>> GetAllSalesAsync()
    {
        return await Context.Sales
            .Include(s => s.CustomerUser)
            .OrderByDescending(s => s.SaleDate)
            .ToListAsync();
    }
}
