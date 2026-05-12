using VehiclePartsManagement.Application.Common;
using VehiclePartsManagement.Application.DTOs.Report;
using VehiclePartsManagement.Application.Interfaces.Repositories;
using VehiclePartsManagement.Application.Interfaces.Services;
using VehiclePartsManagement.Domain.Enums;

namespace VehiclePartsManagement.Application.Services;

public class ReportService(IReportRepository reportRepository) : IReportService
{
    public async Task<ApiResponse<SalesRevenueReportResponseDto>> GetSalesRevenueReportAsync(ReportQueryDto query)
    {
        var hasExplicitGroupBy = !string.IsNullOrWhiteSpace(query.GroupBy);
        var groupBy = hasExplicitGroupBy ? NormalizeGroupBy(query.GroupBy) : null;
        if (hasExplicitGroupBy && groupBy is null)
        {
            return ApiResponse<SalesRevenueReportResponseDto>.FailureResponse("Invalid groupBy. Use: day, month, year.");
        }

        var rangeValidation = await ValidateAndResolveSalesRevenueRangeAsync(query.From, query.To, groupBy);
        if (!rangeValidation.Success || rangeValidation.Data is not ReportDateRangeDto range)
        {
            return ApiResponse<SalesRevenueReportResponseDto>.FailureResponse(rangeValidation.Message, rangeValidation.Errors);
        }

        var effectiveGroupBy = groupBy ?? "day";

        var sales = await reportRepository.GetSalesWithDetailsAsync(range.FromUtc, range.ToUtc, query.IncludeCancelled);
        if (!string.IsNullOrWhiteSpace(query.CustomerId))
        {
            sales = sales.Where(s => s.CustomerApplicationUserId == query.CustomerId).ToList();
        }

        var totalSalesAmount = Round2(sales.Sum(s => s.TotalAmount));
        var totalInvoices = sales.Count;
        var totalDiscountAmount = Round2(sales.Sum(s => s.DiscountAmount));

        // Assumption without schema changes:
        // cashSale = fully settled invoice, creditSale = invoice with due balance.
        var cashSalesAmount = Round2(sales.Where(s => s.BalanceDue <= 0).Sum(s => s.TotalAmount));
        var creditSalesAmount = Round2(sales.Where(s => s.BalanceDue > 0).Sum(s => s.TotalAmount));

        var collections = sales
            .GroupBy(s => BuildPeriodKey(s.SaleDate, effectiveGroupBy))
            .Select(g => new SalesRevenuePeriodItemDto
            {
                Period = g.Key,
                SalesAmount = Round2(g.Sum(x => x.TotalAmount)),
                InvoiceCount = g.Count(),
                DiscountAmount = Round2(g.Sum(x => x.DiscountAmount))
            })
            .OrderBy(x => x.Period)
            .ToList();

        var data = new SalesRevenueReportResponseDto
        {
            Meta = BuildMeta("Sales Revenue Report", range, effectiveGroupBy, query.IncludeCancelled, query.CustomerId, query.VendorId, "Cash/Credit split inferred using BalanceDue."),
            TotalSalesAmount = totalSalesAmount,
            TotalInvoices = totalInvoices,
            CashSalesAmount = cashSalesAmount,
            CreditSalesAmount = creditSalesAmount,
            TotalDiscountAmount = totalDiscountAmount,
            Collections = collections
        };

        return ApiResponse<SalesRevenueReportResponseDto>.SuccessResponse(data, "Sales revenue report generated successfully.");
    }


