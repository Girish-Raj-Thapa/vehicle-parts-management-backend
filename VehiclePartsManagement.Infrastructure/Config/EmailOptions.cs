using System.ComponentModel.DataAnnotations;

namespace VehiclePartsManagement.Infrastructure.Config;

public class EmailOptions
{
    public const string SectionName = "Email";

    [Required]
    public string Host { get; set; } = string.Empty;

    [Range(1, 65535)]
    public int Port { get; set; } = 587;

    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string FromEmail { get; set; } = string.Empty;

    [Required]
    public string FromName { get; set; } = "Vehicle Parts Management";
}
