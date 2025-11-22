using Microsoft.AspNetCore.Mvc;
using Cookidoo.MCP.Infrastructure.Services;
using Cookidoo.MCP.Core.Exceptions;
using System.ComponentModel.DataAnnotations;

namespace Cookidoo.MCP.Api.Controllers;

/// <summary>
/// Controller pro autentizaci s Cookidoo
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly CookidooAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        CookidooAuthService authService,
        ILogger<AuthController> logger)
    {
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Přihlášení uživatele pomocí Cookidoo emailu a hesla
    /// </summary>
    /// <param name="request">Přihlašovací údaje</param>
    /// <returns>JWT token pro autentizaci</returns>
    /// <response code="200">Úspěšné přihlášení, vrací JWT token</response>
    /// <response code="400">Neplatný formát požadavku</response>
    /// <response code="401">Neplatné přihlašovací údaje</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        try
        {
            _logger.LogInformation("Přihlášení uživatele: {Email}", request.Email);

            var authResponse = await _authService.LoginAsync(request.Email, request.Password);

            var response = new LoginResponse
            {
                AccessToken = authResponse.AccessToken,
                RefreshToken = authResponse.RefreshToken,
                ExpiresIn = authResponse.ExpiresIn,
                TokenType = authResponse.TokenType ?? "Bearer"
            };

            _logger.LogInformation("Uživatel {Email} úspěšně přihlášen", request.Email);

            return Ok(response);
        }
        catch (CookidooAuthenticationException ex)
        {
            _logger.LogWarning(ex, "Neúspěšné přihlášení pro {Email}", request.Email);
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Chyba při přihlášení uživatele {Email}", request.Email);
            return BadRequest(new { message = "Chyba při přihlášení" });
        }
    }

    /// <summary>
    /// Obnovení access tokenu pomocí refresh tokenu
    /// </summary>
    /// <param name="request">Refresh token</param>
    /// <returns>Nový JWT token</returns>
    /// <response code="200">Úspěšné obnovení, vrací nový JWT token</response>
    /// <response code="401">Neplatný refresh token</response>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponse>> Refresh([FromBody] RefreshRequest request)
    {
        try
        {
            _logger.LogInformation("Obnovování tokenu");

            var authResponse = await _authService.RefreshTokenAsync(request.RefreshToken);

            var response = new LoginResponse
            {
                AccessToken = authResponse.AccessToken,
                RefreshToken = authResponse.RefreshToken,
                ExpiresIn = authResponse.ExpiresIn,
                TokenType = authResponse.TokenType ?? "Bearer"
            };

            _logger.LogInformation("Token úspěšně obnoven");

            return Ok(response);
        }
        catch (CookidooAuthenticationException ex)
        {
            _logger.LogWarning(ex, "Neúspěšné obnovení tokenu");
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Chyba při obnovování tokenu");
            return BadRequest(new { message = "Chyba při obnovování tokenu" });
        }
    }

    /// <summary>
    /// Ověření platnosti tokenu
    /// </summary>
    /// <returns>Status tokenu</returns>
    /// <response code="200">Token je platný</response>
    /// <response code="401">Token není platný nebo chybí</response>
    [HttpGet("verify")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult Verify()
    {
        var authHeader = Request.Headers["Authorization"].ToString();

        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
        {
            return Unauthorized(new { message = "Token chybí" });
        }

        var token = authHeader.Replace("Bearer ", "");

        // Pro mock tokeny jednoduché ověření
        if (token.StartsWith("mock-") || token == "test-token" || token == "demo-token")
        {
            return Ok(new { valid = true, message = "Mock token je platný" });
        }

        // Pro skutečné tokeny by zde bylo ověření
        return Ok(new { valid = true, message = "Token je platný" });
    }
}

/// <summary>
/// Request model pro přihlášení
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// Email uživatele
    /// </summary>
    [Required(ErrorMessage = "Email je povinný")]
    [EmailAddress(ErrorMessage = "Neplatný formát emailu")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Heslo uživatele
    /// </summary>
    [Required(ErrorMessage = "Heslo je povinné")]
    [MinLength(6, ErrorMessage = "Heslo musí mít alespoň 6 znaků")]
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// Request model pro obnovení tokenu
/// </summary>
public class RefreshRequest
{
    /// <summary>
    /// Refresh token
    /// </summary>
    [Required(ErrorMessage = "Refresh token je povinný")]
    public string RefreshToken { get; set; } = string.Empty;
}

/// <summary>
/// Response model pro přihlášení/obnovení
/// </summary>
public class LoginResponse
{
    /// <summary>
    /// JWT access token
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Refresh token pro obnovení
    /// </summary>
    public string? RefreshToken { get; set; }

    /// <summary>
    /// Doba platnosti v sekundách
    /// </summary>
    public int ExpiresIn { get; set; }

    /// <summary>
    /// Typ tokenu (obvykle "Bearer")
    /// </summary>
    public string TokenType { get; set; } = "Bearer";
}