    public async Task<ApiResponse<PurchaseExpenseReportResponseDto>> GetPurchaseExpenseReportAsync(ReportQueryDto query)
    {
        var rangeValidation = ValidateAndResolveRange(query.From, query.To);
        if (!rangeValidation.Success || rangeValidation.Data is not ReportDateRangeDto range)
        {
            return ApiResponse<PurchaseExpenseReportResponseDto>.FailureResponse(rangeValidation.Message, rangeValidation.Errors);
        }
        
        var groupBy = NormalizeGroupBy(query.GroupBy);
        if (groupBy is null)
        {
            return ApiResponse<PurchaseExpenseReportResponseDto>.FailureResponse("Invalid groupBy. Use: day, month, year.");
        }
        
        var purchases = await reportRepository.GetPurchasesWithDetailsAsync(range.FromUtc, range.ToUtc, query.IncludeCancelled);
        if (query.VendorId.HasValue)
        {
            purchases = purchases.Where(p => p.VendorId == query.VendorId.Value).ToList();
        }

        var totalPurchaseAmount = Round2(purchases.Sum(p => p.TotalAmount));
        var totalPurchaseInvoices = purchases.Count;

        var vendorSummaries = purchases
            .GroupBy(p => new { p.VendorId, Name = p.Vendor.VendorName })
            .Select(g => new PurchaseExpenseVendorItemDto
            {
                VendorId = g.Key.VendorId,
                VendorName = g.Key.Name,
                PurchaseAmount = Round2(g.Sum(x => x.TotalAmount)),
                InvoiceCount = g.Count()
            })
            .OrderByDescending(x => x.PurchaseAmount)
            .ToList();

        var partSummaries = purchases
            .SelectMany(p => p.PurchaseItems)
            .GroupBy(pi => new { pi.PartId, Name = pi.Part.PartName })
            .Select(g => new PurchaseExpensePartItemDto
            {
                PartId = g.Key.PartId,
                PartName = g.Key.Name,
                QuantityPurchased = g.Sum(x => x.QuantityPurchased),
                PurchaseAmount = Round2(g.Sum(x => x.PurchaseLineTotal))
            })
            .OrderByDescending(x => x.PurchaseAmount)
            .ToList();

        var collections = purchases
            .GroupBy(p => BuildPeriodKey(p.PurchaseDate, groupBy))
            .Select(g => new PurchaseExpensePeriodItemDto
            {
                Period = g.Key,
                PurchaseAmount = Round2(g.Sum(x => x.TotalAmount)),
                InvoiceCount = g.Count()
            })
            .OrderBy(x => x.Period)
            .ToList();

        var data = new PurchaseExpenseReportResponseDto
        {
            Meta = BuildMeta("Purchase Expense Report", range, groupBy, query.IncludeCancelled, query.CustomerId, query.VendorId, "Purchase cost and summaries from purchase and purchase-item records."),
            TotalPurchaseAmount = totalPurchaseAmount,
            TotalPurchaseInvoices = totalPurchaseInvoices,
            VendorSummaries = vendorSummaries,
            PartSummaries = partSummaries,
            Collections = collections
        };

        return ApiResponse<PurchaseExpenseReportResponseDto>.SuccessResponse(data, "Purchase expense report generated successfully.");
    }

    public async Task<ApiResponse<PaymentCollectionReportResponseDto>> GetPaymentCollectionReportAsync(
        ReportQueryDto query)
    {
        var rangeValidation = ValidateAndResolveRange(query.From, query.To);
        if (!rangeValidation.Success || rangeValidation.Data is not ReportDateRangeDto range)
        {
            return ApiResponse<PaymentCollectionReportResponseDto>.FailureResponse(rangeValidation.Message, rangeValidation.Errors);
        }

        var groupBy = NormalizeGroupBy(query.GroupBy);
        if (groupBy is null)
        {
            return ApiResponse<PaymentCollectionReportResponseDto>.FailureResponse("Invalid groupBy. Use: day, month, year.");
        }

        var payments = await reportRepository.GetPaymentsWithSaleAsync(range.FromUtc, range.ToUtc);
        if (!string.IsNullOrWhiteSpace(query.CustomerId))
        {
            payments = payments.Where(p => p.Sale.CustomerApplicationUserId == query.CustomerId).ToList();
        }

        var salesInRange = await reportRepository.GetSalesWithBalancesAsync(range.FromUtc, range.ToUtc, query.IncludeCancelled);
        if (!string.IsNullOrWhiteSpace(query.CustomerId))
        {
            salesInRange = salesInRange.Where(s => s.CustomerApplicationUserId == query.CustomerId).ToList();
        }

        var totalCollectedAmount = Round2(payments.Sum(p => p.PaymentAmount));
        var cashPaymentsAmount = Round2(payments.Where(p => p.PaymentMethod == PaymentMethodType.Cash).Sum(p => p.PaymentAmount));
        var onlinePaymentsAmount = Round2(payments.Where(p =>
                p.PaymentMethod == PaymentMethodType.Card ||
                p.PaymentMethod == PaymentMethodType.MobileWallet ||
                p.PaymentMethod == PaymentMethodType.BankTransfer)
            .Sum(p => p.PaymentAmount));
        var bankPaymentsAmount = Round2(payments.Where(p => p.PaymentMethod == PaymentMethodType.BankTransfer).Sum(p => p.PaymentAmount));

        var partialPaymentsAmount = Round2(salesInRange
            .Where(s => s.PaymentStatus == PaymentStatus.PartiallyPaid)
            .Sum(s => s.TotalAmount - s.BalanceDue));

        var unpaidBalanceAmount = Round2(salesInRange.Sum(s => s.BalanceDue));

        var methodBreakdown = payments
            .GroupBy(p => p.PaymentMethod.ToString())
            .Select(g => new PaymentMethodBreakdownItemDto
            {
                PaymentMethod = g.Key,
                Amount = Round2(g.Sum(x => x.PaymentAmount)),
                Count = g.Count()
            })
            .OrderByDescending(x => x.Amount)
            .ToList();

        var collections = payments
            .GroupBy(p => BuildPeriodKey(p.PaymentDate, groupBy))
            .Select(g => new PaymentCollectionPeriodItemDto
            {
                Period = g.Key,
                CollectedAmount = Round2(g.Sum(x => x.PaymentAmount)),
                PaymentCount = g.Count()
            })
            .OrderBy(x => x.Period)
            .ToList();

        var data = new PaymentCollectionReportResponseDto
        {
            Meta = BuildMeta("Payment Collection Report", range, groupBy, query.IncludeCancelled, query.CustomerId, query.VendorId, "Online includes Card, MobileWallet, BankTransfer."),
            TotalCollectedAmount = totalCollectedAmount,
            CashPaymentsAmount = cashPaymentsAmount,
            OnlinePaymentsAmount = onlinePaymentsAmount,
            BankPaymentsAmount = bankPaymentsAmount,
            PartialPaymentsAmount = partialPaymentsAmount,
            UnpaidBalanceAmount = unpaidBalanceAmount,
            MethodBreakdown = methodBreakdown,
            Collections = collections
        };

        return ApiResponse<PaymentCollectionReportResponseDto>.SuccessResponse(data, "Payment collection report generated successfully.");
    }


