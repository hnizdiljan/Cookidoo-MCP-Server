using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Cookidoo.MCP.Core.Interfaces;
using Cookidoo.MCP.Infrastructure.Configuration;
using Cookidoo.MCP.Infrastructure.Services;

namespace Cookidoo.MCP.Infrastructure.Extensions;

/// <summary>
/// Extension methods pro registraci všech služeb aplikace
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registruje všechny Cookidoo služby
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">Konfigurace aplikace</param>
    /// <returns>Service collection pro fluent API</returns>
    public static IServiceCollection AddCookidooServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Základní infrastruktura
        services.AddInfrastructure(configuration);
        
        // Business služby
        services.AddBusinessServices();
        
        return services;
    }

    /// <summary>
    /// Registruje infrastrukturní služby do DI kontejneru
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">Konfigurace aplikace</param>
    /// <returns>Service collection pro fluent API</returns>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Konfigurace
        services.Configure<CookidooOptions>(configuration.GetSection(CookidooOptions.SectionName));

        // HTTP klient pro Cookidoo API
        services.AddHttpClient<ICookidooApiService, CookidooApiService>((serviceProvider, client) =>
        {
            var options = configuration.GetSection(CookidooOptions.SectionName).Get<CookidooOptions>() ?? new CookidooOptions();
            
            client.BaseAddress = new Uri(options.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
            client.DefaultRequestHeaders.Add("User-Agent", options.UserAgent);
        });

        // Registrace služeb
        services.AddScoped<ICookidooApiService, CookidooApiService>();

        return services;
    }

    /// <summary>
    /// Registruje business služby
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection pro fluent API</returns>
    public static IServiceCollection AddBusinessServices(this IServiceCollection services)
    {
        services.AddScoped<IRecipeService, RecipeService>();
        services.AddScoped<ICollectionService, CollectionService>();

        return services;
    }


} 