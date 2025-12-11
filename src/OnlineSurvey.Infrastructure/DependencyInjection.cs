using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OnlineSurvey.Application.Interfaces;
using OnlineSurvey.Domain.Repositories;
using OnlineSurvey.Infrastructure.Caching;
using OnlineSurvey.Infrastructure.Data;
using OnlineSurvey.Infrastructure.Repositories;

namespace OnlineSurvey.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        // Only register PostgreSQL if connection string is available
        // This allows tests to replace with InMemory database
        if (!string.IsNullOrEmpty(connectionString))
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(connectionString, npgsqlOptions =>
                {
                    npgsqlOptions.EnableRetryOnFailure(3);
                    npgsqlOptions.CommandTimeout(30);
                }));
        }

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ISurveyRepository, SurveyRepository>();
        services.AddScoped<IResponseRepository, ResponseRepository>();

        services.AddMemoryCache();
        services.AddSingleton<ICacheService, MemoryCacheService>();

        return services;
    }
}
