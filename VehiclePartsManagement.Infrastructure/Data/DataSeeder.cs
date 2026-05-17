using Microsoft.EntityFrameworkCore;
using VehiclePartsManagement.Domain.Entities;
using VehiclePartsManagement.Domain.Enums;

namespace VehiclePartsManagement.Infrastructure.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        // Seed Brand
        var brand = await context.VehicleBrands.FirstOrDefaultAsync(b => b.BrandName == "Toyota");
        if (brand == null)
        {
            brand = new VehicleBrand { BrandName = "Toyota" };
            context.VehicleBrands.Add(brand);
            await context.SaveChangesAsync();
        }

        // Seed Model
        var model = await context.VehicleModels.FirstOrDefaultAsync(m => m.ModelName == "Corolla" && m.VehicleBrandId == brand.VehicleBrandId);
        if (model == null)
        {
            model = new VehicleModel { ModelName = "Corolla", VehicleBrandId = brand.VehicleBrandId };
            context.VehicleModels.Add(model);
            await context.SaveChangesAsync();
        }

        // Get Users
        var customer = await context.Users.FirstOrDefaultAsync(u => u.Email == "awantikapokharel46@gmail.com");
        var staff = await context.Users.FirstOrDefaultAsync(u => u.Email == "staff@gmail.com");

        if (customer == null || staff == null) return;

        // Seed Vehicle
        var vehicle = await context.Vehicles.FirstOrDefaultAsync(v => v.VehicleNumber == "BA 1 PA 1234");
        if (vehicle == null)
        {
            vehicle = new Vehicle
            {
                ApplicationUserId = customer.Id,
                VehicleModelId = model.VehicleModelId,
                VehicleNumber = "BA 1 PA 1234",
                FuelType = FuelType.Petrol,
                Color = "White"
            };
            context.Vehicles.Add(vehicle);
            await context.SaveChangesAsync();
        }

        // Seed Part Category
        var category = await context.PartCategories.FirstOrDefaultAsync(c => c.CategoryName == "Engine");
        if (category == null)
        {
            category = new PartCategory { CategoryName = "Engine" };
            context.PartCategories.Add(category);
            await context.SaveChangesAsync();
        }

        // Seed Part
        var part = await context.Parts.FirstOrDefaultAsync(p => p.Sku == "OF-001");
        if (part == null)
        {
            part = new Part
            {
                PartName = "Oil Filter",
                Sku = "OF-001",
                PartCategoryId = category.PartCategoryId,
                SellingPrice = 500,
                CurrentStock = 100
            };
            context.Parts.Add(part);
            await context.SaveChangesAsync();
        }

        var brakePart = await context.Parts.FirstOrDefaultAsync(p => p.Sku == "BP-999");
        if (brakePart == null)
        {
            brakePart = new Part
            {
                PartName = "Brake Pads",
                Sku = "BP-999",
                PartCategoryId = category.PartCategoryId,
                SellingPrice = 1200,
                CurrentStock = 2,
                LowStockThreshold = 10,
                IsActive = true
            };
            context.Parts.Add(brakePart);
            await context.SaveChangesAsync();
        }

        var airFilterPart = await context.Parts.FirstOrDefaultAsync(p => p.Sku == "AF-210");
        if (airFilterPart == null)
        {
            airFilterPart = new Part
            {
                PartName = "Air Filter",
                Sku = "AF-210",
                PartCategoryId = category.PartCategoryId,
                SellingPrice = 850,
                CurrentStock = 50,
                LowStockThreshold = 8,
                IsActive = true
            };
            context.Parts.Add(airFilterPart);
            await context.SaveChangesAsync();
        }

        // Seed baseline sale
        var sale = await context.Sales.FirstOrDefaultAsync(s => s.SaleInvoiceNo == "INV-001");
        if (sale == null)
        {
            sale = new Sale
            {
                CustomerApplicationUserId = customer.Id,
                ProcessedByApplicationUserId = staff.Id,
                VehicleId = vehicle.VehicleId,
                SaleInvoiceNo = "INV-001",
                SaleDate = DateTime.UtcNow,
                SaleStatus = SaleStatus.Completed,
                PaymentStatus = PaymentStatus.Paid,
                SubTotal = 500,
                TotalAmount = 500,
                BalanceDue = 0
            };
            context.Sales.Add(sale);
            await context.SaveChangesAsync();

            // Seed Sale Item
            var saleItem = new SaleItem
            {
                SaleId = sale.SaleId,
                PartId = part.PartId,
                QuantitySold = 1,
                UnitPriceAtSale = 500,
                LineTotal = 500
            };
            context.SaleItems.Add(saleItem);
        }

        // Seed payment for baseline sale
        if (!await context.Payments.AnyAsync(p => p.SaleId == sale.SaleId))
        {
            context.Payments.Add(new Payment
            {
                SaleId = sale.SaleId,
                PaymentDate = sale.SaleDate,
                PaymentMethod = PaymentMethodType.Cash,
                PaymentAmount = 500,
                PaymentReference = "SEED-INV001",
                PaymentNote = "Seed payment for INV-001"
            });
        }

        // Seed Overdue Sale
        if (!await context.Sales.AnyAsync(s => s.SaleInvoiceNo == "INV-OVERDUE"))
        {
            var overdueSale = new Sale
            {
                CustomerApplicationUserId = customer.Id,
                ProcessedByApplicationUserId = staff.Id,
                VehicleId = vehicle.VehicleId,
                SaleInvoiceNo = "INV-OVERDUE",
                SaleDate = DateTime.UtcNow.AddMonths(-2),
                DueDate = DateTime.UtcNow.AddMonths(-1),
                SaleStatus = SaleStatus.Completed,
                PaymentStatus = PaymentStatus.PartiallyPaid,
                SubTotal = 1000,
                TotalAmount = 1000,
                BalanceDue = 400
            };
            context.Sales.Add(overdueSale);
        }

        // Rich history seed for /api/customers/me/* endpoints.
        var historySales = new[]
        {
            new
            {
                InvoiceNo = "INV-HIST-1001",
                SaleDate = DateTime.UtcNow.AddDays(-45),
                SubTotal = 2550m,
                Discount = 50m,
                Total = 2500m,
                BalanceDue = 0m,
                PaymentStatus = PaymentStatus.Paid,
                DueDate = (DateTime?)null,
                Items = new[] { (Part: part, Qty: 2, UnitPrice: 500m), (Part: airFilterPart, Qty: 1, UnitPrice: 1550m) },
                Payments = new[] { (Date: DateTime.UtcNow.AddDays(-45), Method: PaymentMethodType.Card, Amount: 2500m, Ref: "SEED-HIST-1001-1", Note: "Full card payment") }
            },
            new
            {
                InvoiceNo = "INV-HIST-1002",
                SaleDate = DateTime.UtcNow.AddDays(-24),
                SubTotal = 3600m,
                Discount = 100m,
                Total = 3500m,
                BalanceDue = 800m,
                PaymentStatus = PaymentStatus.PartiallyPaid,
                DueDate = (DateTime?)DateTime.UtcNow.AddDays(-3),
                Items = new[] { (Part: brakePart, Qty: 2, UnitPrice: 1200m), (Part: airFilterPart, Qty: 2, UnitPrice: 600m) },
                Payments = new[]
                {
                    (Date: DateTime.UtcNow.AddDays(-24), Method: PaymentMethodType.MobileWallet, Amount: 1500m, Ref: "SEED-HIST-1002-1", Note: "Wallet payment"),
                    (Date: DateTime.UtcNow.AddDays(-20), Method: PaymentMethodType.Cash, Amount: 1200m, Ref: "SEED-HIST-1002-2", Note: "Counter cash payment")
                }
            },
            new
            {
                InvoiceNo = "INV-HIST-1003",
                SaleDate = DateTime.UtcNow.AddDays(-8),
                SubTotal = 2200m,
                Discount = 0m,
                Total = 2200m,
                BalanceDue = 2200m,
                PaymentStatus = PaymentStatus.Unpaid,
                DueDate = (DateTime?)DateTime.UtcNow.AddDays(7),
                Items = new[] { (Part: brakePart, Qty: 1, UnitPrice: 1200m), (Part: part, Qty: 2, UnitPrice: 500m) },
                Payments = Array.Empty<(DateTime Date, PaymentMethodType Method, decimal Amount, string Ref, string Note)>()
            }
        };

        foreach (var historySale in historySales)
        {
            var existingSale = await context.Sales.FirstOrDefaultAsync(s => s.SaleInvoiceNo == historySale.InvoiceNo);
            if (existingSale == null)
            {
                existingSale = new Sale
                {
                    CustomerApplicationUserId = customer.Id,
                    ProcessedByApplicationUserId = staff.Id,
                    VehicleId = vehicle.VehicleId,
                    SaleInvoiceNo = historySale.InvoiceNo,
                    SaleDate = historySale.SaleDate,
                    DueDate = historySale.DueDate,
                    SaleStatus = SaleStatus.Completed,
                    PaymentStatus = historySale.PaymentStatus,
                    SubTotal = historySale.SubTotal,
                    LoyaltyDiscountPercent = 0,
                    DiscountAmount = historySale.Discount,
                    TotalAmount = historySale.Total,
                    BalanceDue = historySale.BalanceDue
                };
                context.Sales.Add(existingSale);
                await context.SaveChangesAsync();
            }

            if (!await context.SaleItems.AnyAsync(si => si.SaleId == existingSale.SaleId))
            {
                foreach (var (seedPart, qty, unitPrice) in historySale.Items)
                {
                    context.SaleItems.Add(new SaleItem
                    {
                        SaleId = existingSale.SaleId,
                        PartId = seedPart.PartId,
                        QuantitySold = qty,
                        UnitPriceAtSale = unitPrice,
                        LineTotal = qty * unitPrice
                    });
                }
            }

            foreach (var paymentSeed in historySale.Payments)
            {
                if (await context.Payments.AnyAsync(p => p.PaymentReference == paymentSeed.Ref))
                {
                    continue;
                }

                context.Payments.Add(new Payment
                {
                    SaleId = existingSale.SaleId,
                    PaymentDate = paymentSeed.Date,
                    PaymentMethod = paymentSeed.Method,
                    PaymentAmount = paymentSeed.Amount,
                    PaymentReference = paymentSeed.Ref,
                    PaymentNote = paymentSeed.Note
                });
            }
        }

        var historyAppointments = new[]
        {
            new
            {
                Note = "SEED-APT-HIST-1",
                Date = DateTime.UtcNow.AddDays(-36),
                Time = "10:00 AM",
                Status = ServiceAppointmentStatus.Completed,
                Type = ServiceAppointmentType.GeneralService
            },
            new
            {
                Note = "SEED-APT-HIST-2",
                Date = DateTime.UtcNow.AddDays(-12),
                Time = "02:30 PM",
                Status = ServiceAppointmentStatus.Completed,
                Type = ServiceAppointmentType.Repair
            },
            new
            {
                Note = "SEED-APT-HIST-3",
                Date = DateTime.UtcNow.AddDays(2),
                Time = "11:15 AM",
                Status = ServiceAppointmentStatus.Confirmed,
                Type = ServiceAppointmentType.Maintenance
            },
            new
            {
                Note = "SEED-APT-HIST-4",
                Date = DateTime.UtcNow.AddDays(9),
                Time = "09:45 AM",
                Status = ServiceAppointmentStatus.Pending,
                Type = ServiceAppointmentType.Inspection
            }
        };

        foreach (var seedAppointment in historyAppointments)
        {
            var existingAppointment = await context.ServiceAppointments.FirstOrDefaultAsync(sa =>
                sa.ApplicationUserId == customer.Id &&
                sa.AppointmentNote == seedAppointment.Note);

            if (existingAppointment == null)
            {
                existingAppointment = new ServiceAppointment
                {
                    ApplicationUserId = customer.Id,
                    VehicleId = vehicle.VehicleId,
                    AppointmentDate = seedAppointment.Date,
                    AppointmentTime = seedAppointment.Time,
                    AppointmentStatus = seedAppointment.Status,
                    AppointmentType = seedAppointment.Type,
                    AppointmentNote = seedAppointment.Note
                };
                context.ServiceAppointments.Add(existingAppointment);
                await context.SaveChangesAsync();
            }

            if (seedAppointment.Status != ServiceAppointmentStatus.Completed)
            {
                continue;
            }

            var hasRecord = await context.ServiceRecords.AnyAsync(sr => sr.ServiceAppointmentId == existingAppointment.ServiceAppointmentId);
            if (hasRecord)
            {
                continue;
            }

            context.ServiceRecords.Add(new ServiceRecord
            {
                ServiceAppointmentId = existingAppointment.ServiceAppointmentId,
                VehicleId = vehicle.VehicleId,
                ServiceDate = seedAppointment.Date,
                ServiceType = seedAppointment.Type.ToString(),
                Diagnosis = seedAppointment.Type == ServiceAppointmentType.Repair
                    ? "Front brake wear and reduced stopping confidence."
                    : "Routine service check required.",
                WorkDone = seedAppointment.Type == ServiceAppointmentType.Repair
                    ? "Replaced front brake pads and test-drove vehicle."
                    : "Engine oil change, oil filter replacement, general inspection.",
                MileageAtService = seedAppointment.Type == ServiceAppointmentType.Repair ? 47200 : 43850,
                LaborCharge = seedAppointment.Type == ServiceAppointmentType.Repair ? 1800m : 1200m,
                NextServiceDueDate = seedAppointment.Date.AddMonths(3)
            });
        }

        await context.SaveChangesAsync();
    }
}
