namespace Cookidoo.MCP.Api.Models.Auth;

/// <summary>
/// Odpověď na úspěšné přihlášení
/// </summary>
public class LoginResponse
{
    /// <summary>
    /// JWT token pro MCP API
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Doba platnosti tokenu
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Informace o uživateli
    /// </summary>
    public UserInfo User { get; set; } = new();
} 