    public async Task<ApiResponse<ReceivableReportResponseDto>> GetReceivableReportAsync(ReportQueryDto query)
    {
        var rangeValidation = ValidateAndResolveRange(query.From, query.To);
        if (!rangeValidation.Success || rangeValidation.Data is not ReportDateRangeDto range)
        {
            return ApiResponse<ReceivableReportResponseDto>.FailureResponse(rangeValidation.Message, rangeValidation.Errors);
        }

        var groupBy = NormalizeGroupBy(query.GroupBy) ?? "day";
        var overdueDays = query.OverdueDays <= 0 ? 30 : query.OverdueDays;
        var overdueThreshold = DateTime.UtcNow.Date.AddDays(-overdueDays);

        var sales = await reportRepository.GetSalesWithBalancesAsync(range.FromUtc, range.ToUtc, query.IncludeCancelled);
        sales = sales.Where(s => s.BalanceDue > 0).ToList();

        if (!string.IsNullOrWhiteSpace(query.CustomerId))
        {
            sales = sales.Where(s => s.CustomerApplicationUserId == query.CustomerId).ToList();
        }

        var totalCreditOutstanding = Round2(sales.Sum(s => s.BalanceDue));
        var overdueSales = sales.Where(s => s.DueDate.HasValue && s.DueDate.Value.Date <= overdueThreshold).ToList();
        var overdueCreditAmount = Round2(overdueSales.Sum(s => s.BalanceDue));

        var customerBalances = sales
            .GroupBy(s => new { s.CustomerApplicationUserId, Name = s.CustomerUser.FullName })
            .Select(g => new ReceivableCustomerBalanceItemDto
            {
                CustomerId = g.Key.CustomerApplicationUserId,
                CustomerName = g.Key.Name,
                PendingBalance = Round2(g.Sum(x => x.BalanceDue)),
                OverdueBalance = Round2(g.Where(x => x.DueDate.HasValue && x.DueDate.Value.Date <= overdueThreshold).Sum(x => x.BalanceDue)),
                InvoiceCount = g.Count()
            })
            .OrderByDescending(x => x.PendingBalance)
            .ToList();

        var overdueInvoices = overdueSales
            .Select(s => new OverdueInvoiceItemDto
            {
                SaleId = s.SaleId,
                InvoiceNo = s.SaleInvoiceNo,
                CustomerName = s.CustomerUser.FullName,
                DueDate = s.DueDate!.Value,
                BalanceDue = Round2(s.BalanceDue),
                DaysOverdue = (DateTime.UtcNow.Date - s.DueDate!.Value.Date).Days
            })
            .OrderByDescending(x => x.DaysOverdue)
            .ToList();

        var data = new ReceivableReportResponseDto
        {
            Meta = BuildMeta("Credit / Receivable Report", range, groupBy, query.IncludeCancelled, query.CustomerId, query.VendorId, $"Overdue uses {overdueDays}-day threshold."),
            TotalCreditOutstanding = totalCreditOutstanding,
            OverdueCreditAmount = overdueCreditAmount,
            CustomerBalances = customerBalances,
            OverdueInvoices = overdueInvoices
        };

        return ApiResponse<ReceivableReportResponseDto>.SuccessResponse(data, "Receivable report generated successfully.");
    }

