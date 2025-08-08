namespace Cookidoo.MCP.Core.Entities;

/// <summary>
/// Výsledek přihlášení uživatele
/// </summary>
public class AuthResult
{
    /// <summary>
    /// Zda bylo přihlášení úspěšné
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// JWT token pro MCP API
    /// </summary>
    public string? McpToken { get; set; }

    /// <summary>
    /// Token pro Cookidoo API
    /// </summary>
    public string? CookidooToken { get; set; }

    /// <summary>
    /// ID uživatele v Cookidoo
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// Email uživatele
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Doba platnosti MCP tokenu
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Chybová zpráva v případě neúspěchu
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Vytvoří úspěšný výsledek přihlášení
    /// </summary>
    public static AuthResult Success(string mcpToken, string cookidooToken, string userId, string email, DateTime expiresAt)
    {
        return new AuthResult
        {
            IsSuccess = true,
            McpToken = mcpToken,
            CookidooToken = cookidooToken,
            UserId = userId,
            Email = email,
            ExpiresAt = expiresAt
        };
    }

    /// <summary>
    /// Vytvoří neúspěšný výsledek přihlášení
    /// </summary>
    public static AuthResult Failure(string errorMessage)
    {
        return new AuthResult
        {
            IsSuccess = false,
            ErrorMessage = errorMessage
        };
    }
} 