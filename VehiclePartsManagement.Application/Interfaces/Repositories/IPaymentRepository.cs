using VehiclePartsManagement.Domain.Entities;

namespace VehiclePartsManagement.Application.Interfaces.Repositories;

public interface IPaymentRepository : IRepositoryBase<Payment>
{
    /// <summary>
    /// Returns all payments for a sale, most recent first.
    /// </summary>
    Task<List<Payment>> GetBySaleIdAsync(int saleId);

    /// <summary>
    /// Returns total paid amount for a specific sale.
    /// </summary>
    Task<decimal> GetTotalPaidBySaleIdAsync(int saleId);
}
