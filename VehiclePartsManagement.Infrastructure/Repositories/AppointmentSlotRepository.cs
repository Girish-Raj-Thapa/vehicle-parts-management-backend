using Microsoft.EntityFrameworkCore;
using VehiclePartsManagement.Application.Interfaces.Repositories;
using VehiclePartsManagement.Domain.Entities;
using VehiclePartsManagement.Infrastructure.Data;

namespace VehiclePartsManagement.Infrastructure.Repositories;

/// <summary>
/// EF Core repository implementation for appointment slot queries.
/// </summary>
public class AppointmentSlotRepository(AppDbContext context)
    : RepositoryBase<AppointmentSlot>(context), IAppointmentSlotRepository
{
    /// <inheritdoc />
    public IQueryable<AppointmentSlot> GetSlotQuery(bool trackChanges = false) =>
        trackChanges ? Context.AppointmentSlots : Context.AppointmentSlots.AsNoTracking();


    /// <inheritdoc />
    public async Task<bool> ExistsByDateAndTimeAsync(DateTime slotDate, string slotTime, int? excludeSlotId = null)
    {
        var normalizedTime = slotTime.Trim();

        var query = Context.AppointmentSlots
            .AsNoTracking()
            .Where(s => s.SlotDate == slotDate && s.SlotTime == normalizedTime);

        if (excludeSlotId.HasValue)
        {
            query = query.Where(s => s.AppointmentSlotId != excludeSlotId.Value);
        }

        return await query.AnyAsync();
    }
}
