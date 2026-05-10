namespace VehiclePartsManagement.Application.DTOs.Customer;

public class CustomerHistoryDto
{
    public string UserId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public List<CustomerSaleHistoryItemDto> PurchaseHistory { get; set; } = [];
    public List<CustomerServiceHistoryItemDto> ServiceHistory { get; set; } = [];
}
