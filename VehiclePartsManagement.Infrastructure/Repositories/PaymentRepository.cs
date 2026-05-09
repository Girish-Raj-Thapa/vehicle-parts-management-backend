using Microsoft.EntityFrameworkCore;
using VehiclePartsManagement.Application.Interfaces.Repositories;
using VehiclePartsManagement.Domain.Entities;
using VehiclePartsManagement.Domain.Enums;
using VehiclePartsManagement.Infrastructure.Data;

namespace VehiclePartsManagement.Infrastructure.Repositories;

public class PaymentRepository(AppDbContext context)
    : RepositoryBase<Payment>(context), IPaymentRepository
{
    public async Task<(List<Payment> Items, int TotalItems)> GetCustomerPaymentHistoryAsync(
        string customerUserId,
        int page,
        int pageSize)
    {
        var query = Context.Payments
            .AsNoTracking()
            .Include(p => p.Sale)
            .Where(p =>
                p.Sale.CustomerApplicationUserId == customerUserId &&
                p.Sale.SaleStatus != SaleStatus.Draft &&
                p.Sale.SaleStatus != SaleStatus.Cancelled);

        var totalItems = await query.CountAsync();
        if (totalItems == 0)
        {
            return ([], 0);
        }

        var items = await query
            .OrderByDescending(p => p.PaymentDate)
            .ThenByDescending(p => p.PaymentId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalItems);
    }
}
