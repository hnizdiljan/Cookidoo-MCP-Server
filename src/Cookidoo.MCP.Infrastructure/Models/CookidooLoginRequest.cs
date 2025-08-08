using System.Text.Json.Serialization;

namespace Cookidoo.MCP.Infrastructure.Models;

/// <summary>
/// Požadavek na přihlášení do Cookidoo API
/// </summary>
public class CookidooLoginRequest
{
    /// <summary>
    /// Email uživatele
    /// </summary>
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Heslo uživatele
    /// </summary>
    [JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;
} 