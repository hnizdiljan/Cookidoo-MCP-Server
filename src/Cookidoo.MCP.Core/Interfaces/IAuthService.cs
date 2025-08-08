using Cookidoo.MCP.Core.Entities;

namespace Cookidoo.MCP.Core.Interfaces;

/// <summary>
/// Interface pro autentizační služby
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Přihlásí uživatele pomocí Cookidoo přihlašovacích údajů
    /// </summary>
    /// <param name="email">Email uživatele</param>
    /// <param name="password">Heslo uživatele</param>
    /// <returns>Výsledek přihlášení</returns>
    Task<AuthResult> LoginAsync(string email, string password);

    /// <summary>
    /// Ověří platnost Cookidoo tokenu
    /// </summary>
    /// <param name="cookidooToken">Token z Cookidoo</param>
    /// <returns>True pokud je token platný</returns>
    Task<bool> ValidateCookidooTokenAsync(string cookidooToken);

    /// <summary>
    /// Vygeneruje JWT token pro MCP API
    /// </summary>
    /// <param name="userId">ID uživatele</param>
    /// <param name="email">Email uživatele</param>
    /// <param name="cookidooToken">Volitelný Cookidoo token</param>
    /// <returns>JWT token</returns>
    string GenerateJwtToken(string userId, string email, string? cookidooToken = null);

    /// <summary>
    /// Obnoví JWT token
    /// </summary>
    /// <param name="token">Starý token</param>
    /// <returns>Nový token</returns>
    Task<string> RefreshTokenAsync(string token);

    /// <summary>
    /// Odhlásí uživatele z Cookidoo API
    /// </summary>
    /// <param name="cookidooToken">Cookidoo token</param>
    /// <returns>True pokud bylo odhlášení úspěšné</returns>
    Task<bool> LogoutFromCookidooAsync(string cookidooToken);
} 