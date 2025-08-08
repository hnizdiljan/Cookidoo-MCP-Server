using System.Text.Json.Serialization;
using Cookidoo.MCP.Core.Interfaces;

namespace Cookidoo.MCP.Infrastructure.Models;

/// <summary>
/// Odpověď z Cookidoo API při přihlášení
/// </summary>
public class CookidooLoginResponse
{
    /// <summary>
    /// JWT token pro autentizaci
    /// </summary>
    [JsonPropertyName("token")]
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Informace o uživateli
    /// </summary>
    [JsonPropertyName("user")]
    public CookidooUser? User { get; set; }

    /// <summary>
    /// Doba platnosti tokenu
    /// </summary>
    [JsonPropertyName("expiresAt")]
    public DateTime? ExpiresAt { get; set; }
} 