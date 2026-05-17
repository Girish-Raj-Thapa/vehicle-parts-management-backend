using System.ComponentModel.DataAnnotations;

namespace VehiclePartsManagement.Application.Config;

public class EmailOptions
{
    public const string SectionName = "EmailSettings";

    [Required]
    public string SmtpServer { get; set; } = string.Empty;

    [Range(1, 65535)]
    public int SmtpPort { get; set; }

    [Required]
    [EmailAddress]
    public string SenderEmail { get; set; } = string.Empty;

    [Required]
    public string SenderName { get; set; } = string.Empty;

    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    public bool UseSsl { get; set; } = true;
}
