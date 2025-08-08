namespace Cookidoo.MCP.Api.Models.Auth;

/// <summary>
/// Informace o uživateli
/// </summary>
public class UserInfo
{
    /// <summary>
    /// ID uživatele v Cookidoo
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Email uživatele
    /// </summary>
    public string Email { get; set; } = string.Empty;
} 