    public async Task<ApiResponse<ProfitLossReportResponseDto>> GetProfitLossReportAsync(ReportQueryDto query)
    {
        var rangeValidation = ValidateAndResolveRange(query.From, query.To);
        if (!rangeValidation.Success || rangeValidation.Data is not ReportDateRangeDto range)
        {
            return ApiResponse<ProfitLossReportResponseDto>.FailureResponse(rangeValidation.Message, rangeValidation.Errors);
        }

        var groupBy = NormalizeGroupBy(query.GroupBy);
        if (groupBy is null)
        {
            return ApiResponse<ProfitLossReportResponseDto>.FailureResponse("Invalid groupBy. Use: day, month, year.");
        }

        var sales = await reportRepository.GetSalesWithDetailsAsync(range.FromUtc, range.ToUtc, query.IncludeCancelled);
        var purchases = await reportRepository.GetPurchasesWithDetailsAsync(range.FromUtc, range.ToUtc, query.IncludeCancelled);
        var services = await reportRepository.GetServiceRecordsAsync(range.FromUtc, range.ToUtc);

        if (!string.IsNullOrWhiteSpace(query.CustomerId))
        {
            sales = sales.Where(s => s.CustomerApplicationUserId == query.CustomerId).ToList();
        }

        if (query.VendorId.HasValue)
        {
            purchases = purchases.Where(p => p.VendorId == query.VendorId.Value).ToList();
        }

        var totalSalesRevenue = Round2(sales.Sum(s => s.TotalAmount));
        var totalPurchaseCost = Round2(purchases.Sum(p => p.TotalAmount));
        var serviceLaborIncome = Round2(services.Sum(sr => sr.LaborCharge));
        var totalDiscountAmount = Round2(sales.Sum(s => s.DiscountAmount));
        var estimatedGrossProfit = Round2(totalSalesRevenue - totalPurchaseCost);
        var estimatedNetProfit = Round2(estimatedGrossProfit + serviceLaborIncome);

        var periodKeys = sales.Select(s => BuildPeriodKey(s.SaleDate, groupBy))
            .Union(purchases.Select(p => BuildPeriodKey(p.PurchaseDate, groupBy)))
            .Union(services.Select(sr => BuildPeriodKey(sr.ServiceDate, groupBy)))
            .Distinct()
            .OrderBy(x => x)
            .ToList();

        var collections = periodKeys.Select(period =>
        {
            var periodSales = sales.Where(s => BuildPeriodKey(s.SaleDate, groupBy) == period).ToList();
            var periodPurchases = purchases.Where(p => BuildPeriodKey(p.PurchaseDate, groupBy) == period).ToList();
            var periodServices = services.Where(sr => BuildPeriodKey(sr.ServiceDate, groupBy) == period).ToList();

            var pSales = Round2(periodSales.Sum(x => x.TotalAmount));
            var pPurchase = Round2(periodPurchases.Sum(x => x.TotalAmount));
            var pService = Round2(periodServices.Sum(x => x.LaborCharge));
            var pDiscount = Round2(periodSales.Sum(x => x.DiscountAmount));
            var pGross = Round2(pSales - pPurchase);
            var pNet = Round2(pGross + pService);

            return new ProfitLossPeriodItemDto
            {
                Period = period,
                SalesRevenue = pSales,
                PurchaseCost = pPurchase,
                ServiceLaborIncome = pService,
                DiscountAmount = pDiscount,
                EstimatedGrossProfit = pGross,
                EstimatedNetProfit = pNet
            };
        }).ToList();

        var data = new ProfitLossReportResponseDto
        {
            Meta = BuildMeta("Profit & Loss Report", range, groupBy, query.IncludeCancelled, query.CustomerId, query.VendorId, "Estimated only. No operating-expense/tax ledger in current schema."),
            TotalSalesRevenue = totalSalesRevenue,
            TotalPurchaseCost = totalPurchaseCost,
            ServiceLaborIncome = serviceLaborIncome,
            TotalDiscountAmount = totalDiscountAmount,
            EstimatedGrossProfit = estimatedGrossProfit,
            EstimatedNetProfit = estimatedNetProfit,
            Collections = collections
        };

        return ApiResponse<ProfitLossReportResponseDto>.SuccessResponse(data, "Profit and loss report generated successfully.");
    }

