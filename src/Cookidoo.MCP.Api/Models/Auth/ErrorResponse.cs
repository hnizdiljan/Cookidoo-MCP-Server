using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Cookidoo.MCP.Api.Models.Auth;

/// <summary>
/// Standardní chybová odpověď
/// </summary>
public class ErrorResponse
{
    /// <summary>
    /// Chybová zpráva
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// Detaily chyby (např. validační chyby)
    /// </summary>
    public Dictionary<string, string[]>? Details { get; set; }

    /// <summary>
    /// Časové razítko chyby
    /// </summary>
    public DateTime Timestamp { get; set; }

    public ErrorResponse(string message)
    {
        Message = message;
        Timestamp = DateTime.UtcNow;
    }

    public ErrorResponse(string message, ModelStateDictionary modelState) : this(message)
    {
        Details = modelState
            .Where(x => x.Value?.Errors.Count > 0)
            .ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
            );
    }
} 