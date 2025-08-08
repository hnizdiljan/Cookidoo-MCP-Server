namespace Cookidoo.MCP.Infrastructure.Configuration;

/// <summary>
/// Konfigurační možnosti pro Cookidoo API podle technického zadání
/// </summary>
public class CookidooApiOptions
{
    /// <summary>
    /// Název sekce v konfiguraci
    /// </summary>
    public const string SectionName = "CookidooApi";

    /// <summary>
    /// Základní URL pro Cookidoo API
    /// </summary>
    public string BaseUrl { get; set; } = "https://cookidoo.thermomix.com";

    /// <summary>
    /// Endpoint pro přihlášení
    /// </summary>
    public string LoginPath { get; set; } = "/api/v2/authentication/login";

    /// <summary>
    /// Endpoint pro odhlášení
    /// </summary>
    public string LogoutPath { get; set; } = "/api/v2/authentication/logout";

    /// <summary>
    /// User-Agent string pro HTTP požadavky
    /// </summary>
    public string UserAgent { get; set; } = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/125.0.0.0 Safari/537.36";
} 