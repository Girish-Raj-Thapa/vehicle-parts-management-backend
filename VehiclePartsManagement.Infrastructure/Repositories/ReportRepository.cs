using Microsoft.EntityFrameworkCore;
using VehiclePartsManagement.Application.DTOs.Reports;
using VehiclePartsManagement.Application.Interfaces.Repositories;
using VehiclePartsManagement.Infrastructure.Data;

namespace VehiclePartsManagement.Infrastructure.Repositories;

public class ReportRepository(AppDbContext context) : IReportRepository
{
    protected AppDbContext Context { get; } = context;

    public async Task<IEnumerable<RegularCustomerReportDto>> GetRegularCustomersAsync()
    {
        return await Context.Sales
            .GroupBy(s => s.CustomerApplicationUserId)
            .Where(g => g.Count() > 1)
            .Select(g => new RegularCustomerReportDto
            {
                CustomerId = g.Key,
                FullName = g.First().CustomerUser.FullName,
                Email = g.First().CustomerUser.Email ?? string.Empty,
                PhoneNumber = g.First().CustomerUser.PhoneNumber ?? string.Empty,
                TotalOrders = g.Count(),
                TotalSpent = g.Sum(s => s.TotalAmount),
                LastPurchaseDate = g.Max(s => s.SaleDate)
            })
            .ToListAsync();
    }

    public async Task<IEnumerable<TopSpenderReportDto>> GetTopSpendersAsync()
    {
        return await Context.Sales
            .GroupBy(s => s.CustomerApplicationUserId)
            .Select(g => new TopSpenderReportDto
            {
                CustomerId = g.Key,
                FullName = g.First().CustomerUser.FullName,
                Email = g.First().CustomerUser.Email ?? string.Empty,
                PhoneNumber = g.First().CustomerUser.PhoneNumber ?? string.Empty,
                TotalOrders = g.Count(),
                TotalSpent = g.Sum(s => s.TotalAmount)
            })
            .OrderByDescending(dto => dto.TotalSpent)
            .ToListAsync();
    }

    public async Task<IEnumerable<PendingCreditReportDto>> GetPendingCreditsAsync()
    {
        var today = DateTime.UtcNow.Date;
        return await Context.Sales
            .Where(s => s.BalanceDue > 0)
            .Select(s => new PendingCreditReportDto
            {
                CustomerId = s.CustomerApplicationUserId,
                FullName = s.CustomerUser.FullName,
                Email = s.CustomerUser.Email ?? string.Empty,
                PhoneNumber = s.CustomerUser.PhoneNumber ?? string.Empty,
                SaleId = s.SaleId,
                InvoiceNo = s.SaleInvoiceNo,
                TotalAmount = s.TotalAmount,
                PaidAmount = s.TotalAmount - s.BalanceDue,
                BalanceDue = s.BalanceDue,
                DueDate = s.DueDate,
                IsOverdue = s.DueDate < today
            })
            .ToListAsync();
    }
}
