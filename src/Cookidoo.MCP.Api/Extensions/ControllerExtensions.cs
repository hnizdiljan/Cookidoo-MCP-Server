using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace Cookidoo.MCP.Api.Extensions;

/// <summary>
/// Extension metody pro controllery
/// </summary>
public static class ControllerExtensions
{
    /// <summary>
    /// Získá Cookidoo JWT token z Authorization headeru nebo query parametru
    /// </summary>
    /// <param name="controller">Controller instance</param>
    /// <returns>Cookidoo JWT token nebo null</returns>
    public static string? GetCookidooToken(this ControllerBase controller)
    {
        // Zkusíme najít token v Authorization headeru
        var authHeader = controller.Request.Headers.Authorization.FirstOrDefault();
        if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return authHeader.Substring("Bearer ".Length).Trim();
        }

        // Zkusíme najít token v query parametru
        var queryToken = controller.Request.Query["jwt_token"].FirstOrDefault();
        if (!string.IsNullOrEmpty(queryToken))
        {
            return queryToken;
        }

        // Zkusíme najít token v headeru jwt_token
        var headerToken = controller.Request.Headers["jwt_token"].FirstOrDefault();
        if (!string.IsNullOrEmpty(headerToken))
        {
            return headerToken;
        }

        return null;
    }

    /// <summary>
    /// Získá ID uživatele z Cookidoo tokenu (tuto funkci bude možné implementovat později pokud bude potřeba)
    /// </summary>
    /// <param name="controller">Controller instance</param>
    /// <returns>ID uživatele nebo null</returns>
    public static string? GetUserId(this ControllerBase controller)
    {
        var token = controller.GetCookidooToken();
        if (string.IsNullOrEmpty(token))
            return null;

        // TODO: Implementovat dekódování JWT tokenu pro získání user ID
        // Pro teď vracíme dummy hodnotu
        return "cookidoo_user";
    }

    /// <summary>
    /// Získá email uživatele z Cookidoo tokenu (tuto funkci bude možné implementovat později pokud bude potřeba)
    /// </summary>
    /// <param name="controller">Controller instance</param>
    /// <returns>Email uživatele nebo null</returns>
    public static string? GetUserEmail(this ControllerBase controller)
    {
        var token = controller.GetCookidooToken();
        if (string.IsNullOrEmpty(token))
            return null;

        // TODO: Implementovat dekódování JWT tokenu pro získání emailu
        // Pro teď vracíme dummy hodnotu
        return "user@cookidoo.com";
    }
} 