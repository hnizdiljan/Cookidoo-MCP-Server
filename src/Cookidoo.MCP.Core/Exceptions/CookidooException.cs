namespace Cookidoo.MCP.Core.Exceptions;

/// <summary>
/// Základní výjimka pro Cookidoo MCP operace
/// </summary>
public class CookidooException : Exception
{
    public CookidooException() : base() { }

    public CookidooException(string message) : base(message) { }

    public CookidooException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Výjimka pro autentizační problémy
/// </summary>
public class CookidooAuthenticationException : CookidooException
{
    public CookidooAuthenticationException() : base("Autentizace s Cookidoo API selhala") { }

    public CookidooAuthenticationException(string message) : base(message) { }

    public CookidooAuthenticationException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Výjimka pro autorizační problémy
/// </summary>
public class CookidooAuthorizationException : CookidooException
{
    public CookidooAuthorizationException() : base("Nemáte oprávnění k této operaci") { }

    public CookidooAuthorizationException(string message) : base(message) { }

    public CookidooAuthorizationException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Výjimka pro nenalezené zdroje
/// </summary>
public class CookidooNotFoundException : CookidooException
{
    public CookidooNotFoundException() : base("Požadovaný zdroj nebyl nalezen") { }

    public CookidooNotFoundException(string message) : base(message) { }

    public CookidooNotFoundException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Výjimka pro validační chyby
/// </summary>
public class CookidooValidationException : CookidooException
{
    public List<string> ValidationErrors { get; }

    public CookidooValidationException(List<string> validationErrors) 
        : base($"Validace selhala: {string.Join(", ", validationErrors)}")
    {
        ValidationErrors = validationErrors;
    }

    public CookidooValidationException(string validationError) 
        : base($"Validace selhala: {validationError}")
    {
        ValidationErrors = new List<string> { validationError };
    }
}

/// <summary>
/// Výjimka pro API komunikační problémy
/// </summary>
public class CookidooApiException : CookidooException
{
    public int? StatusCode { get; }
    public string? ResponseContent { get; }

    public CookidooApiException(int statusCode, string? responseContent = null) 
        : base($"Cookidoo API vrátilo chybu {statusCode}: {responseContent}")
    {
        StatusCode = statusCode;
        ResponseContent = responseContent;
    }

    public CookidooApiException(string message, int? statusCode = null, string? responseContent = null) 
        : base(message)
    {
        StatusCode = statusCode;
        ResponseContent = responseContent;
    }

    public CookidooApiException(string message, Exception innerException, int? statusCode = null, string? responseContent = null) 
        : base(message, innerException)
    {
        StatusCode = statusCode;
        ResponseContent = responseContent;
    }
} 