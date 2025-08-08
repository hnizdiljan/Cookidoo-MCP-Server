namespace Cookidoo.MCP.Infrastructure.Configuration;

/// <summary>
/// Konfigurace pro JWT autentizaci
/// </summary>
public class JwtOptions
{
    /// <summary>
    /// Název sekce v appsettings.json
    /// </summary>
    public const string SectionName = "Jwt";

    /// <summary>
    /// Tajný klíč pro podpis tokenů
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>
    /// Vydavatel tokenu
    /// </summary>
    public string Issuer { get; set; } = "CookidooMcpServer";

    /// <summary>
    /// Cílová skupina tokenu
    /// </summary>
    public string Audience { get; set; } = "CookidooMcpApi";

    /// <summary>
    /// Doba platnosti tokenu v minutách
    /// </summary>
    public int ExpirationMinutes { get; set; } = 60;
} 