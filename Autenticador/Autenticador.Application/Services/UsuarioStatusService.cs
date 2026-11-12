using Autenticador.Application.Interfaces;
using Autenticador.Domain.Models;

namespace Autenticador.Application.Services;

public class UsuarioStatusService
{
    private readonly IUsuarioRepository _usuarioRepository;

    public UsuarioStatusService(IUsuarioRepository usuarioRepository)
    {
        _usuarioRepository = usuarioRepository;
    }

    public async Task<ResultadoConsultaLogin> ConsultarStatusAsync(string login)
    {
        if (string.IsNullOrWhiteSpace(login))
        {
            return new ResultadoConsultaLogin { Existe = false, Ativa = false };
        }

        return await _usuarioRepository.ObterStatusAsync(login.Trim());
    }
}