    public async Task<ApiResponse<DailyCashFlowReportResponseDto>> GetDailyCashFlowReportAsync(ReportQueryDto query)
    {
        var rangeValidation = ValidateAndResolveRange(query.From, query.To);
        if (!rangeValidation.Success || rangeValidation.Data is not ReportDateRangeDto range)
        {
            return ApiResponse<DailyCashFlowReportResponseDto>.FailureResponse(rangeValidation.Message, rangeValidation.Errors);
        }

        var payments = await reportRepository.GetPaymentsWithSaleAsync(range.FromUtc, range.ToUtc);
        var purchases = await reportRepository.GetPurchasesWithDetailsAsync(range.FromUtc, range.ToUtc, query.IncludeCancelled);

        if (!string.IsNullOrWhiteSpace(query.CustomerId))
        {
            payments = payments.Where(p => p.Sale.CustomerApplicationUserId == query.CustomerId).ToList();
        }

        if (query.VendorId.HasValue)
        {
            purchases = purchases.Where(p => p.VendorId == query.VendorId.Value).ToList();
        }

        var moneyReceived = Round2(payments.Sum(p => p.PaymentAmount));
        var moneySpentOnPurchases = Round2(purchases.Sum(p => p.TotalAmount));

        // Without explicit ledger table, opening is derived as 0 for reporting range.
        // You can later replace with actual cash ledger opening.
        var openingBalance = 0m;
        var closingBalance = Round2(openingBalance + moneyReceived - moneySpentOnPurchases);

        var startDate = range.FromUtc.Date;
        var endDate = range.ToUtc.Date;

        var dailyFlows = new List<DailyCashFlowDayItemDto>();
        for (var date = startDate; date <= endDate; date = date.AddDays(1))
        {
            var received = Round2(payments.Where(p => p.PaymentDate.Date == date).Sum(p => p.PaymentAmount));
            var spent = Round2(purchases.Where(p => p.PurchaseDate.Date == date).Sum(p => p.TotalAmount));
            dailyFlows.Add(new DailyCashFlowDayItemDto
            {
                DateUtc = date,
                Received = received,
                Spent = spent,
                NetFlow = Round2(received - spent)
            });
        }

        var paymentMethodBreakdown = payments
            .GroupBy(p => p.PaymentMethod.ToString())
            .Select(g => new PaymentMethodBreakdownItemDto
            {
                PaymentMethod = g.Key,
                Amount = Round2(g.Sum(x => x.PaymentAmount)),
                Count = g.Count()
            })
            .OrderByDescending(x => x.Amount)
            .ToList();

        var data = new DailyCashFlowReportResponseDto
        {
            Meta = BuildMeta("Daily Cash Flow Report", range, "day", query.IncludeCancelled, query.CustomerId, query.VendorId, "Derived cash flow. Opening balance defaults to 0 without a cashbook ledger."),
            OpeningBalance = openingBalance,
            MoneyReceived = moneyReceived,
            MoneySpentOnPurchases = moneySpentOnPurchases,
            ClosingBalance = closingBalance,
            PaymentMethodBreakdown = paymentMethodBreakdown,
            DailyFlows = dailyFlows
        };

        return ApiResponse<DailyCashFlowReportResponseDto>.SuccessResponse(data, "Daily cash flow report generated successfully.");
    }

    public async Task<ApiResponse<DiscountReportResponseDto>> GetDiscountReportAsync(ReportQueryDto query)
    {
        var rangeValidation = ValidateAndResolveRange(query.From, query.To);
        if (!rangeValidation.Success || rangeValidation.Data is not ReportDateRangeDto range)
        {
            return ApiResponse<DiscountReportResponseDto>.FailureResponse(rangeValidation.Message, rangeValidation.Errors);
        }

        var groupBy = NormalizeGroupBy(query.GroupBy);
        if (groupBy is null)
        {
            return ApiResponse<DiscountReportResponseDto>.FailureResponse("Invalid groupBy. Use: day, month, year.");
        }

        var sales = await reportRepository.GetSalesWithDetailsAsync(range.FromUtc, range.ToUtc, query.IncludeCancelled);
        if (!string.IsNullOrWhiteSpace(query.CustomerId))
        {
            sales = sales.Where(s => s.CustomerApplicationUserId == query.CustomerId).ToList();
        }

        var discountedSales = sales.Where(s => s.DiscountAmount > 0).ToList();
        var totalDiscountGiven = Round2(discountedSales.Sum(s => s.DiscountAmount));
        var discountedInvoiceCount = discountedSales.Count;
        var loyalty10PercentInvoiceCount = discountedSales.Count(s => s.LoyaltyDiscountPercent == 10);
        var totalSubTotal = Round2(sales.Sum(s => s.SubTotal));
        var impactOnRevenuePercent = totalSubTotal > 0 ? Round2((totalDiscountGiven / totalSubTotal) * 100) : 0;

        var customerDiscounts = discountedSales
            .GroupBy(s => new { s.CustomerApplicationUserId, Name = s.CustomerUser.FullName })
            .Select(g => new DiscountCustomerItemDto
            {
                CustomerId = g.Key.CustomerApplicationUserId,
                CustomerName = g.Key.Name,
                DiscountAmount = Round2(g.Sum(x => x.DiscountAmount)),
                InvoiceCount = g.Count()
            })
            .OrderByDescending(x => x.DiscountAmount)
            .ToList();

        var collections = discountedSales
            .GroupBy(s => BuildPeriodKey(s.SaleDate, groupBy))
            .Select(g => new DiscountPeriodItemDto
            {
                Period = g.Key,
                DiscountAmount = Round2(g.Sum(x => x.DiscountAmount)),
                DiscountedInvoiceCount = g.Count()
            })
            .OrderBy(x => x.Period)
            .ToList();

        var data = new DiscountReportResponseDto
        {
            Meta = BuildMeta("Discount Report", range, groupBy, query.IncludeCancelled, query.CustomerId, query.VendorId, "Impact on revenue calculated against total SubTotal in selected range."),
            TotalDiscountGiven = totalDiscountGiven,
            DiscountedInvoiceCount = discountedInvoiceCount,
            Loyalty10PercentInvoiceCount = loyalty10PercentInvoiceCount,
            ImpactOnRevenuePercent = impactOnRevenuePercent,
            CustomerDiscounts = customerDiscounts,
            Collections = collections
        };

        return ApiResponse<DiscountReportResponseDto>.SuccessResponse(data, "Discount report generated successfully.");
    }

