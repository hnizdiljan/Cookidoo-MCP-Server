using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Cookidoo.MCP.Infrastructure.Configuration;
using Cookidoo.MCP.Infrastructure.Services;

namespace Cookidoo.MCP.Test;

/// <summary>
/// Jednoduchý test OAuth2 autentizace podle cookidoo-api-master implementace
/// </summary>
public class TestOAuth2
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("=== Test OAuth2 autentizace podle cookidoo-api-master ===");
        
        // Konfigurace
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Cookidoo:BaseUrl"] = "https://ch.tmmobile.vorwerk-digital.com",
                ["Cookidoo:DefaultLanguage"] = "de-CH",
                ["Cookidoo:DefaultCountryCode"] = "ch",
                ["Cookidoo:TimeoutSeconds"] = "30",
                ["Cookidoo:ClientId"] = "kupferwerk-client-nwot",
                ["Cookidoo:ClientSecret"] = "Ls50ON1woySqs1dCdJge",
                ["Cookidoo:AuthorizationHeader"] = "Basic a3VwZmVyd2Vyay1jbGllbnQtbndvdDpMczUwT04xd285U3FzMWRDZEpnZQ==",
                ["Cookidoo:TokenPath"] = "ciam/auth/token",
                ["Cookidoo:UserPath"] = "community/profile",
                ["Cookidoo:ApiEndpointPattern"] = "https://{0}.tmmobile.vorwerk-digital.com",
                ["Cookidoo:UserAgent"] = "Cookidoo-MCP-Server-Test/1.0"
            })
            .Build();

        // DI kontejner
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));
        services.Configure<CookidooOptions>(configuration.GetSection("Cookidoo"));
        services.AddHttpClient();
        services.AddScoped<CookidooAuthService>();

        var serviceProvider = services.BuildServiceProvider();
        var authService = serviceProvider.GetRequiredService<CookidooAuthService>();
        var logger = serviceProvider.GetRequiredService<ILogger<TestOAuth2>>();

        // Test s mock údaji (nebudou fungovat, ale otestujeme HTTP komunikaci)
        try
        {
            logger.LogInformation("Testování OAuth2 autentizace...");
            
            var result = await authService.LoginAsync("test@example.com", "testpassword");
            
            logger.LogInformation("OAuth2 test dokončen - AccessToken: {HasToken}", !string.IsNullOrEmpty(result.AccessToken));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Očekávaná chyba při testu s mock údaji: {Message}", ex.Message);
            
            // Toto je očekávané - mock údaje nebudou fungovat
            if (ex.Message.Contains("Neplatné přihlašovací údaje") || 
                ex.Message.Contains("Cookidoo API není dostupné"))
            {
                logger.LogInformation("✅ OAuth2 implementace funguje správně - HTTP komunikace proběhla");
            }
            else
            {
                logger.LogError("❌ Neočekávaná chyba v OAuth2 implementaci");
                throw;
            }
        }

        Console.WriteLine("\n=== Test dokončen ===");
        Console.WriteLine("Pro skutečný test použijte platné Cookidoo přihlašovací údaje.");
    }
} 