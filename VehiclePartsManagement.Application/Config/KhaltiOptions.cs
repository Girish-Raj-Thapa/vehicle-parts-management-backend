using System.ComponentModel.DataAnnotations;

namespace VehiclePartsManagement.Application.Config;

public class KhaltiOptions
{
    public const string SectionName = "Khalti";

    [Required]
    public string BaseUrl { get; set; } = "https://dev.khalti.com/api/v2";

    [Required]
    public string SecretKey { get; set; } = string.Empty;

    [Required]
    public string ReturnUrl { get; set; } = "http://localhost:5228/api/payments/khalti/callback";

    [Required]
    public string WebsiteUrl { get; set; } = "http://localhost:5228";
}
