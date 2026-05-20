using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VehiclePartsManagement.Application.Common;
using VehiclePartsManagement.Application.DTOs.Payment;
using VehiclePartsManagement.Application.DTOs.Sale;
using VehiclePartsManagement.Application.DTOs.Sales;
using VehiclePartsManagement.Application.Interfaces.Repositories;
using VehiclePartsManagement.Application.Interfaces.Services;
using VehiclePartsManagement.Application.Services.EmailTemplates;
using VehiclePartsManagement.Domain.Entities;
using VehiclePartsManagement.Domain.Enums;

using Microsoft.Extensions.Options;
using VehiclePartsManagement.Application.Config;

namespace VehiclePartsManagement.Application.Services;

public class SaleService(
    ISaleRepository saleRepository,
    IEmailService emailService,
    IPaymentRepository paymentRepository,
    IPartRepository partRepository,
    IVehicleRepository vehicleRepository,
    IStockMovementRepository stockMovementRepository,
    UserManager<ApplicationUser> userManager,
    IPaymentService paymentService,
    IOptions<KhaltiOptions> khaltiOptions) : ISaleService
{
    private readonly KhaltiOptions _khaltiOptions = khaltiOptions.Value;

    public async Task<ApiResponse<IEnumerable<SaleSummaryDto>>> GetAllSalesAsync()
    {
        var sales = await saleRepository.GetAllSalesAsync();
        var data = sales.Select(s => new SaleSummaryDto
        {
            SaleId = s.SaleId,
            InvoiceNo = s.SaleInvoiceNo,
            CustomerName = s.CustomerUser?.FullName ?? string.Empty,
            CustomerEmail = s.CustomerUser?.Email ?? string.Empty,
            TotalAmount = s.TotalAmount,
            BalanceDue = s.BalanceDue,
            SaleDate = s.SaleDate,
            PaymentStatus = s.PaymentStatus.ToString()
        });
        return ApiResponse<IEnumerable<SaleSummaryDto>>.SuccessResponse(data, "Sales retrieved successfully");
    }

    public async Task<ApiResponse> SendInvoiceEmailAsync(int saleId)
    {
        var sale = await saleRepository.GetSaleWithDetailsAsync(saleId);
        if (sale == null)
            return ApiResponse.FailureResponse("Sale not found");

        if (string.IsNullOrEmpty(sale.CustomerUser?.Email))
            return ApiResponse.FailureResponse("Customer email not found");

        string? paymentUrl = null;
        if (sale.BalanceDue > 0)
        {
            // Direct endpoint that dynamically initiates Khalti checkout and redirects the user
            paymentUrl = _khaltiOptions.ReturnUrl.Replace("/khalti/callback", $"/khalti/pay/{sale.SaleId}");
        }

        var subject = $"Invoice for Sale #{sale.SaleInvoiceNo}";
        var body = GenerateInvoiceEmailBody(sale, paymentUrl);

        var sent = await emailService.SendEmailAsync(sale.CustomerUser.Email, subject, body);

        return sent 
            ? ApiResponse.SuccessResponse("Invoice email sent successfully")
            : ApiResponse.FailureResponse("Failed to send invoice email");
    }

    private string GenerateInvoiceEmailBody(Domain.Entities.Sale sale, string? paymentUrl)
    {
        var paidAmount = sale.Payments?.Sum(p => p.PaymentAmount) ?? 0m;
        
        var utcSaleDate = sale.SaleDate.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(sale.SaleDate, DateTimeKind.Utc)
            : sale.SaleDate.ToUniversalTime();

        TimeZoneInfo nepalZone;
        try
        {
            nepalZone = TimeZoneInfo.FindSystemTimeZoneById("Nepal Standard Time");
        }
        catch (TimeZoneNotFoundException)
        {
            nepalZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Kathmandu");
        }
        var nepalSaleDate = TimeZoneInfo.ConvertTime(utcSaleDate, nepalZone);
        
        var statusText = sale.PaymentStatus.ToString();
        var statusBg = "#f3f4f6";
        var statusColor = "#374151";
        
        if (sale.PaymentStatus == PaymentStatus.Paid)
        {
            statusBg = "#dcfce7";
            statusColor = "#15803d";
        }
        else if (sale.PaymentStatus == PaymentStatus.Overdue)
        {
            statusBg = "#fee2e2";
            statusColor = "#b91c1c";
        }
        else if (sale.PaymentStatus == PaymentStatus.PartiallyPaid)
        {
            statusText = "Partial";
            statusBg = "#f3f4f6";
            statusColor = "#374151";
        }

        var dueDateText = sale.DueDate.HasValue 
            ? sale.DueDate.Value.ToString("MMMM dd, yyyy") 
            : "-";

        var sb = new StringBuilder();
        
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html>");
        sb.AppendLine("<head>");
        sb.AppendLine("  <meta charset='utf-8'>");
        sb.AppendLine("  <meta name='viewport' content='width=device-width, initial-scale=1.0'>");
        sb.AppendLine("  <title>Sales Invoice</title>");
        sb.AppendLine("  <style>");
        sb.AppendLine("    @import url('https://fonts.googleapis.com/css2?family=Outfit:wght@400;600;700;800&family=Plus+Jakarta+Sans:wght@400;500;600;700&display=swap');");
        sb.AppendLine("    body { margin: 0; padding: 0; background-color: #ffffff; font-family: 'Plus Jakarta Sans', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif; -webkit-font-smoothing: antialiased; }");
        sb.AppendLine("    table { border-collapse: collapse; width: 100%; }");
        sb.AppendLine("  </style>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");
        sb.AppendLine("  <table border='0' cellpadding='0' cellspacing='0' width='100%' style='background-color: #ffffff;'>");
        sb.AppendLine("    <tr>");
        sb.AppendLine("      <td align='center' style='padding: 24px 16px;'>");
        sb.AppendLine("        <table border='0' cellpadding='0' cellspacing='0' width='100%' style='max-width: 680px; text-align: left;'>");
        
        // --- HEADER SECTION ---
        sb.AppendLine("          <tr>");
        sb.AppendLine("            <td>");
        sb.AppendLine("              <table border='0' cellpadding='0' cellspacing='0' width='100%'>");
        sb.AppendLine("                <tr>");
        // Left Column (Torquex & Title)
        sb.AppendLine("                  <td valign='top' style='padding-bottom: 24px;'>");
        sb.AppendLine("                    <div style=\"font-family: 'Outfit', sans-serif; font-size: 11px; font-weight: 800; color: #2563eb; letter-spacing: 0.15em; text-transform: uppercase; margin-bottom: 8px;\">TORQUEX</div>");
        sb.AppendLine("                    <h1 style=\"font-family: 'Outfit', sans-serif; font-size: 28px; font-weight: 700; color: #111827; margin: 0 0 4px 0; line-height: 1.1;\">Sales Invoice</h1>");
        sb.AppendLine("                    <p style='font-size: 13px; color: #6b7280; margin: 0;'>Premium vehicle parts and service billing</p>");
        sb.AppendLine("                  </td>");
        // Right Column (Invoice Number Info Card)
        sb.AppendLine("                  <td valign='top' align='right' style='padding-bottom: 24px; width: 240px;'>");
        sb.AppendLine("                    <table border='0' cellpadding='0' cellspacing='0' style='background-color: #f3f4f6; border-radius: 12px; width: 100%;'>");
        sb.AppendLine("                      <tr>");
        sb.AppendLine("                        <td style='padding: 16px 20px; text-align: left;'>");
        sb.AppendLine("                          <div style='font-size: 9px; font-weight: 700; color: #9ca3af; letter-spacing: 0.05em; text-transform: uppercase; margin-bottom: 2px;'>INVOICE NUMBER</div>");
        sb.AppendLine($"                          <div style=\"font-family: 'Outfit', sans-serif; font-size: 13px; font-weight: 700; color: #111827; margin-bottom: 12px; word-break: break-all;\">{sale.SaleInvoiceNo}</div>");
        sb.AppendLine("                          <div style='font-size: 9px; font-weight: 700; color: #9ca3af; letter-spacing: 0.05em; text-transform: uppercase; margin-bottom: 2px;'>ISSUE DATE</div>");
        sb.AppendLine($"                          <div style=\"font-family: 'Outfit', sans-serif; font-size: 13px; font-weight: 700; color: #111827;\">{nepalSaleDate:MMMM dd, yyyy}</div>");
        sb.AppendLine("                        </td>");
        sb.AppendLine("                      </tr>");
        sb.AppendLine("                    </table>");
        sb.AppendLine("                  </td>");
        sb.AppendLine("                </tr>");
        sb.AppendLine("              </table>");
        sb.AppendLine("            </td>");
        sb.AppendLine("          </tr>");
        
        // --- PAYMENT STATUS SECTION ---
        sb.AppendLine("          <tr>");
        sb.AppendLine("            <td style='padding-bottom: 16px;'>");
        sb.AppendLine("              <table border='0' cellpadding='0' cellspacing='0'>");
        sb.AppendLine("                <tr>");
        sb.AppendLine("                  <td valign='middle' style=\"font-size: 9px; font-weight: 700; color: #9ca3af; letter-spacing: 0.05em; text-transform: uppercase; padding-right: 12px;\">PAYMENT STATUS</td>");
        sb.AppendLine($"                  <td valign='middle' style='background-color: {statusBg}; color: {statusColor}; border-radius: 9999px; padding: 4px 12px; font-size: 11px; font-weight: 600; display: inline-block; margin-right: 8px;'>{statusText}</td>");
        sb.AppendLine($"                  <td valign='middle' style='background-color: #f3f4f6; color: #374151; border-radius: 9999px; padding: 4px 12px; font-size: 11px; font-weight: 600; display: inline-block;'>Due: {dueDateText}</td>");
        sb.AppendLine("                </tr>");
        sb.AppendLine("              </table>");
        sb.AppendLine("            </td>");
        sb.AppendLine("          </tr>");
        
        // --- DIVIDER LINE ---
        sb.AppendLine("          <tr>");
        sb.AppendLine("            <td>");
        sb.AppendLine("              <div style='border-top: 1px solid #f3f4f6; margin: 16px 0 24px 0;'></div>");
        sb.AppendLine("            </td>");
        sb.AppendLine("          </tr>");
        
        // --- CUSTOMER & VEHICLE AND TOTALS SECTION ---
        sb.AppendLine("          <tr>");
        sb.AppendLine("            <td style='padding-bottom: 32px;'>");
        sb.AppendLine("              <table border='0' cellpadding='0' cellspacing='0' width='100%'>");
        sb.AppendLine("                <tr>");
        // Left Column: Customer & Vehicle (56% width)
        sb.AppendLine("                  <td valign='top' style='width: 56%; padding-right: 24px;'>");
        sb.AppendLine("                    <h2 style=\"font-family: 'Outfit', sans-serif; font-size: 15px; font-weight: 700; color: #111827; margin: 0 0 16px 0;\">Customer & Vehicle</h2>");
        sb.AppendLine("                    <table border='0' cellpadding='0' cellspacing='0' width='100%'>");
        sb.AppendLine("                      <tr>");
        // Customer detail block
        sb.AppendLine("                        <td valign='top' style='width: 50%; padding-right: 12px;'>");
        sb.AppendLine("                          <div style='font-size: 9px; font-weight: 700; color: #9ca3af; letter-spacing: 0.05em; text-transform: uppercase; margin-bottom: 4px;'>CUSTOMER</div>");
        sb.AppendLine($"                          <div style='font-size: 13px; font-weight: 700; color: #111827; margin-bottom: 2px;'>{sale.CustomerUser?.FullName}</div>");
        sb.AppendLine($"                          <div style='font-size: 12px; color: #6b7280; margin-bottom: 1px; word-break: break-all;'>{sale.CustomerUser?.Email}</div>");
        sb.AppendLine($"                          <div style='font-size: 12px; color: #6b7280;'>{sale.CustomerUser?.PhoneNumber}</div>");
        sb.AppendLine("                        </td>");
        // Vehicle detail block
        sb.AppendLine("                        <td valign='top' style='width: 50%;'>");
        sb.AppendLine("                          <div style='font-size: 9px; font-weight: 700; color: #9ca3af; letter-spacing: 0.05em; text-transform: uppercase; margin-bottom: 4px;'>VEHICLE</div>");
        sb.AppendLine($"                          <div style='font-size: 13px; font-weight: 700; color: #111827; margin-bottom: 2px;'>{sale.Vehicle?.VehicleNumber}</div>");
        sb.AppendLine($"                          <div style='font-size: 12px; color: #6b7280;'>{sale.Vehicle?.Model?.Brand?.BrandName} {sale.Vehicle?.Model?.ModelName}</div>");
        sb.AppendLine("                        </td>");
        sb.AppendLine("                      </tr>");
        sb.AppendLine("                    </table>");
        sb.AppendLine("                  </td>");
        
        // Right Column: Totals Card (44% width)
        sb.AppendLine("                  <td valign='top' style='width: 44%;'>");
        sb.AppendLine("                    <table border='0' cellpadding='0' cellspacing='0' style='background-color: #f9fafb; border-radius: 16px; border: 1px solid #f3f4f6; width: 100%;'>");
        sb.AppendLine("                      <tr>");
        sb.AppendLine("                        <td style='padding: 20px;'>");
        sb.AppendLine("                          <div style='font-size: 14px; font-weight: 700; color: #111827; margin-bottom: 14px;'>Totals</div>");
        sb.AppendLine("                          <table border='0' cellpadding='0' cellspacing='0' width='100%'>");
        
        // Sub total
        sb.AppendLine("                            <tr>");
        sb.AppendLine("                              <td style='font-size: 12px; color: #6b7280; padding: 4px 0;'>Sub total</td>");
        sb.AppendLine($"                              <td align='right' style='font-size: 13px; font-weight: 600; color: #111827; padding: 4px 0;'>Rs. {sale.SubTotal:N0}</td>");
        sb.AppendLine("                            </tr>");
        
        // Loyalty discount
        sb.AppendLine("                            <tr>");
        sb.AppendLine($"                              <td style='font-size: 12px; color: #6b7280; padding: 4px 0;'>Loyalty discount ({sale.LoyaltyDiscountPercent:0}%)</td>");
        sb.AppendLine($"                              <td align='right' style='font-size: 13px; font-weight: 600; color: #111827; padding: 4px 0;'>- Rs. {sale.DiscountAmount:N0}</td>");
        sb.AppendLine("                            </tr>");
        
        // Divider
        sb.AppendLine("                            <tr>");
        sb.AppendLine("                              <td colspan='2' style='padding: 8px 0;'>");
        sb.AppendLine("                                <div style='border-top: 1px solid #f3f4f6;'></div>");
        sb.AppendLine("                              </td>");
        sb.AppendLine("                            </tr>");
        
        // Total
        sb.AppendLine("                            <tr>");
        sb.AppendLine("                              <td style='font-size: 13px; font-weight: 600; color: #111827; padding: 4px 0;'>Total</td>");
        sb.AppendLine($"                              <td align='right' style=\"font-family: 'Outfit', sans-serif; font-size: 15px; font-weight: 700; color: #111827; padding: 4px 0;\">Rs. {sale.TotalAmount:N0}</td>");
        sb.AppendLine("                            </tr>");
        
        // Paid
        sb.AppendLine("                            <tr>");
        sb.AppendLine("                              <td style='font-size: 12px; color: #6b7280; padding: 4px 0;'>Paid</td>");
        sb.AppendLine($"                              <td align='right' style='font-size: 13px; font-weight: 600; color: #111827; padding: 4px 0;'>Rs. {paidAmount:N0}</td>");
        sb.AppendLine("                            </tr>");
        
        // Balance due
        sb.AppendLine("                            <tr>");
        sb.AppendLine("                              <td style='font-size: 13px; font-weight: 600; color: #111827; padding: 4px 0;'>Balance due</td>");
        sb.AppendLine($"                              <td align='right' style=\"font-family: 'Outfit', sans-serif; font-size: 15px; font-weight: 700; color: #111827; padding: 4px 0;\">Rs. {sale.BalanceDue:N0}</td>");
        sb.AppendLine("                            </tr>");
        
        sb.AppendLine("                          </table>");
        sb.AppendLine("                        </td>");
        sb.AppendLine("                      </tr>");
        sb.AppendLine("                    </table>");
        sb.AppendLine("                  </td>");
        sb.AppendLine("                </tr>");
        sb.AppendLine("              </table>");
        sb.AppendLine("            </td>");
        sb.AppendLine("          </tr>");
        
        // --- ITEMS SECTION ---
        sb.AppendLine("          <tr>");
        sb.AppendLine("            <td style='padding-bottom: 32px;'>");
        sb.AppendLine("              <h2 style=\"font-family: 'Outfit', sans-serif; font-size: 15px; font-weight: 700; color: #111827; margin: 0 0 16px 0;\">Items</h2>");
        sb.AppendLine("              <table border='0' cellpadding='0' cellspacing='0' width='100%'>");
        sb.AppendLine("                <thead>");
        sb.AppendLine("                  <tr>");
        sb.AppendLine("                    <th style='text-align: left; font-size: 9px; font-weight: 700; color: #9ca3af; letter-spacing: 0.05em; text-transform: uppercase; padding: 10px 12px; border-bottom: 1px solid #f3f4f6;'>PART</th>");
        sb.AppendLine("                    <th style='text-align: center; font-size: 9px; font-weight: 700; color: #9ca3af; letter-spacing: 0.05em; text-transform: uppercase; padding: 10px 12px; border-bottom: 1px solid #f3f4f6;'>QUANTITY</th>");
        sb.AppendLine("                    <th style='text-align: right; font-size: 9px; font-weight: 700; color: #9ca3af; letter-spacing: 0.05em; text-transform: uppercase; padding: 10px 12px; border-bottom: 1px solid #f3f4f6;'>UNIT PRICE</th>");
        sb.AppendLine("                    <th style='text-align: right; font-size: 9px; font-weight: 700; color: #9ca3af; letter-spacing: 0.05em; text-transform: uppercase; padding: 10px 12px; border-bottom: 1px solid #f3f4f6;'>LINE TOTAL</th>");
        sb.AppendLine("                  </tr>");
        sb.AppendLine("                </thead>");
        sb.AppendLine("                <tbody>");
        
        foreach (var item in sale.SaleItems)
        {
            sb.AppendLine("                  <tr>");
            sb.AppendLine($"                    <td style='text-align: left; font-size: 13px; font-weight: 600; color: #111827; padding: 14px 12px; border-bottom: 1px solid #f3f4f6;'>{item.Part?.PartName}</td>");
            sb.AppendLine($"                    <td align='center' style='font-size: 13px; color: #4b5563; padding: 14px 12px; border-bottom: 1px solid #f3f4f6;'>{item.QuantitySold}</td>");
            sb.AppendLine($"                    <td align='right' style='font-size: 13px; color: #4b5563; padding: 14px 12px; border-bottom: 1px solid #f3f4f6;'>Rs. {item.UnitPriceAtSale:N0}</td>");
            sb.AppendLine($"                    <td align='right' style='font-size: 13px; font-weight: 600; color: #111827; padding: 14px 12px; border-bottom: 1px solid #f3f4f6;'>Rs. {item.LineTotal:N0}</td>");
            sb.AppendLine("                  </tr>");
        }
        
        sb.AppendLine("                </tbody>");
        sb.AppendLine("              </table>");
        sb.AppendLine("            </td>");
        sb.AppendLine("          </tr>");
        
        if (!string.IsNullOrEmpty(paymentUrl))
        {
            sb.AppendLine("          <!-- --- PAYMENT CTA SECTION --- -->");
            sb.AppendLine("          <tr>");
            sb.AppendLine("            <td style='padding-bottom: 32px;'>");
            sb.AppendLine("              <table border='0' cellpadding='0' cellspacing='0' width='100%' style='background: linear-gradient(135deg, #eff6ff 0%, #dbeafe 100%); border-radius: 20px; border: 1px solid #bfdbfe;'>");
            sb.AppendLine("                <tr>");
            sb.AppendLine("                  <td style='padding: 24px; text-align: center;'>");
            sb.AppendLine("                    <h3 style=\"font-family: 'Outfit', sans-serif; font-size: 16px; font-weight: 700; color: #1e3a8a; margin: 0 0 6px 0;\">Outstanding Balance Due</h3>");
            sb.AppendLine($"                    <p style='font-size: 13px; color: #1e40af; margin: 0 0 16px 0;'>Settle your invoice balance of <strong>Rs. {sale.BalanceDue:N0}</strong> securely online via Khalti.</p>");
            sb.AppendLine("                    <table border='0' cellpadding='0' cellspacing='0' style='margin: 0 auto;'>");
            sb.AppendLine("                      <tr>");
            sb.AppendLine($"                        <td align='center' style='border-radius: 9999px; background-color: #2563eb;'>");
            sb.AppendLine($"                          <a href='{paymentUrl}' target='_blank' style=\"font-family: 'Outfit', sans-serif; font-size: 14px; font-weight: 700; color: #ffffff; text-decoration: none; padding: 12px 32px; display: inline-block; letter-spacing: 0.05em; text-transform: uppercase;\">Pay Invoice Securely</a>");
            sb.AppendLine("                        </td>");
            sb.AppendLine("                      </tr>");
            sb.AppendLine("                    </table>");
            sb.AppendLine("                  </td>");
            sb.AppendLine("                </tr>");
            sb.AppendLine("              </table>");
            sb.AppendLine("            </td>");
            sb.AppendLine("          </tr>");
        }
        
        // --- FOOTER NOTE / GREETING ---
        sb.AppendLine("          <tr>");
        sb.AppendLine("            <td style='padding-top: 16px; border-top: 1px solid #f3f4f6;'>");
        sb.AppendLine("              <p style='font-size: 13px; color: #6b7280; margin: 0 0 4px 0;'>If you have any questions about this invoice, please reach out to our support.</p>");
        sb.AppendLine("              <p style='font-size: 13px; font-weight: 600; color: #111827; margin: 0;'>Thank you for your business!</p>");
        sb.AppendLine("            </td>");
        sb.AppendLine("          </tr>");
        
        sb.AppendLine("        </table>");
        sb.AppendLine("      </td>");
        sb.AppendLine("    </tr>");
        sb.AppendLine("  </table>");
        sb.AppendLine("</body>");
        sb.AppendLine("</html>");

        return sb.ToString();
    }
    private const decimal LoyaltyDiscountPercent = 10m;
    private const decimal LoyaltyThresholdAmount = 5000m;

    /// <summary>
    /// Creates a sale, decrements inventory stock, and optionally records initial payment.
    /// </summary>
    public async Task<ApiResponse<SaleCreateResponseDto>> CreateAsync(
        CreateSaleDto dto,
        string processedByApplicationUserId)
    {
        // Customer must exist and belong to Customer role for sale ownership integrity.
        var customer = await userManager.FindByIdAsync(dto.CustomerUserId);
        if (customer is null || !await userManager.IsInRoleAsync(customer, "Customer"))
        {
            return ApiResponse<SaleCreateResponseDto>.FailureResponse(
                "Customer not found.",
                new Dictionary<string, List<string>>
                {
                    { "customerUserId", ["Customer not found."] }
                });
        }

        // Vehicle must exist and belong to the same selected customer.
        var vehicle = await vehicleRepository.GetByIdAsync(dto.VehicleId);
        if (vehicle is null)
        {
            return ApiResponse<SaleCreateResponseDto>.FailureResponse(
                "Vehicle not found.",
                new Dictionary<string, List<string>>
                {
                    { "vehicleId", ["Vehicle not found."] }
                });
        }

        if (vehicle.ApplicationUserId != dto.CustomerUserId)
        {
            return ApiResponse<SaleCreateResponseDto>.FailureResponse(
                "Selected vehicle does not belong to the selected customer.",
                new Dictionary<string, List<string>>
                {
                    { "vehicleId", ["Selected vehicle does not belong to the selected customer."] }
                });
        }

        var lineValidation = await ValidateAndBuildSaleItemsAsync(dto.Items);
        if (!lineValidation.Success)
        {
            return ApiResponse<SaleCreateResponseDto>.FailureResponse(
                lineValidation.Message,
                lineValidation.Errors);
        }

        var saleItems = lineValidation.SaleItems;
        var partsById = lineValidation.PartsById;

        // Loyalty discount rule: 10% only when subtotal is strictly greater than threshold.
        var subTotal = saleItems.Sum(i => i.LineTotal);
        var discountPercent = subTotal > LoyaltyThresholdAmount ? LoyaltyDiscountPercent : 0m;
        var discountAmount = Math.Round((subTotal * discountPercent) / 100m, 2, MidpointRounding.AwayFromZero);
        var totalAmount = subTotal - discountAmount;

        // Credit sale and due-date combination is validated to prevent inconsistent states.
        if (dto.IsCreditSale && !dto.DueDate.HasValue)
        {
            return ApiResponse<SaleCreateResponseDto>.FailureResponse(
                "Due date is required for credit sales.",
                new Dictionary<string, List<string>>
                {
                    { "dueDate", ["Due date is required for credit sales."] }
                });
        }

        if (!dto.IsCreditSale && dto.DueDate.HasValue)
        {
            return ApiResponse<SaleCreateResponseDto>.FailureResponse(
                "Due date is only allowed for credit sales.",
                new Dictionary<string, List<string>>
                {
                    { "dueDate", ["Due date is only allowed for credit sales."] }
                });
        }

        // Payment amount validation enforces no-negative and no-overpayment rules.
        var paymentAmount = dto.Payment?.PaymentAmount ?? 0m;
        if (paymentAmount < 0m)
        {
            return ApiResponse<SaleCreateResponseDto>.FailureResponse(
                "Payment amount cannot be negative.",
                new Dictionary<string, List<string>>
                {
                    { "payment.paymentAmount", ["Payment amount cannot be negative."] }
                });
        }

        if (paymentAmount > totalAmount)
        {
            return ApiResponse<SaleCreateResponseDto>.FailureResponse(
                "Payment amount cannot exceed total amount.",
                new Dictionary<string, List<string>>
                {
                    { "payment.paymentAmount", ["Payment amount cannot exceed total amount."] }
                });
        }

        // Optional payment method is parsed only when payment payload is provided.
        PaymentMethodType? paymentMethod = null;
        if (dto.Payment is not null)
        {
            if (!Enum.TryParse<PaymentMethodType>(dto.Payment.PaymentMethod, true, out var parsedMethod))
            {
                return ApiResponse<SaleCreateResponseDto>.FailureResponse(
                    "Payment method is invalid.",
                    new Dictionary<string, List<string>>
                    {
                        { "payment.paymentMethod", ["Payment method is invalid."] }
                    });
            }

            paymentMethod = parsedMethod;
        }

        var balanceDue = totalAmount - paymentAmount;
        var paymentStatus = ResolvePaymentStatus(balanceDue, dto.DueDate);

        var sale = new Sale
        {
            CustomerApplicationUserId = dto.CustomerUserId,
            VehicleId = dto.VehicleId,
            ProcessedByApplicationUserId = processedByApplicationUserId,
            SaleChannel = SaleChannel.Workshop,
            SaleInvoiceNo = $"SAL-{DateTime.UtcNow:yyyyMMddHHmmssfff}",
            SaleDate = DateTime.UtcNow,
            SaleStatus = SaleStatus.Completed,
            SubTotal = subTotal,
            LoyaltyDiscountPercent = discountPercent,
            DiscountAmount = discountAmount,
            TotalAmount = totalAmount,
            BalanceDue = balanceDue,
            PaymentStatus = paymentStatus,
            DueDate = dto.DueDate.HasValue
                ? DateTime.SpecifyKind(dto.DueDate.Value.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc)
                : null,
            SaleItems = saleItems
        };

        // Persist sale first to obtain SaleId for payment and stock movement references.
        saleRepository.Create(sale);
        await saleRepository.SaveChangesAsync();

        // Initial payment is optional and recorded only when amount is positive.
        if (dto.Payment is not null && paymentAmount > 0m && paymentMethod.HasValue)
        {
            var payment = new Payment
            {
                SaleId = sale.SaleId,
                PaymentDate = DateTime.UtcNow,
                PaymentMethod = paymentMethod.Value,
                PaymentAmount = paymentAmount,
                PaymentReference = dto.Payment.PaymentReference.Trim(),
                PaymentNote = dto.Payment.PaymentNote.Trim()
            };

            paymentRepository.Create(payment);
        }

        // Inventory is decremented per line and each decrement is audited in StockMovement.
        foreach (var line in dto.Items)
        {
            var part = partsById[line.PartId];
            part.CurrentStock -= line.Quantity;
            partRepository.Update(part);

            stockMovementRepository.Create(new StockMovement
            {
                PartId = part.PartId,
                MovementType = StockMovementType.Out,
                MovementQuantity = line.Quantity,
                MovementDate = DateTime.UtcNow,
                ReferenceType = StockMovementReferenceType.Sale,
                ReferenceId = sale.SaleId
            });
        }

        // Final save persists payment (if any), stock updates, and stock movements.
        await saleRepository.SaveChangesAsync();

        // Automatically trigger secure Khalti payment link generation and send invoice email
        // if payment method is Mobile Wallet. We do this asynchronously to keep the UI highly responsive.
        if (paymentMethod == PaymentMethodType.MobileWallet)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await SendInvoiceEmailAsync(sale.SaleId);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Auto Email Error] Failed to send automatic invoice email for sale {sale.SaleId}: {ex.Message}");
                }
            });
        }

        return ApiResponse<SaleCreateResponseDto>.SuccessResponse(
            new SaleCreateResponseDto
            {
                SaleId = sale.SaleId,
                SaleInvoiceNo = sale.SaleInvoiceNo,
                SubTotal = sale.SubTotal,
                LoyaltyDiscountPercent = sale.LoyaltyDiscountPercent,
                DiscountAmount = sale.DiscountAmount,
                TotalAmount = sale.TotalAmount,
                PaidAmount = paymentAmount,
                BalanceDue = sale.BalanceDue,
                PaymentStatus = MapPaymentStatus(sale.PaymentStatus)
            },
            "Sale invoice created successfully");
    }

    public async Task<ApiResponse<SaleCreateResponseDto>> CreateOnlineAsync(
        CreateOnlineSaleDto dto,
        string customerUserId)
    {
        var customer = await userManager.FindByIdAsync(customerUserId);
        if (customer is null || !await userManager.IsInRoleAsync(customer, "Customer"))
        {
            return ApiResponse<SaleCreateResponseDto>.FailureResponse(
                "Customer not found.",
                new Dictionary<string, List<string>>
                {
                    { "customerUserId", ["Customer not found."] }
                });
        }

        var lineValidation = await ValidateAndBuildSaleItemsAsync(dto.Items);
        if (!lineValidation.Success)
        {
            return ApiResponse<SaleCreateResponseDto>.FailureResponse(
                lineValidation.Message,
                lineValidation.Errors);
        }

        var saleItems = lineValidation.SaleItems;
        var partsById = lineValidation.PartsById;

        var subTotal = saleItems.Sum(i => i.LineTotal);
        var discountPercent = subTotal > LoyaltyThresholdAmount ? LoyaltyDiscountPercent : 0m;
        var discountAmount = Math.Round((subTotal * discountPercent) / 100m, 2, MidpointRounding.AwayFromZero);
        var totalAmount = subTotal - discountAmount;

        var sale = new Sale
        {
            CustomerApplicationUserId = customerUserId,
            VehicleId = null,
            ProcessedByApplicationUserId = null,
            SaleChannel = SaleChannel.OnlineParts,
            SaleInvoiceNo = $"SAL-{DateTime.UtcNow:yyyyMMddHHmmssfff}",
            SaleDate = DateTime.UtcNow,
            SaleStatus = SaleStatus.Completed,
            SubTotal = subTotal,
            LoyaltyDiscountPercent = discountPercent,
            DiscountAmount = discountAmount,
            TotalAmount = totalAmount,
            BalanceDue = totalAmount,
            PaymentStatus = PaymentStatus.Unpaid,
            DueDate = null,
            SaleItems = saleItems
        };

        saleRepository.Create(sale);
        await saleRepository.SaveChangesAsync();

        foreach (var line in dto.Items)
        {
            var part = partsById[line.PartId];
            part.CurrentStock -= line.Quantity;
            partRepository.Update(part);

            stockMovementRepository.Create(new StockMovement
            {
                PartId = part.PartId,
                MovementType = StockMovementType.Out,
                MovementQuantity = line.Quantity,
                MovementDate = DateTime.UtcNow,
                ReferenceType = StockMovementReferenceType.Sale,
                ReferenceId = sale.SaleId
            });
        }

        await saleRepository.SaveChangesAsync();

        return ApiResponse<SaleCreateResponseDto>.SuccessResponse(
            new SaleCreateResponseDto
            {
                SaleId = sale.SaleId,
                SaleInvoiceNo = sale.SaleInvoiceNo,
                SubTotal = sale.SubTotal,
                LoyaltyDiscountPercent = sale.LoyaltyDiscountPercent,
                DiscountAmount = sale.DiscountAmount,
                TotalAmount = sale.TotalAmount,
                PaidAmount = 0m,
                BalanceDue = sale.BalanceDue,
                PaymentStatus = MapPaymentStatus(sale.PaymentStatus)
            },
            "Online part order created successfully");
    }


    /// <summary>
    /// Retrieves paged sales with optional customer and payment-status filters.
    /// </summary>
    public async Task<PagedResponse<List<SaleListItemDto>>> GetPagedAsync(
        SaleListQueryDto query,
        string? customerScopeUserId)
    {
        // Normalize pagination limits to keep endpoint behavior predictable.
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize is < 1 or > 100 ? 10 : query.PageSize;

        PaymentStatus? paymentStatus = null;
        if (!string.IsNullOrWhiteSpace(query.PaymentStatus))
        {
            if (!TryParsePaymentStatus(query.PaymentStatus, out var parsedStatus))
            {
                return PagedFailure("Payment status is invalid.", "paymentStatus");
            }

            paymentStatus = parsedStatus;
        }
        SaleChannel? saleChannel = null;
        if (!string.IsNullOrWhiteSpace(query.SaleChannel))
        {
            if (!Enum.TryParse<SaleChannel>(query.SaleChannel, true, out var parsedChannel))
            {
                return PagedFailure("Sale channel is invalid.", "saleChannel");
            }

            saleChannel = parsedChannel;
        }

        // Customer-scoped reads override explicit query customer filter for authorization safety.
        var customerFilter = !string.IsNullOrWhiteSpace(customerScopeUserId)
            ? customerScopeUserId
            : query.CustomerUserId;

        var (items, totalItems) = await saleRepository.GetPagedAsync(page, pageSize, customerFilter, paymentStatus, saleChannel);
        var data = items.Select(MapListItem).ToList();

        return PagedResponse<List<SaleListItemDto>>.SuccessPagedResponse(
            data,
            new PaginationMeta
            {
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
            },
            "Sales retrieved successfully");
    }


    /// <summary>
    /// Retrieves one sale detail, optionally constrained to a specific customer scope.
    /// </summary>
    public async Task<ApiResponse<SaleDetailDto>> GetByIdAsync(int saleId, string? customerScopeUserId)
    {
        var sale = string.IsNullOrWhiteSpace(customerScopeUserId)
            ? await saleRepository.GetByIdWithDetailsAsync(saleId)
            : await saleRepository.GetByIdForCustomerAsync(saleId, customerScopeUserId);

        if (sale is null)
        {
            return ApiResponse<SaleDetailDto>.FailureResponse("Sale not found.");
        }

        return ApiResponse<SaleDetailDto>.SuccessResponse(
            MapDetail(sale),
            "Sale retrieved successfully");
    }

    /// <summary>
    /// Returns invoice-focused sale payload, optionally constrained to customer scope.
    /// </summary>
    public async Task<ApiResponse<SaleInvoiceDto>> GetInvoiceAsync(int saleId, string? customerScopeUserId)
    {
        var sale = string.IsNullOrWhiteSpace(customerScopeUserId)
            ? await saleRepository.GetByIdWithDetailsAsync(saleId)
            : await saleRepository.GetByIdForCustomerAsync(saleId, customerScopeUserId);

        if (sale is null)
        {
            return ApiResponse<SaleInvoiceDto>.FailureResponse("Sale invoice not found.");
        }

        return ApiResponse<SaleInvoiceDto>.SuccessResponse(
            MapInvoice(sale),
            "Invoice retrieved successfully");
    }

    public async Task<ApiResponse<SaleDetailDto>> GetMyOnlineOrderByIdAsync(int saleId, string customerUserId)
    {
        var sale = await saleRepository.GetByIdForCustomerAsync(saleId, customerUserId);
        if (sale is null || sale.SaleChannel != SaleChannel.OnlineParts)
        {
            return ApiResponse<SaleDetailDto>.FailureResponse("Order not found.");
        }

        return ApiResponse<SaleDetailDto>.SuccessResponse(
            MapDetail(sale),
            "Order retrieved successfully");
    }

    public async Task<ApiResponse<SaleInvoiceDto>> GetMyOnlineOrderInvoiceAsync(int saleId, string customerUserId)
    {
        var sale = await saleRepository.GetByIdForCustomerAsync(saleId, customerUserId);
        if (sale is null || sale.SaleChannel != SaleChannel.OnlineParts)
        {
            return ApiResponse<SaleInvoiceDto>.FailureResponse("Order invoice not found.");
        }

        return ApiResponse<SaleInvoiceDto>.SuccessResponse(
            MapInvoice(sale),
            "Order invoice retrieved successfully");
    }

    public async Task<ApiResponse<List<RecentPurchasedPartDto>>> GetRecentPurchasedPartsAsync(string customerUserId, int limit)
    {
        var sanitizedLimit = Math.Clamp(limit, 1, 50);

        var data = await saleRepository
            .FindByCondition(
                s => s.CustomerApplicationUserId == customerUserId &&
                     s.SaleChannel == SaleChannel.OnlineParts &&
                     s.SaleStatus != SaleStatus.Cancelled,
                trackChanges: false)
            .SelectMany(s => s.SaleItems.Select(si => new
            {
                si.PartId,
                PartName = si.Part.PartName,
                s.SaleDate,
                si.QuantitySold
            }))
            .GroupBy(x => new { x.PartId, x.PartName })
            .Select(g => new RecentPurchasedPartDto
            {
                PartId = g.Key.PartId,
                PartName = g.Key.PartName,
                LastPurchasedAt = g.Max(x => x.SaleDate),
                TotalQuantityPurchased = g.Sum(x => x.QuantitySold),
                PurchaseCount = g.Count()
            })
            .OrderByDescending(x => x.LastPurchasedAt)
            .ThenByDescending(x => x.PartId)
            .Take(sanitizedLimit)
            .ToListAsync();

        return ApiResponse<List<RecentPurchasedPartDto>>.SuccessResponse(
            data,
            "Recent purchased parts retrieved successfully.");
    }


    private static PaymentStatus ResolvePaymentStatus(decimal balanceDue, DateOnly? dueDate)
    {
        if (balanceDue <= 0m)
        {
            return PaymentStatus.Paid;
        }

        if (dueDate.HasValue && dueDate.Value < DateOnly.FromDateTime(DateTime.UtcNow))
        {
            return PaymentStatus.Overdue;
        }

        return PaymentStatus.PartiallyPaid;
    }


    private static bool TryParsePaymentStatus(string value, out PaymentStatus paymentStatus)
    {
        if (Enum.TryParse<PaymentStatus>(value, true, out paymentStatus))
        {
            return true;
        }

        if (string.Equals(value, "Partial", StringComparison.OrdinalIgnoreCase))
        {
            paymentStatus = PaymentStatus.PartiallyPaid;
            return true;
        }

        if (string.Equals(value, "Unpaid", StringComparison.OrdinalIgnoreCase))
        {
            paymentStatus = PaymentStatus.Unpaid;
            return true;
        }

        return false;
    }


    private static string MapPaymentStatus(PaymentStatus paymentStatus) =>
        paymentStatus switch
        {
            PaymentStatus.PartiallyPaid => "Partial",
            _ => paymentStatus.ToString()
        };


    private static SaleListItemDto MapListItem(Sale sale)
    {
        var paidAmount = sale.Payments.Sum(p => p.PaymentAmount);
        return new SaleListItemDto
        {
            SaleId = sale.SaleId,
            SaleInvoiceNo = sale.SaleInvoiceNo,
            CustomerName = sale.CustomerUser.FullName,
            SaleDate = DateOnly.FromDateTime(sale.SaleDate),
            TotalAmount = sale.TotalAmount,
            PaidAmount = paidAmount,
            BalanceDue = sale.BalanceDue,
            SaleChannel = sale.SaleChannel.ToString(),
            PaymentStatus = MapPaymentStatus(sale.PaymentStatus),
            DueDate = sale.DueDate.HasValue ? DateOnly.FromDateTime(sale.DueDate.Value) : null
        };
    }


    private static SaleDetailDto MapDetail(Sale sale)
    {
        var paidAmount = sale.Payments.Sum(p => p.PaymentAmount);
        return new SaleDetailDto
        {
            SaleId = sale.SaleId,
            SaleInvoiceNo = sale.SaleInvoiceNo,
            SaleDate = DateOnly.FromDateTime(sale.SaleDate),
            Customer = new SaleDetailCustomerDto
            {
                UserId = sale.CustomerApplicationUserId,
                FullName = sale.CustomerUser.FullName,
                Email = sale.CustomerUser.Email ?? string.Empty,
                PhoneNumber = sale.CustomerUser.PhoneNumber ?? string.Empty
            },
            Vehicle = MapSaleVehicle(sale),
            Items = sale.SaleItems.Select(i => new SaleDetailItemDto
            {
                PartId = i.PartId,
                PartName = i.Part.PartName,
                Quantity = i.QuantitySold,
                UnitPriceAtSale = i.UnitPriceAtSale,
                LineTotal = i.LineTotal
            }).ToList(),
            SubTotal = sale.SubTotal,
            LoyaltyDiscountPercent = sale.LoyaltyDiscountPercent,
            DiscountAmount = sale.DiscountAmount,
            TotalAmount = sale.TotalAmount,
            PaidAmount = paidAmount,
            BalanceDue = sale.BalanceDue,
            SaleChannel = sale.SaleChannel.ToString(),
            PaymentStatus = MapPaymentStatus(sale.PaymentStatus),
            DueDate = sale.DueDate.HasValue ? DateOnly.FromDateTime(sale.DueDate.Value) : null
        };
    }

    private static SaleInvoiceDto MapInvoice(Sale sale)
    {
        var paidAmount = sale.Payments.Sum(p => p.PaymentAmount);
        return new SaleInvoiceDto
        {
            InvoiceNo = sale.SaleInvoiceNo,
            InvoiceDate = DateOnly.FromDateTime(sale.SaleDate),
            CustomerName = sale.CustomerUser.FullName,
            CustomerEmail = sale.CustomerUser.Email ?? string.Empty,
            Vehicle = BuildInvoiceVehicleLabel(sale),
            Items = sale.SaleItems.Select(i => new SaleInvoiceItemDto
            {
                PartName = i.Part.PartName,
                Quantity = i.QuantitySold,
                UnitPrice = i.UnitPriceAtSale,
                LineTotal = i.LineTotal
            }).ToList(),
            SubTotal = sale.SubTotal,
            DiscountAmount = sale.DiscountAmount,
            TotalAmount = sale.TotalAmount,
            PaidAmount = paidAmount,
            BalanceDue = sale.BalanceDue
        };
    }


    private static PagedResponse<List<SaleListItemDto>> PagedFailure(string message, string errorKey) =>
        new()
        {
            Success = false,
            Message = message,
            Errors = new Dictionary<string, List<string>>
            {
                { errorKey, [message] }
            },
            Data = new List<SaleListItemDto>(),
            Pagination = new PaginationMeta()
        };

    private async Task<(bool Success, string Message, Dictionary<string, List<string>> Errors, List<SaleItem> SaleItems, Dictionary<int, Part> PartsById)>
        ValidateAndBuildSaleItemsAsync(IReadOnlyCollection<CreateSaleItemDto> items)
    {
        if (items.Count == 0)
        {
            return (
                false,
                "At least one sale item is required.",
                new Dictionary<string, List<string>> { { "items", ["At least one sale item is required."] } },
                [],
                new Dictionary<int, Part>());
        }

        var duplicatePartIds = items
            .GroupBy(i => i.PartId)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicatePartIds.Count > 0)
        {
            return (
                false,
                "Duplicate part lines are not allowed in one sale.",
                new Dictionary<string, List<string>> { { "items", ["Duplicate part lines are not allowed in one sale."] } },
                [],
                new Dictionary<int, Part>());
        }

        var partIds = items.Select(i => i.PartId).Distinct().ToList();
        var parts = await partRepository.GetByIdsAsync(partIds);
        if (parts.Count != partIds.Count)
        {
            return (
                false,
                "One or more parts were not found.",
                new Dictionary<string, List<string>> { { "items", ["One or more parts were not found."] } },
                [],
                new Dictionary<int, Part>());
        }

        var partsById = parts.ToDictionary(p => p.PartId);
        foreach (var line in items)
        {
            var part = partsById[line.PartId];
            if (!part.IsActive)
            {
                return (
                    false,
                    $"Part '{part.PartName}' is inactive.",
                    new Dictionary<string, List<string>> { { "items", [$"Part '{part.PartName}' is inactive."] } },
                    [],
                    new Dictionary<int, Part>());
            }

            if (part.CurrentStock < line.Quantity)
            {
                return (
                    false,
                    $"Insufficient stock for part '{part.PartName}'.",
                    new Dictionary<string, List<string>> { { "items", [$"Insufficient stock for part '{part.PartName}'."] } },
                    [],
                    new Dictionary<int, Part>());
            }
        }

        var saleItems = items.Select(line =>
        {
            var part = partsById[line.PartId];
            return new SaleItem
            {
                PartId = line.PartId,
                QuantitySold = line.Quantity,
                UnitPriceAtSale = part.SellingPrice,
                LineTotal = part.SellingPrice * line.Quantity
            };
        }).ToList();

        return (true, string.Empty, new Dictionary<string, List<string>>(), saleItems, partsById);
    }

    private static SaleDetailVehicleDto MapSaleVehicle(Sale sale)
    {
        if (sale.Vehicle is null)
        {
            return new SaleDetailVehicleDto();
        }

        return new SaleDetailVehicleDto
        {
            VehicleId = sale.VehicleId ?? 0,
            VehicleNumber = sale.Vehicle.VehicleNumber,
            BrandName = sale.Vehicle.Model.Brand.BrandName,
            ModelName = sale.Vehicle.Model.ModelName
        };
    }

    private static string BuildInvoiceVehicleLabel(Sale sale) =>
        sale.Vehicle is null
            ? "N/A"
            : $"{sale.Vehicle.Model.Brand.BrandName} {sale.Vehicle.Model.ModelName} - {sale.Vehicle.VehicleNumber}";
}
