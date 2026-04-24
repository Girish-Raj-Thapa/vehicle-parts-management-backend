using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using VehiclePartsManagement.Application.Interfaces;
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

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
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
