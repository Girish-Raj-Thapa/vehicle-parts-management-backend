using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using VehiclePartsManagement.Application.Common;
using VehiclePartsManagement.Application.Config;
using VehiclePartsManagement.Application.DTOs.Payment;
using VehiclePartsManagement.Application.Interfaces.Repositories;
using VehiclePartsManagement.Application.Interfaces.Services;
using VehiclePartsManagement.Domain.Entities;
using VehiclePartsManagement.Domain.Enums;

namespace VehiclePartsManagement.Application.Services;

public class PaymentService(
    HttpClient httpClient,
    IOptions<KhaltiOptions> khaltiOptions,
    ISaleRepository saleRepository,
    IPaymentRepository paymentRepository,
    ISaleService saleService) : IPaymentService
{
    private const string CompletedStatus = "Completed";
    private readonly KhaltiOptions _khaltiOptions = khaltiOptions.Value;


    public async Task<ApiResponse<KhaltiInitiateResponseDto>> InitiateKhaltiPaymentAsync(
        KhaltiInitiatePaymentDto dto,
        string userId,
        bool canManagePayments)
    {
        try
        {
            return await InitiateKhaltiPaymentCoreAsync(dto, userId, canManagePayments);
        }
        catch (Exception ex)
        {
            return ApiResponse<KhaltiInitiateResponseDto>.FailureResponse(
                "Khalti payment initiation failed.",
                new Dictionary<string, List<string>> { { "khalti", [ex.Message] } });
        }
    }


    private async Task<ApiResponse<KhaltiInitiateResponseDto>> InitiateKhaltiPaymentCoreAsync(
        KhaltiInitiatePaymentDto dto,
        string userId,
        bool canManagePayments)
    {
        var configError = ValidateConfiguration();
        if (configError is not null)
        {
            return ApiResponse<KhaltiInitiateResponseDto>.FailureResponse(configError);
        }

        var sale = await saleRepository
            .FindByCondition(s => s.SaleId == dto.SaleId, trackChanges: true)
            .Include(s => s.CustomerUser)
            .FirstOrDefaultAsync();

        if (sale is null)
        {
            return ApiResponse<KhaltiInitiateResponseDto>.FailureResponse("Sale not found.");
        }

        if (!canManagePayments && sale.CustomerApplicationUserId != userId)
        {
            return ApiResponse<KhaltiInitiateResponseDto>.FailureResponse("Sale not found.");
        }

        if (sale.BalanceDue <= 0)
        {
            return ApiResponse<KhaltiInitiateResponseDto>.FailureResponse("Sale has no outstanding balance.");
        }

        var amountPaisa = ToPaisa(sale.BalanceDue);
        if (amountPaisa < 1000)
        {
            return ApiResponse<KhaltiInitiateResponseDto>.FailureResponse("Khalti amount must be at least Rs. 10.");
        }

        var purchaseOrderId = $"SALE-{sale.SaleId}-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
        var payload = new KhaltiInitiateRequest(
            _khaltiOptions.ReturnUrl,
            _khaltiOptions.WebsiteUrl,
            amountPaisa,
            purchaseOrderId,
            $"Invoice {sale.SaleInvoiceNo}",
            new KhaltiCustomerInfo(
                sale.CustomerUser.FullName,
                sale.CustomerUser.Email ?? string.Empty,
                sale.CustomerUser.PhoneNumber ?? string.Empty));

        var request = CreateKhaltiRequest(HttpMethod.Post, "epayment/initiate/", payload);
        var (response, requestError) = await SendKhaltiRequestAsync(request);
        if (requestError is not null)
        {
            return ApiResponse<KhaltiInitiateResponseDto>.FailureResponse(requestError);
        }

        if (!response.IsSuccessStatusCode)
        {
            return ApiResponse<KhaltiInitiateResponseDto>.FailureResponse(
                "Khalti payment initiation failed.",
                await ReadKhaltiErrorsAsync(response));
        }

        var khaltiResponse = await response.Content.ReadFromJsonAsync<KhaltiInitiateApiResponse>();
        if (khaltiResponse is null || string.IsNullOrWhiteSpace(khaltiResponse.Pidx))
        {
            return ApiResponse<KhaltiInitiateResponseDto>.FailureResponse("Invalid response received from Khalti.");
        }

        return ApiResponse<KhaltiInitiateResponseDto>.SuccessResponse(
            new KhaltiInitiateResponseDto
            {
                SaleId = sale.SaleId,
                Pidx = khaltiResponse.Pidx,
                PaymentUrl = khaltiResponse.PaymentUrl,
                AmountPaisa = amountPaisa,
                PurchaseOrderId = purchaseOrderId,
                ExpiresAt = khaltiResponse.ExpiresAt,
                ExpiresIn = khaltiResponse.ExpiresIn
            },
            "Khalti payment initiated successfully.");
    }


    public async Task<ApiResponse<KhaltiVerifyResponseDto>> VerifyKhaltiPaymentAsync(
        KhaltiVerifyPaymentDto dto,
        string? userId,
        bool canManagePayments)
    {
        try
        {
            return await VerifyKhaltiPaymentCoreAsync(dto, userId, canManagePayments);
        }
        catch (Exception ex)
        {
            return ApiResponse<KhaltiVerifyResponseDto>.FailureResponse(
                "Khalti payment verification failed.",
                new Dictionary<string, List<string>> { { "khalti", [ex.Message] } });
        }
    }


    private async Task<ApiResponse<KhaltiVerifyResponseDto>> VerifyKhaltiPaymentCoreAsync(
        KhaltiVerifyPaymentDto dto,
        string? userId,
        bool canManagePayments)
    {
        var configError = ValidateConfiguration();
        if (configError is not null)
        {
            return ApiResponse<KhaltiVerifyResponseDto>.FailureResponse(configError);
        }

        var pidx = dto.Pidx.Trim();
        var request = CreateKhaltiRequest(HttpMethod.Post, "epayment/lookup/", new KhaltiLookupRequest(pidx));
        var (response, requestError) = await SendKhaltiRequestAsync(request);
        if (requestError is not null)
        {
            return ApiResponse<KhaltiVerifyResponseDto>.FailureResponse(requestError);
        }

        if (!response.IsSuccessStatusCode)
        {
            return ApiResponse<KhaltiVerifyResponseDto>.FailureResponse(
                "Khalti payment lookup failed.",
                await ReadKhaltiErrorsAsync(response));
        }

        var lookup = await response.Content.ReadFromJsonAsync<KhaltiLookupApiResponse>();
        if (lookup is null || string.IsNullOrWhiteSpace(lookup.Status))
        {
            return ApiResponse<KhaltiVerifyResponseDto>.FailureResponse("Invalid response received from Khalti.");
        }

        var saleId = dto.SaleId ?? GetSaleIdFromPurchaseOrderId(lookup.PurchaseOrderId);
        if (saleId is null)
        {
            return ApiResponse<KhaltiVerifyResponseDto>.FailureResponse("saleId is required to verify this Khalti payment.");
        }

        var sale = await saleRepository
            .FindByCondition(s => s.SaleId == saleId.Value, trackChanges: true)
            .FirstOrDefaultAsync();

        if (sale is null)
        {
            return ApiResponse<KhaltiVerifyResponseDto>.FailureResponse("Sale not found.");
        }

        if (!canManagePayments && !string.IsNullOrWhiteSpace(userId) && sale.CustomerApplicationUserId != userId)
        {
            return ApiResponse<KhaltiVerifyResponseDto>.FailureResponse("Sale not found.");
        }

        var existingPayment = await paymentRepository
            .FindByCondition(p => p.PaymentReference == pidx, trackChanges: true)
            .Include(p => p.Sale)
            .FirstOrDefaultAsync();

        if (existingPayment is not null)
        {
            return ApiResponse<KhaltiVerifyResponseDto>.SuccessResponse(
                MapVerifyResponse(existingPayment, lookup, existingPayment.PaymentAmount),
                "Khalti payment already verified.");
        }

        var expectedAmountPaisa = ToPaisa(sale.BalanceDue);
        if (lookup.TotalAmount != expectedAmountPaisa)
        {
            return ApiResponse<KhaltiVerifyResponseDto>.FailureResponse("Khalti amount does not match the invoice amount.");
        }

        if (!string.Equals(lookup.Status, CompletedStatus, StringComparison.OrdinalIgnoreCase))
        {
            return ApiResponse<KhaltiVerifyResponseDto>.SuccessResponse(
                MapVerifyResponse(sale, lookup, 0),
                $"Khalti payment status is {lookup.Status}.");
        }

        var paidAmount = FromPaisa(lookup.TotalAmount);
        var payment = new Payment
        {
            SaleId = sale.SaleId,
            PaymentDate = DateTime.UtcNow,
            PaymentMethod = PaymentMethodType.MobileWallet,
            PaymentAmount = paidAmount,
            PaymentReference = pidx,
            PaymentNote = BuildPaymentNote(lookup.Status, expectedAmountPaisa, lookup.PurchaseOrderId, lookup.TransactionId)
        };

        sale.BalanceDue = Math.Max(0, sale.BalanceDue - paidAmount);
        sale.PaymentStatus = sale.BalanceDue == 0
            ? PaymentStatus.Paid
            : PaymentStatus.PartiallyPaid;

        paymentRepository.Create(payment);
        saleRepository.Update(sale);
        await paymentRepository.SaveChangesAsync();

        var message = "Khalti payment verified successfully.";
        if (sale.SaleChannel == SaleChannel.OnlineParts)
        {
            var emailResult = await saleService.SendInvoiceEmailAsync(sale.SaleId);
            if (!emailResult.Success)
            {
                message = "Khalti payment verified successfully. Invoice email could not be sent.";
            }
        }

        return ApiResponse<KhaltiVerifyResponseDto>.SuccessResponse(
            MapVerifyResponse(sale, lookup, payment.PaymentAmount),
            message);
    }


    private string? ValidateConfiguration()
    {
        if (string.IsNullOrWhiteSpace(_khaltiOptions.SecretKey) ||
            string.IsNullOrWhiteSpace(_khaltiOptions.BaseUrl) ||
            string.IsNullOrWhiteSpace(_khaltiOptions.ReturnUrl) ||
            string.IsNullOrWhiteSpace(_khaltiOptions.WebsiteUrl))
        {
            return "Khalti payment is not configured.";
        }

        return null;
    }


    private HttpRequestMessage CreateKhaltiRequest<TPayload>(HttpMethod method, string path, TPayload payload)
    {
        var baseUrl = _khaltiOptions.BaseUrl.TrimEnd('/');
        var request = new HttpRequestMessage(method, $"{baseUrl}/{path}")
        {
            Content = JsonContent.Create(payload)
        };

        request.Headers.Authorization = new AuthenticationHeaderValue("Key", _khaltiOptions.SecretKey);
        return request;
    }


    private async Task<(HttpResponseMessage Response, string? ErrorMessage)> SendKhaltiRequestAsync(HttpRequestMessage request)
    {
        try
        {
            return (await httpClient.SendAsync(request), null);
        }
        catch (HttpRequestException)
        {
            return (new HttpResponseMessage(), "Could not connect to Khalti. Check internet connection and Khalti configuration.");
        }
        catch (TaskCanceledException)
        {
            return (new HttpResponseMessage(), "Khalti request timed out. Please try again.");
        }
        catch (InvalidOperationException)
        {
            return (new HttpResponseMessage(), "Khalti configuration is invalid.");
        }
    }


    private static int ToPaisa(decimal amount) =>
        (int)Math.Round(amount * 100, MidpointRounding.AwayFromZero);


    private static decimal FromPaisa(int amountPaisa) => amountPaisa / 100m;


    private static int? GetSaleIdFromPurchaseOrderId(string? purchaseOrderId)
    {
        if (string.IsNullOrWhiteSpace(purchaseOrderId))
        {
            return null;
        }

        var parts = purchaseOrderId.Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return parts.Length >= 2 &&
               string.Equals(parts[0], "SALE", StringComparison.OrdinalIgnoreCase) &&
               int.TryParse(parts[1], out var saleId)
            ? saleId
            : null;
    }


    private static string BuildPaymentNote(string status, int expectedPaisa, string? purchaseOrderId, string? transactionId)
    {
        var note = $"provider=Khalti;status={status};expected_paisa={expectedPaisa}";
        if (!string.IsNullOrWhiteSpace(purchaseOrderId))
        {
            note += $";order={purchaseOrderId}";
        }

        if (!string.IsNullOrWhiteSpace(transactionId))
        {
            note += $";transaction={transactionId}";
        }

        return note.Length <= 500 ? note : note[..500];
    }


    private static KhaltiVerifyResponseDto MapVerifyResponse(
        Sale sale,
        KhaltiLookupApiResponse lookup,
        decimal paidAmount) =>
        new()
        {
            SaleId = sale.SaleId,
            Pidx = lookup.Pidx,
            Status = lookup.Status,
            TransactionId = lookup.TransactionId,
            TotalAmountPaisa = lookup.TotalAmount,
            PaidAmount = paidAmount,
            BalanceDue = sale.BalanceDue,
            PaymentStatus = sale.PaymentStatus.ToString()
        };


    private static KhaltiVerifyResponseDto MapVerifyResponse(
        Payment payment,
        KhaltiLookupApiResponse lookup,
        decimal paidAmount) =>
        new()
        {
            SaleId = payment.SaleId,
            Pidx = lookup.Pidx,
            Status = lookup.Status,
            TransactionId = lookup.TransactionId,
            TotalAmountPaisa = lookup.TotalAmount,
            PaidAmount = paidAmount,
            BalanceDue = payment.Sale.BalanceDue,
            PaymentStatus = payment.Sale.PaymentStatus.ToString()
        };


    private static async Task<Dictionary<string, List<string>>> ReadKhaltiErrorsAsync(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        return new Dictionary<string, List<string>>
        {
            { "khalti", [string.IsNullOrWhiteSpace(content) ? response.ReasonPhrase ?? "Khalti request failed." : content] }
        };
    }


    private sealed record KhaltiInitiateRequest(
        [property: JsonPropertyName("return_url")] string ReturnUrl,
        [property: JsonPropertyName("website_url")] string WebsiteUrl,
        [property: JsonPropertyName("amount")] int Amount,
        [property: JsonPropertyName("purchase_order_id")] string PurchaseOrderId,
        [property: JsonPropertyName("purchase_order_name")] string PurchaseOrderName,
        [property: JsonPropertyName("customer_info")] KhaltiCustomerInfo CustomerInfo);


    private sealed record KhaltiCustomerInfo(
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("email")] string Email,
        [property: JsonPropertyName("phone")] string Phone);


    private sealed record KhaltiLookupRequest([property: JsonPropertyName("pidx")] string Pidx);


    private sealed class KhaltiInitiateApiResponse
    {
        [JsonPropertyName("pidx")]
        public string Pidx { get; set; } = string.Empty;

        [JsonPropertyName("payment_url")]
        public string PaymentUrl { get; set; } = string.Empty;

        [JsonPropertyName("expires_at")]
        public DateTime? ExpiresAt { get; set; }

        [JsonPropertyName("expires_in")]
        public int? ExpiresIn { get; set; }
    }


    private sealed class KhaltiLookupApiResponse
    {
        [JsonPropertyName("pidx")]
        public string Pidx { get; set; } = string.Empty;

        [JsonPropertyName("total_amount")]
        public int TotalAmount { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("transaction_id")]
        public string? TransactionId { get; set; }

        [JsonPropertyName("purchase_order_id")]
        public string? PurchaseOrderId { get; set; }
    }
}
