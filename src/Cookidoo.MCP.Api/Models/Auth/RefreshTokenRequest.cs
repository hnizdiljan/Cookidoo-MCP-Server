using System.ComponentModel.DataAnnotations;

namespace Cookidoo.MCP.Api.Models.Auth;

/// <summary>
/// Požadavek na obnovení JWT tokenu
/// </summary>
public class RefreshTokenRequest
{
    /// <summary>
    /// Starý JWT token k obnovení
    /// </summary>
    [Required(ErrorMessage = "Token je povinný")]
    public string Token { get; set; } = string.Empty;
} 