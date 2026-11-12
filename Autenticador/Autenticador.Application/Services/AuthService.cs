using Autenticador.Application.Interfaces;
using Autenticador.Domain.Entities;

namespace Autenticador.Application.Services;

public class AuthService
{
    private const int MaxFalhasConsecutivas = 3;
    private static readonly TimeSpan DuracaoBloqueio = TimeSpan.FromMinutes(5);

    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenFactory _jwtTokenFactory;
    private readonly int _duracaoTokenEmMinutos;

    public AuthService(
        IUsuarioRepository usuarioRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenFactory jwtTokenFactory,
        int duracaoTokenEmMinutos = 60)
    {
        if (duracaoTokenEmMinutos <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(duracaoTokenEmMinutos));
        }

        _usuarioRepository = usuarioRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenFactory = jwtTokenFactory;
        _duracaoTokenEmMinutos = duracaoTokenEmMinutos;
    }

    public async Task<LoginResponse> AutenticarAsync(LoginRequest request)
    {
        if (request is null)
        {
            throw new UnauthorizedAccessException("Credenciais inválidas.");
        }

        var login = request.Login?.Trim();
        var senha = request.Senha;

        if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(senha))
        {
            throw new UnauthorizedAccessException("Credenciais inválidas.");
        }

        var usuario = await _usuarioRepository.GetByLoginAsync(login);
        if (usuario is null || !usuario.Ativo)
        {
            throw new UnauthorizedAccessException("Credenciais inválidas.");
        }

        var agora = DateTime.UtcNow;
        if (usuario.BloqueadoAte is not null && usuario.BloqueadoAte > agora)
        {
            throw new UnauthorizedAccessException("Credenciais inválidas.");
        }

        if (usuario.BloqueadoAte is not null)
        {
            usuario.BloqueadoAte = null;
            usuario.DataUltimaFalha = null;
            usuario.QuantidadeFalhas = 0;
        }

        if (!_passwordHasher.VerifyPassword(senha, usuario.SenhaHash))
        {
            usuario.DataUltimaFalha = agora;
            usuario.QuantidadeFalhas += 1;

            if (usuario.QuantidadeFalhas >= MaxFalhasConsecutivas)
            {
                usuario.BloqueadoAte = agora.Add(DuracaoBloqueio);
                usuario.QuantidadeFalhas = MaxFalhasConsecutivas;
            }

            await _usuarioRepository.AtualizarAsync(usuario);
            throw new UnauthorizedAccessException("Credenciais inválidas.");
        }

        if (usuario.BloqueadoAte is not null && usuario.BloqueadoAte <= DateTime.UtcNow)
        {
            usuario.BloqueadoAte = null;
        }

        usuario.DataUltimaFalha = null;
        usuario.QuantidadeFalhas = 0;
        usuario.BloqueadoAte = null;
        await _usuarioRepository.AtualizarAsync(usuario);

        var expiraEm = agora.AddMinutes(_duracaoTokenEmMinutos);
        var token = _jwtTokenFactory.CreateToken(usuario.Login, expiraEm);

        return new LoginResponse
        {
            Token = token,
            ExpiraEm = expiraEm
        };
    }
}
