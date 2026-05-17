using Microsoft.EntityFrameworkCore;
using VehiclePartsManagement.Application.Interfaces.Repositories;
using VehiclePartsManagement.Domain.Entities;
using VehiclePartsManagement.Domain.Enums;
using VehiclePartsManagement.Infrastructure.Data;

namespace VehiclePartsManagement.Infrastructure.Repositories;

public class ServiceAppointmentRepository(AppDbContext context)
    : RepositoryBase<ServiceAppointment>(context), IServiceAppointmentRepository
{
    /// <summary>
    /// Retrieves appointments with optional owner/status filtering and pagination.
    /// </summary>
    public async Task<(List<ServiceAppointment> Items, int TotalItems)> GetPagedAsync(
        int page,
        int pageSize,
        string? applicationUserId,
        ServiceAppointmentStatus? status,
        DateOnly? appointmentDate = null)
    {
        var query = Context.ServiceAppointments
            .AsNoTracking()
            .Include(sa => sa.User)
            .Include(sa => sa.Vehicle)
            .ThenInclude(v => v.Model)
            .ThenInclude(m => m.Brand)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(applicationUserId))
        {
            query = query.Where(sa => sa.ApplicationUserId == applicationUserId);
        }

        if (status.HasValue)
        {
            query = query.Where(sa => sa.AppointmentStatus == status.Value);
        }

        if (appointmentDate.HasValue)
        {
            var from = DateTime.SpecifyKind(appointmentDate.Value.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
            var to = from.AddDays(1);
            query = query.Where(sa => sa.AppointmentDate >= from && sa.AppointmentDate < to);
        }

        // Count query is executed separately for pagination metadata.
        var totalItems = await query.CountAsync();
        if (totalItems == 0)
        {
            return ([], 0);
        }

        var items = await query
            .OrderByDescending(sa => sa.AppointmentDate)
            .ThenByDescending(sa => sa.ServiceAppointmentId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalItems);
    }


    /// <summary>
    /// Retrieves appointment detail with related customer and vehicle context.
    /// </summary>
    public Task<ServiceAppointment?> GetByIdWithDetailsAsync(int appointmentId) =>
        Context.ServiceAppointments
            .AsNoTracking()
            .Include(sa => sa.User)
            .Include(sa => sa.Vehicle)
            .ThenInclude(v => v.Model)
            .ThenInclude(m => m.Brand)
            .FirstOrDefaultAsync(sa => sa.ServiceAppointmentId == appointmentId);


    /// <summary>
    /// Retrieves appointment detail constrained to a single customer scope.
    /// </summary>
    public Task<ServiceAppointment?> GetByIdForUserAsync(int appointmentId, string applicationUserId) =>
        Context.ServiceAppointments
            .AsNoTracking()
            .Include(sa => sa.User)
            .Include(sa => sa.Vehicle)
            .ThenInclude(v => v.Model)
            .ThenInclude(m => m.Brand)
            .FirstOrDefaultAsync(sa =>
                sa.ServiceAppointmentId == appointmentId &&
                sa.ApplicationUserId == applicationUserId);


    /// <summary>
    /// Retrieves tracked appointment entity for staff/admin status update flow.
    /// </summary>
    public Task<ServiceAppointment?> GetByIdForUpdateAsync(int appointmentId) =>
        Context.ServiceAppointments
            .FirstOrDefaultAsync(sa => sa.ServiceAppointmentId == appointmentId);


    /// <summary>
    /// Verifies vehicle ownership for appointment booking authorization checks.
    /// </summary>
    public Task<bool> IsVehicleOwnedByUserAsync(int vehicleId, string applicationUserId) =>
        Context.Vehicles
            .AsNoTracking()
            .AnyAsync(v => v.VehicleId == vehicleId && v.ApplicationUserId == applicationUserId);
}
