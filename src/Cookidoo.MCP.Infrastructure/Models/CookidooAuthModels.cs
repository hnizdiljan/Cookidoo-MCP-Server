using System.Text.Json.Serialization;

namespace Cookidoo.MCP.Infrastructure.Models;

/// <summary>
/// Request pro Cookidoo autentizaci
/// </summary>
public class CookidooAuthRequest
{
    [JsonPropertyName("grant_type")]
    public string GrantType { get; set; } = "password";

    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;

    [JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;

    [JsonPropertyName("client_id")]
    public string ClientId { get; set; } = string.Empty;
}

/// <summary>
/// Response z Cookidoo autentizace podle cookidoo-api-master implementace
/// </summary>
public class CookidooAuthResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; } = string.Empty;

    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = string.Empty;

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    /// <summary>
    /// Subject (uživatelské ID) podle cookidoo-api-master
    /// </summary>
    [JsonPropertyName("sub")]
    public string Sub { get; set; } = string.Empty;
}

/// <summary>
/// Request pro refresh token
/// </summary>
public class CookidooRefreshRequest
{
    [JsonPropertyName("grant_type")]
    public string GrantType { get; set; } = "refresh_token";

    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; } = string.Empty;

    [JsonPropertyName("client_id")]
    public string ClientId { get; set; } = string.Empty;
}

/// <summary>
/// Model pro informace o uživateli z Cookidoo API podle cookidoo-api-master
/// </summary>
public class CookidooUserInfo
{
    /// <summary>
    /// Uživatelské jméno
    /// </summary>
    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Popis uživatele
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// URL obrázku profilu
    /// </summary>
    [JsonPropertyName("picture")]
    public string? Picture { get; set; }
}

/// <summary>
/// Wrapper pro response s user info z community/profile endpointu podle cookidoo-api-master
/// </summary>
public class CookidooProfileResponse
{
    /// <summary>
    /// Informace o uživateli
    /// </summary>
    [JsonPropertyName("userInfo")]
    public CookidooUserInfo UserInfo { get; set; } = new();
}

/// <summary>
/// Model pro lokalizační konfiguraci podle cookidoo-api-master
/// </summary>
public class CookidooLocalizationConfig
{
    /// <summary>
    /// Kód země (např. "ch", "de", "us")
    /// </summary>
    public string CountryCode { get; set; } = "ch";

    /// <summary>
    /// Jazyk (např. "de-CH", "en-US")
    /// </summary>
    public string Language { get; set; } = "de-CH";

    /// <summary>
    /// URL podle regionu
    /// </summary>
    public string Url { get; set; } = "https://cookidoo.ch/foundation/de-CH";
}

 