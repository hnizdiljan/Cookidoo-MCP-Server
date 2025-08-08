namespace Cookidoo.MCP.Api.Models.Auth;

/// <summary>
/// Odpověď na obnovení JWT tokenu
/// </summary>
public class RefreshTokenResponse
{
    /// <summary>
    /// Nový JWT token
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Doba platnosti nového tokenu
    /// </summary>
    public DateTime ExpiresAt { get; set; }
} 