    public async Task<ApiResponse<DashboardSummaryResponseDto>> GetDashboardSummaryAsync(ReportQueryDto query)
    {
        var hasExplicitGroupBy = !string.IsNullOrWhiteSpace(query.GroupBy);
        var groupBy = hasExplicitGroupBy ? NormalizeGroupBy(query.GroupBy) : null;
        if (hasExplicitGroupBy && groupBy is null)
        {
            return ApiResponse<DashboardSummaryResponseDto>.FailureResponse("Invalid groupBy. Use: day, month, year.");
        }

        var rangeValidation = await ValidateAndResolveSummaryRangeAsync(query.From, query.To, groupBy);
        if (!rangeValidation.Success || rangeValidation.Data is not ReportDateRangeDto range)
        {
            return ApiResponse<DashboardSummaryResponseDto>.FailureResponse(rangeValidation.Message, rangeValidation.Errors);
        }

        var nowUtc = DateTime.UtcNow;
        var todayStart = nowUtc.Date;
        var todayEnd = todayStart.AddDays(1).AddTicks(-1);
        var summaryStart = range.FromUtc;
        var summaryEnd = range.ToUtc;

        var monthSales = await reportRepository.GetSalesWithDetailsAsync(summaryStart, summaryEnd, includeCancelled: query.IncludeCancelled);
        var monthPurchases = await reportRepository.GetPurchasesWithDetailsAsync(summaryStart, summaryEnd, includeCancelled: query.IncludeCancelled);
        var monthPayments = await reportRepository.GetPaymentsWithSaleAsync(summaryStart, summaryEnd);
        var monthServices = await reportRepository.GetServiceRecordsAsync(summaryStart, summaryEnd);
        var outstandingSales = await reportRepository.GetSalesWithBalancesAsync(summaryStart, summaryEnd, query.IncludeCancelled);
        outstandingSales = outstandingSales.Where(s => s.BalanceDue > 0).ToList();
        var lowStockParts = await reportRepository.GetLowStockPartsAsync();

        if (!string.IsNullOrWhiteSpace(query.CustomerId))
        {
            monthSales = monthSales.Where(s => s.CustomerApplicationUserId == query.CustomerId).ToList();
            monthPayments = monthPayments.Where(p => p.Sale.CustomerApplicationUserId == query.CustomerId).ToList();
            outstandingSales = outstandingSales.Where(s => s.CustomerApplicationUserId == query.CustomerId).ToList();
        }

        var todaySales = monthSales.Where(s => s.SaleDate >= todayStart && s.SaleDate <= todayEnd).ToList();
        var monthSalesAmount = Round2(monthSales.Sum(s => s.TotalAmount));
        var todaySalesAmount = Round2(todaySales.Sum(s => s.TotalAmount));
        var monthPurchasesAmount = Round2(monthPurchases.Sum(p => p.TotalAmount));
        var monthPaymentsAmount = Round2(monthPayments.Sum(p => p.PaymentAmount));
        var pendingCreditAmount = Round2(outstandingSales.Sum(s => s.BalanceDue));
        var serviceIncome = Round2(monthServices.Sum(sr => sr.LaborCharge));
        var estimatedMonthProfit = Round2((monthSalesAmount - monthPurchasesAmount) + serviceIncome);

        var topPendingReceivables = outstandingSales
            .GroupBy(s => new { s.CustomerApplicationUserId, Name = s.CustomerUser.FullName })
            .Select(g => new PendingReceivableItemDto
            {
                CustomerId = g.Key.CustomerApplicationUserId,
                CustomerName = g.Key.Name,
                BalanceDue = Round2(g.Sum(x => x.BalanceDue))
            })
            .OrderByDescending(x => x.BalanceDue)
            .Take(5)
            .ToList();

        var lowStockRisk = new LowStockFinancialRiskItemDto
        {
            AtRiskPartCount = lowStockParts.Count,
            EstimatedRiskValue = Round2(lowStockParts.Sum(p =>
            {
                var shortageUnits = Math.Max(0, p.LowStockThreshold - p.CurrentStock + 1);
                return shortageUnits * p.SellingPrice;
            }))
        };

        var data = new DashboardSummaryResponseDto
        {
            Meta = new ReportMetaDto
            {
                ReportTitle = "Financial Dashboard Summary",
                GeneratedAtUtc = DateTime.UtcNow,
                DateRange = new ReportDateRangeDto { FromUtc = summaryStart, ToUtc = summaryEnd },
                GroupBy = groupBy ?? "all-time",
                IncludeCancelled = query.IncludeCancelled,
                CustomerId = query.CustomerId,
                VendorId = null,
                CalculationNote = "Explicit from/to overrides range defaults. Without from/to: groupBy day/month/year maps to current day/month/year, otherwise summary uses all-time (earliest record to now). Estimated profit and low-stock risk are derived from existing sale/purchase/service/part fields."
            },
            TodaySales = todaySalesAmount,
            MonthSales = monthSalesAmount,
            MonthPurchases = monthPurchasesAmount,
            MonthPaymentsReceived = monthPaymentsAmount,
            PendingCreditAmount = pendingCreditAmount,
            EstimatedMonthProfit = estimatedMonthProfit,
            LowStockFinancialRisk = lowStockRisk,
            TopPendingReceivables = topPendingReceivables
        };

        return ApiResponse<DashboardSummaryResponseDto>.SuccessResponse(data, "Dashboard summary generated successfully.");
    }
    
    
    
