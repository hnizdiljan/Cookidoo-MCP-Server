using System.ComponentModel.DataAnnotations;

namespace Cookidoo.MCP.Api.Models.Auth;

/// <summary>
/// Požadavek na přihlášení uživatele
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// Email uživatele pro Cookidoo
    /// </summary>
    [Required(ErrorMessage = "Email je povinný")]
    [EmailAddress(ErrorMessage = "Neplatný formát emailu")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Heslo uživatele pro Cookidoo
    /// </summary>
    [Required(ErrorMessage = "Heslo je povinné")]
    [MinLength(1, ErrorMessage = "Heslo nemůže být prázdné")]
    public string Password { get; set; } = string.Empty;
} 