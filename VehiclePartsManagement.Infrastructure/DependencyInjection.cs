using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using VehiclePartsManagement.Application.Config;
using VehiclePartsManagement.Application.Interfaces;
using VehiclePartsManagement.Application.Interfaces.Repositories;
using VehiclePartsManagement.Application.Interfaces.Services;
using VehiclePartsManagement.Application.Services;
using VehiclePartsManagement.Domain.Entities;
using VehiclePartsManagement.Infrastructure.Config;
using VehiclePartsManagement.Infrastructure.Data;
using VehiclePartsManagement.Infrastructure.Repositories;

namespace VehiclePartsManagement.Infrastructure;

/// <summary>
/// Registers infrastructure-layer dependencies.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds persistence, identity, authentication, and infrastructure services.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <param name="configuration">Application configuration.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services
            .AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        services.AddOptions<ImageStorageOptions>()
            .Bind(configuration.GetSection(ImageStorageOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<VehiclePartsManagement.Application.Config.EmailOptions>()
            .Bind(configuration.GetSection(VehiclePartsManagement.Application.Config.EmailOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<KhaltiOptions>()
            .Bind(configuration.GetSection(KhaltiOptions.SectionName));

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IImageStorageService, ImageStorageService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IPartService, PartService>();
        services.AddScoped<IPartCategoryService, PartCategoryService>();
        services.AddScoped<IVehicleService, VehicleService>();
        services.AddScoped<IPartService, PartService>();
        services.AddScoped<IVendorService, VendorService>();
        services.AddScoped<IPurchaseService, PurchaseService>();
        services.AddScoped<ISaleService, SaleService>();
        services.AddHttpClient<IPaymentService, PaymentService>();
        services.AddScoped<IServiceAppointmentService, ServiceAppointmentService>();
        services.AddScoped<IAppointmentSlotService, AppointmentSlotService>();
        services.AddScoped<IServiceRecordService, ServiceRecordService>();
        services.AddScoped<IReviewService, ReviewService>();
        services.AddScoped<IPartRequestService, PartRequestService>();
        services.AddScoped<IReportsService, ReportsService>();
        services.AddScoped<IReportPdfService, ReportPdfService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<INotificationService, NotificationService>();

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IVehicleRepository, VehicleRepository>();
        services.AddScoped<IPartRepository, PartRepository>();
        services.AddScoped<IPartCategoryRepository, PartCategoryRepository>();
        services.AddScoped<IVendorRepository, VendorRepository>();
        services.AddScoped<IPurchaseRepository, PurchaseRepository>();
        services.AddScoped<ISaleRepository, SaleRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IStockMovementRepository, StockMovementRepository>();
        services.AddScoped<IServiceAppointmentRepository, ServiceAppointmentRepository>();
        services.AddScoped<IAppointmentSlotRepository, AppointmentSlotRepository>();
        services.AddScoped<IServiceRecordRepository, ServiceRecordRepository>();
        services.AddScoped<IReviewRepository, ReviewRepository>();
        services.AddScoped<IPartRequestRepository, PartRequestRepository>();
        services.AddScoped<IReportsRepository, ReportsRepository>();
        services.AddScoped(typeof(IRepositoryBase<>), typeof(RepositoryBase<>));

        AddJwtAuthentication(services, configuration);

        return services;
    }


    /// <summary>
    /// Configures JWT bearer authentication and authorization.
    /// </summary>
    private static void AddJwtAuthentication(IServiceCollection services, IConfiguration configuration)
    {
        var jwtConfig = configuration.GetSection(JwtOptions.SectionName);

        services.AddOptions<JwtOptions>()
            .Bind(jwtConfig)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // Validated options are required for token validation parameters at startup.
        var jwtOptions = jwtConfig.Get<JwtOptions>()!;

        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtOptions.Issuer,
                ValidateAudience = true,
                ValidAudience = jwtOptions.Audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = jwtOptions.SymmetricSecurityKey,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        });

        services.AddAuthorization();
    }
}
