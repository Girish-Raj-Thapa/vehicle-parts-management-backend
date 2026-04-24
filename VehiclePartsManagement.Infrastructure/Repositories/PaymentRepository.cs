using VehiclePartsManagement.Application.Interfaces.Repositories;
using VehiclePartsManagement.Domain.Entities;
using VehiclePartsManagement.Infrastructure.Data;

namespace VehiclePartsManagement.Infrastructure.Repositories;

public class PaymentRepository(AppDbContext context)
    : RepositoryBase<Payment>(context), IPaymentRepository
{
}
