using Autenticador.Api.Extensions;
using Autenticador.Api.Models;
using Autenticador.Application.Services;
using Microsoft.AspNetCore.Mvc;
using LoginRequest = Autenticador.Api.Models.LoginRequest;
using LoginResponse = Autenticador.Api.Models.LoginResponse;

namespace Autenticador.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(AuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(new ValidationProblemDetails(ModelState));
        }

        try
        {
            var result = await _authService.AutenticarAsync(new Application.Services.LoginRequest
            {
                Login = request.Login,
                Senha = request.Senha
            });

            return Ok(new LoginResponse
            {
                Token = result.Token,
                ExpiraEm = result.ExpiraEm
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Authentication request was rejected.");
            return Unauthorized(ProblemDetailsExtensions.CreateUnauthorizedProblem(ex.Message));
        }
    }
}
