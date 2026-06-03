using CarTracking.Application.Interfaces;
using CarTracking.Application.Services;
using CarTracking.API.Hubs;
using CarTracking.Infrastructure.Data;
using CarTracking.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CarTracking.API.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Connection string 'Default' is not configured.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql =>
            {
                npgsql.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(10), errorCodesToAdd: null);
                npgsql.MigrationsAssembly(typeof(AppDbContext).Assembly.GetName().Name);
            }));

        return services;
    }

    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IVehicleRepository, VehicleRepository>();
        services.AddScoped<ILocationRepository, LocationRepository>();
        services.AddScoped<IVehicleService, VehicleService>();
        services.AddScoped<ILocationService, LocationService>();

        // ILocationBroadcaster is a singleton because IHubContext<T> is singleton-safe
        services.AddSingleton<ILocationBroadcaster, VehicleLocationBroadcaster>();

        return services;
    }

    public static async Task ApplyMigrationsAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();

        logger.LogInformation("Applying database migrations...");
        await db.Database.MigrateAsync();
        logger.LogInformation("Database migrations applied.");
    }
}