    // Private Methods
    private static ApiResponse<ReportDateRangeDto> ValidateAndResolveRange(DateTime? from, DateTime? to)
    {
        var now = DateTime.UtcNow;
        var defaultFrom = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var resolvedFrom = (from ?? defaultFrom).ToUniversalTime();
        var rawTo = (to ?? now).ToUniversalTime();
        
        var resolvedFromUtc = resolvedFrom.Date;
        var resolvedToUtc = rawTo.TimeOfDay == TimeSpan.Zero
            ? rawTo.Date.AddDays(1).AddTicks(-1)
            : rawTo;

        if (resolvedFromUtc > resolvedToUtc)
        {
            return ApiResponse<ReportDateRangeDto>.FailureResponse(
                "Invalid date range: from cannot be greater than to.",
                new Dictionary<string, List<string>>
                {
                    ["dateRange"] = ["from must be less than or equal to to"]
                });
        }
        
        return ApiResponse<ReportDateRangeDto>.SuccessResponse(
            new ReportDateRangeDto { FromUtc = resolvedFromUtc, ToUtc = resolvedToUtc },
            "Date range validated.");
    }

    private async Task<ApiResponse<ReportDateRangeDto>> ValidateAndResolveSalesRevenueRangeAsync(DateTime? from, DateTime? to, string? groupBy)
    {
        var nowUtc = DateTime.UtcNow;
        var allTimeStart = await reportRepository.GetDashboardStartDateUtcAsync() ?? nowUtc;

        DateTime resolvedFrom;
        DateTime rawTo;

        if (from.HasValue || to.HasValue)
        {
            resolvedFrom = (from ?? allTimeStart).ToUniversalTime();
            rawTo = (to ?? nowUtc).ToUniversalTime();
        }
        else
        {
            if (groupBy is "day")
            {
                resolvedFrom = nowUtc.Date;
                rawTo = nowUtc;
            }
            else if (groupBy is "month")
            {
                resolvedFrom = new DateTime(nowUtc.Year, nowUtc.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                rawTo = nowUtc;
            }
            else if (groupBy is "year")
            {
                resolvedFrom = new DateTime(nowUtc.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                rawTo = nowUtc;
            }
            else
            {
                resolvedFrom = allTimeStart;
                rawTo = nowUtc;
            }
        }

        var resolvedFromUtc = resolvedFrom.Date;
        var resolvedToUtc = rawTo.TimeOfDay == TimeSpan.Zero
            ? rawTo.Date.AddDays(1).AddTicks(-1)
            : rawTo;

        if (resolvedFromUtc > resolvedToUtc)
        {
            return ApiResponse<ReportDateRangeDto>.FailureResponse(
                "Invalid date range: from cannot be greater than to.",
                new Dictionary<string, List<string>>
                {
                    ["dateRange"] = ["from must be less than or equal to to"]
                });
        }

        return ApiResponse<ReportDateRangeDto>.SuccessResponse(
            new ReportDateRangeDto { FromUtc = resolvedFromUtc, ToUtc = resolvedToUtc },
            "Date range validated.");
    }

    private async Task<ApiResponse<ReportDateRangeDto>> ValidateAndResolveSummaryRangeAsync(DateTime? from, DateTime? to, string? groupBy)
    {
        var nowUtc = DateTime.UtcNow;
        var allTimeStart = await reportRepository.GetDashboardStartDateUtcAsync() ?? nowUtc;
        DateTime resolvedFrom;
        DateTime rawTo;

        // Priority: explicit date filters win over groupBy-derived default ranges.
        if (from.HasValue || to.HasValue)
        {
            resolvedFrom = (from ?? allTimeStart).ToUniversalTime();
            rawTo = (to ?? nowUtc).ToUniversalTime();
        }
        else
        {
            if (groupBy is "day")
            {
                resolvedFrom = nowUtc.Date;
                rawTo = nowUtc;
            }
            else if (groupBy is "month")
            {
                resolvedFrom = new DateTime(nowUtc.Year, nowUtc.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                rawTo = nowUtc;
            }
            else if (groupBy is "year")
            {
                resolvedFrom = new DateTime(nowUtc.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                rawTo = nowUtc;
            }
            else
            {
                // /summary with no dates and no valid groupBy => all-time.
                resolvedFrom = allTimeStart;
                rawTo = nowUtc;
            }
        }

        var resolvedFromUtc = resolvedFrom.Date;
        var resolvedToUtc = rawTo.TimeOfDay == TimeSpan.Zero
            ? rawTo.Date.AddDays(1).AddTicks(-1)
            : rawTo;

        if (resolvedFromUtc > resolvedToUtc)
        {
            return ApiResponse<ReportDateRangeDto>.FailureResponse(
                "Invalid date range: from cannot be greater than to.",
                new Dictionary<string, List<string>>
                {
                    ["dateRange"] = ["from must be less than or equal to to"]
                });
        }

        return ApiResponse<ReportDateRangeDto>.SuccessResponse(
            new ReportDateRangeDto { FromUtc = resolvedFromUtc, ToUtc = resolvedToUtc },
            "Date range validated.");
    }
    
    
    private static string? NormalizeGroupBy(string? groupBy)
    {
        var value = (groupBy ?? "day").Trim().ToLowerInvariant();
        return value is "day" or "month" or "year" ? value : null;
    }

    
    private static string BuildPeriodKey(DateTime dateTimeUtc, string groupBy) =>
        groupBy switch
        {
            "year" => $"{dateTimeUtc:yyyy}",
            "month" => $"{dateTimeUtc:yyyy-MM}",
            _ => $"{dateTimeUtc:yyyy-MM-dd}"
        };

    
    private static decimal Round2(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);

    
    private static ReportMetaDto BuildMeta(
        string title,
        ReportDateRangeDto range,
        string groupBy,
        bool includeCancelled,
        string? customerId,
        int? vendorId,
        string calculationNote) =>
        new()
        {
            ReportTitle = title,
            GeneratedAtUtc = DateTime.UtcNow,
            DateRange = new ReportDateRangeDto { FromUtc = range.FromUtc, ToUtc = range.ToUtc },
            GroupBy = groupBy,
            IncludeCancelled = includeCancelled,
            CustomerId = customerId,
            VendorId = vendorId,
            CalculationNote = calculationNote
        };
    public async Task<ApiResponse<IEnumerable<RegularCustomerReportDto>>> GetRegularCustomersAsync()
    {
        var data = await reportRepository.GetRegularCustomersAsync();
        return ApiResponse<IEnumerable<RegularCustomerReportDto>>.SuccessResponse(data, "Regular customers report retrieved successfully");
    }

    public async Task<ApiResponse<IEnumerable<TopSpenderReportDto>>> GetTopSpendersAsync()
    {
        var data = await reportRepository.GetTopSpendersAsync();
        return ApiResponse<IEnumerable<TopSpenderReportDto>>.SuccessResponse(data, "Top spenders report retrieved successfully");
    }

    public async Task<ApiResponse<IEnumerable<PendingCreditReportDto>>> GetPendingCreditsAsync()
    {
        var data = await reportRepository.GetPendingCreditsAsync();
        return ApiResponse<IEnumerable<PendingCreditReportDto>>.SuccessResponse(data, "Pending credits report retrieved successfully");
    }
}
