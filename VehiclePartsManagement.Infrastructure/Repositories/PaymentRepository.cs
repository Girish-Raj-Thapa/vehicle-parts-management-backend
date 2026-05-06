using Microsoft.EntityFrameworkCore;
using VehiclePartsManagement.Application.Interfaces.Repositories;
using VehiclePartsManagement.Domain.Entities;
using VehiclePartsManagement.Infrastructure.Data;

namespace VehiclePartsManagement.Infrastructure.Repositories;

public class PaymentRepository(AppDbContext context)
    : RepositoryBase<Payment>(context), IPaymentRepository
{
    /// <summary>
    /// Retrieves sale payments ordered by newest entry first.
    /// </summary>
    public Task<List<Payment>> GetBySaleIdAsync(int saleId) =>
        Context.Payments
            .AsNoTracking()
            .Where(p => p.SaleId == saleId)
            .OrderByDescending(p => p.PaymentDate)
            .ThenByDescending(p => p.PaymentId)
            .ToListAsync();


    /// <summary>
    /// Calculates total paid amount for one sale.
    /// </summary>
    public async Task<decimal> GetTotalPaidBySaleIdAsync(int saleId)
    {
        var totalPaid = await Context.Payments
            .AsNoTracking()
            .Where(p => p.SaleId == saleId)
            .SumAsync(p => (decimal?)p.PaymentAmount);

        return totalPaid ?? 0m;
    }
}
