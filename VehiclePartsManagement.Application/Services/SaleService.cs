using System.Text;
using VehiclePartsManagement.Application.Common;
using VehiclePartsManagement.Application.Interfaces.Repositories;
using VehiclePartsManagement.Application.Interfaces.Services;

namespace VehiclePartsManagement.Application.Services;

public class SaleService(ISaleRepository saleRepository, IEmailService emailService) : ISaleService
{
    public async Task<ApiResponse> SendInvoiceEmailAsync(int saleId)
    {
        var sale = await saleRepository.GetSaleWithDetailsAsync(saleId);
        if (sale == null)
            return ApiResponse.FailureResponse("Sale not found");

        if (string.IsNullOrEmpty(sale.CustomerUser?.Email))
            return ApiResponse.FailureResponse("Customer email not found");

        var subject = $"Invoice for Sale #{sale.SaleInvoiceNo}";
        var body = GenerateInvoiceEmailBody(sale);

        var sent = await emailService.SendEmailAsync(sale.CustomerUser.Email, subject, body);

        return sent 
            ? ApiResponse.SuccessResponse("Invoice email sent successfully")
            : ApiResponse.FailureResponse("Failed to send invoice email");
    }

    private string GenerateInvoiceEmailBody(Domain.Entities.Sale sale)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Hello {sale.CustomerUser.FullName},");
        sb.AppendLine();
        sb.AppendLine($"Thank you for your purchase. Here is your invoice details for Sale #{sale.SaleInvoiceNo}:");
        sb.AppendLine("--------------------------------------------------");
        foreach (var item in sale.SaleItems)
        {
            sb.AppendLine($"{item.Part.PartName} x {item.QuantitySold} @ {item.UnitPriceAtSale:C} = {item.LineTotal:C}");
        }
        sb.AppendLine("--------------------------------------------------");
        sb.AppendLine($"SubTotal: {sale.SubTotal:C}");
        sb.AppendLine($"Discount: {sale.DiscountAmount:C}");
        sb.AppendLine($"Total: {sale.TotalAmount:C}");
        sb.AppendLine($"Balance Due: {sale.BalanceDue:C}");
        if (sale.DueDate.HasValue)
        {
            sb.AppendLine($"Due Date: {sale.DueDate.Value.ToShortDateString()}");
        }
        sb.AppendLine();
        sb.AppendLine("Regards,");
        sb.AppendLine("Vehicle Parts Management Team");
        return sb.ToString();
    }
}
