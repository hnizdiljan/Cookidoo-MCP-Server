using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Cookidoo.MCP.Core.Entities;
using Cookidoo.MCP.Core.Interfaces;
using Cookidoo.MCP.Core.Exceptions;
using Cookidoo.MCP.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;

namespace Cookidoo.MCP.Infrastructure.Services;

/// <summary>
/// Implementace autentizační služby podle cookidoo-api-master
/// </summary>
public class AuthService : IAuthService
{
    private readonly ILogger<AuthService> _logger;
    private readonly ICookidooApiService _cookidooApiService;
    private readonly CookidooAuthService _cookidooAuthService;
    private readonly JwtOptions _jwtOptions;

    public AuthService(
        ILogger<AuthService> logger,
        ICookidooApiService cookidooApiService,
        CookidooAuthService cookidooAuthService,
        IOptions<JwtOptions> jwtOptions)
    {
        _logger = logger;
        _cookidooApiService = cookidooApiService;
        _cookidooAuthService = cookidooAuthService;
        _jwtOptions = jwtOptions.Value;
    }

    /// <summary>
    /// Přihlásí uživatele pomocí Cookidoo přihlašovacích údajů podle cookidoo-api-master
    /// </summary>
    public async Task<AuthResult> LoginAsync(string email, string password)
    {
        try
        {
            _logger.LogInformation("Začíná OAuth2 přihlašování uživatele {Email} podle cookidoo-api-master", email);

            // Přihlášení pomocí OAuth2 Cookidoo API podle cookidoo-api-master
            var cookidooAuthResponse = await _cookidooAuthService.LoginAsync(email, password);
            
            if (cookidooAuthResponse == null || string.IsNullOrEmpty(cookidooAuthResponse.AccessToken))
            {
                _logger.LogWarning("Neplatné přihlašovací údaje pro uživatele {Email}", email);
                return AuthResult.Failure("Neplatné přihlašovací údaje");
            }

            // Získá informace o uživateli pomocí access tokenu
            var userInfo = await _cookidooAuthService.GetUserInfoAsync(cookidooAuthResponse.AccessToken);
            
            if (userInfo == null)
            {
                _logger.LogWarning("Nepodařilo se získat informace o uživateli {Email}", email);
                return AuthResult.Failure("Nepodařilo se ověřit uživatele");
            }
            
            // Vygeneruje JWT token pro MCP API s Cookidoo access tokenem
            var mcpToken = GenerateJwtToken(cookidooAuthResponse.Sub, email, cookidooAuthResponse.AccessToken);
            var expiresAt = DateTime.UtcNow.AddMinutes(_jwtOptions.ExpirationMinutes);

            _logger.LogInformation("Uživatel {Email} byl úspěšně přihlášen pomocí OAuth2", email);

            return AuthResult.Success(mcpToken, cookidooAuthResponse.AccessToken, cookidooAuthResponse.Sub, email, expiresAt);
        }
        catch (CookidooAuthenticationException ex)
        {
            _logger.LogWarning(ex, "Chyba OAuth2 autentizace pro uživatele {Email}", email);
            return AuthResult.Failure("Neplatné přihlašovací údaje");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Neočekávaná chyba při OAuth2 přihlašování uživatele {Email}", email);
            return AuthResult.Failure("Došlo k neočekávané chybě");
        }
    }

    /// <summary>
    /// Ověří platnost Cookidoo access tokenu podle cookidoo-api-master
    /// </summary>
    public async Task<bool> ValidateCookidooTokenAsync(string cookidooToken)
    {
        try
        {
            return await _cookidooAuthService.ValidateTokenAsync(cookidooToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Chyba při ověřování Cookidoo access tokenu");
            return false;
        }
    }

    /// <summary>
    /// Vygeneruje JWT token pro MCP API
    /// </summary>
    public string GenerateJwtToken(string userId, string email, string? cookidooToken = null)
    {
        var key = Encoding.UTF8.GetBytes(_jwtOptions.SecretKey);
        var tokenHandler = new JwtSecurityTokenHandler();

        var claimsList = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Email, email),
            new Claim(JwtRegisteredClaimNames.Sub, userId),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, 
                new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(), 
                ClaimValueTypes.Integer64)
        };

        // Přidání Cookidoo access tokenu pokud je k dispozici
        if (!string.IsNullOrEmpty(cookidooToken))
        {
            claimsList.Add(new Claim("cookidoo_token", cookidooToken));
        }

        var claims = claimsList.ToArray();

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_jwtOptions.ExpirationMinutes),
            Issuer = _jwtOptions.Issuer,
            Audience = _jwtOptions.Audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key), 
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    /// <summary>
    /// Obnoví JWT token
    /// </summary>
    public Task<string> RefreshTokenAsync(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtOptions.SecretKey);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _jwtOptions.Issuer,
                ValidateAudience = true,
                ValidAudience = _jwtOptions.Audience,
                ValidateLifetime = false, // Ignorujeme expiraci pro refresh
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
            
            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var email = principal.FindFirst(ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(email))
            {
                throw new SecurityTokenException("Token neobsahuje potřebné claims");
            }

            return Task.FromResult(GenerateJwtToken(userId, email));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Chyba při obnovování tokenu");
            throw;
        }
    }

    /// <summary>
    /// Odhlásí uživatele z Cookidoo API podle cookidoo-api-master
    /// </summary>
    public async Task<bool> LogoutFromCookidooAsync(string cookidooToken)
    {
        try
        {
            return await _cookidooAuthService.LogoutAsync(cookidooToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Chyba při odhlašování z Cookidoo API");
            return false;
        }
    }
} 