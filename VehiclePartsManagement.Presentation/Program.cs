using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using VehiclePartsManagement.Application.Common;
using VehiclePartsManagement.Application.Config;
using VehiclePartsManagement.Infrastructure;
using VehiclePartsManagement.Infrastructure.Config;
using VehiclePartsManagement.Infrastructure.Data;
using VehiclePartsManagement.Presentation.Middlewares;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DictionaryKeyPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendDev", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddOpenApi();
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        // This path handles only model-binding/data-annotation validation errors.
        var errors = context.ModelState
            .Where(x => x.Value?.Errors.Count > 0)
            .ToDictionary(
                kvp =>
                {
                    if (string.IsNullOrWhiteSpace(kvp.Key))
                    {
                        return "general";
                    }

                    return char.ToLowerInvariant(kvp.Key[0]) + kvp.Key[1..];
                },
                kvp => kvp.Value!.Errors
                    .Select(e => string.IsNullOrWhiteSpace(e.ErrorMessage)
                        ? "Invalid value"
                        : e.ErrorMessage)
                    .ToList());

        var response = ApiResponse.FailureResponse(
            message: "Validation failed",
            errors: errors);

        return new BadRequestObjectResult(response);
    };
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.Database.MigrateAsync();

    await IdentitySeeder.SeedAsync(scope.ServiceProvider);
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DataSeeder.SeedAsync(context);
}

app.UseMiddleware<ExceptionHandler>();

var imageStorageOptions = app.Services.GetRequiredService<IOptions<ImageStorageOptions>>().Value;
var imageStorageRoot = Path.Combine(app.Environment.ContentRootPath, imageStorageOptions.RelativeStoragePath);
Directory.CreateDirectory(imageStorageRoot);

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(imageStorageRoot),
    RequestPath = imageStorageOptions.RequestPath
});

app.UseHttpsRedirection();

app.UseCors("FrontendDev");
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/test", () => "Server is running!");

app.Run();
