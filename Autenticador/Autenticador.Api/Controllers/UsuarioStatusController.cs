using System.Security.Cryptography;
using System.Text;
using Autenticador.Api.Extensions;
using Autenticador.Api.Models;
using Autenticador.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Autenticador.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class UsuarioStatusController : ControllerBase
{
    private readonly UsuarioStatusService _usuarioStatusService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<UsuarioStatusController> _logger;

    public UsuarioStatusController(
        UsuarioStatusService usuarioStatusService,
        IConfiguration configuration,
        ILogger<UsuarioStatusController> logger)
    {
        _usuarioStatusService = usuarioStatusService;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpGet("usuarios/{login}/status")]
    public async Task<ActionResult<UsuarioStatusResponse>> GetStatus(string login)
    {
        var headerValue = Request.Headers["X-Service-Credential"].ToString();
        var expected = _configuration["ServiceCredential:Secret"];
        if (string.IsNullOrWhiteSpace(headerValue)
            || string.IsNullOrWhiteSpace(expected)
            || !CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(headerValue),
                Encoding.UTF8.GetBytes(expected)))
        {
            _logger.LogWarning("Status query was rejected due to invalid service credential.");
            return Unauthorized(ProblemDetailsExtensions.CreateUnauthorizedProblem("Credencial de serviço inválida."));
        }

        var status = await _usuarioStatusService.ConsultarStatusAsync(login);
        return Ok(new UsuarioStatusResponse
        {
            Existe = status.Existe,
            Ativa = status.Ativa
        });
    }
}
