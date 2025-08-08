using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Cookidoo.MCP.Infrastructure.Configuration;
using Cookidoo.MCP.Infrastructure.Services;
using Cookidoo.MCP.Core.Exceptions;

namespace QuickTest;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("🍳 Cookidoo MCP Server - OAuth2 Test");
        Console.WriteLine("=====================================\n");

        // Konfigurace podle appsettings.json
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
        services.AddLogging(builder => 
            builder.AddConsole()
                   .SetMinimumLevel(LogLevel.Information));
        
        services.Configure<CookidooOptions>(configuration.GetSection("Cookidoo"));
        services.AddHttpClient();
        services.AddScoped<CookidooAuthService>();

        var serviceProvider = services.BuildServiceProvider();
        var authService = serviceProvider.GetRequiredService<CookidooAuthService>();
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

        Console.WriteLine("🔧 Konfigurace načtena:");
        Console.WriteLine($"   API Endpoint: https://ch.tmmobile.vorwerk-digital.com");
        Console.WriteLine($"   Token Path: ciam/auth/token");
        Console.WriteLine($"   Client ID: kupferwerk-client-nwot");
        Console.WriteLine();

        // Test 1: Mock test (očekáváme 401 Unauthorized)
        Console.WriteLine("📋 Test 1: Mock přihlašovací údaje (očekáváme chybu)");
        try
        {
            var mockResult = await authService.LoginAsync("test@example.com", "wrongpassword");
            Console.WriteLine($"❌ Neočekávaný úspěch: {mockResult.AccessToken}");
        }
        catch (CookidooAuthenticationException ex)
        {
            if (ex.Message.Contains("Neplatné přihlašovací údaje"))
            {
                Console.WriteLine("✅ OAuth2 komunikace funguje - získali jsme očekávanou chybu 401");
            }
            else
            {
                Console.WriteLine($"⚠️  Jiná autentizační chyba: {ex.Message}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Neočekávaná chyba: {ex.Message}");
        }

        Console.WriteLine();

        // Test 2: Interaktivní test s reálnými údaji
        Console.WriteLine("📋 Test 2: Chcete vyzkoušet s reálnými Cookidoo údaji? (y/n)");
        var response = Console.ReadLine();
        
        if (response?.ToLower() == "y" || response?.ToLower() == "yes")
        {
            Console.Write("📧 Email: ");
            var email = Console.ReadLine();
            
            Console.Write("🔐 Heslo: ");
            var password = ReadPassword();
            
            if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(password))
            {
                Console.WriteLine("\n🔄 Přihlašuji...");
                
                try
                {
                    var result = await authService.LoginAsync(email, password);
                    Console.WriteLine("✅ Přihlášení úspěšné!");
                    Console.WriteLine($"   Access Token: {result.AccessToken[..20]}...");
                    Console.WriteLine($"   Token Type: {result.TokenType}");
                    Console.WriteLine($"   Expires In: {result.ExpiresIn} sekund");
                    Console.WriteLine($"   User ID: {result.Sub}");

                    // Test načtení informací o uživateli
                    Console.WriteLine("\n🔄 Načítám informace o uživateli...");
                    var userInfo = await authService.GetUserInfoAsync(result.AccessToken);
                    Console.WriteLine("✅ Informace o uživateli načteny!");
                    Console.WriteLine($"   Username: {userInfo.Username}");
                    Console.WriteLine($"   Description: {userInfo.Description ?? "N/A"}");
                    Console.WriteLine($"   Picture: {userInfo.Picture ?? "N/A"}");
                }
                catch (CookidooAuthenticationException ex)
                {
                    Console.WriteLine($"❌ Autentizační chyba: {ex.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Chyba: {ex.Message}");
                }
            }
        }

        Console.WriteLine("\n🎉 Test dokončen!");
        Console.WriteLine("\n💡 Pro další testování:");
        Console.WriteLine("   • Spusťte API server: dotnet run --project Cookidoo.MCP.Api");
        Console.WriteLine("   • Otevřete Swagger: http://localhost:5555/swagger");
        Console.WriteLine("   • Použijte /api/v1/auth/login endpoint");
    }

    private static string ReadPassword()
    {
        string password = "";
        ConsoleKeyInfo key;
        
        do
        {
            key = Console.ReadKey(true);
            
            if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
            {
                password += key.KeyChar;
                Console.Write("*");
            }
            else if (key.Key == ConsoleKey.Backspace && password.Length > 0)
            {
                password = password[0..^1];
                Console.Write("\b \b");
            }
        }
        while (key.Key != ConsoleKey.Enter);
        
        Console.WriteLine();
        return password;
    }
} 