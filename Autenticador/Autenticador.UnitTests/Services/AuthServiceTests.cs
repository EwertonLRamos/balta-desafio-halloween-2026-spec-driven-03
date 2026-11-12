using Autenticador.Application.Services;
using Autenticador.Domain.Entities;
using Autenticador.Infrastructure.Data;
using Autenticador.Infrastructure.Repositories;
using Autenticador.Infrastructure.Security;
using FluentAssertions;
using Microsoft.Extensions.Options;

namespace Autenticador.UnitTests.Services;

public class AuthServiceTests
{
    [Fact]
    public async Task Deve_Emitir_Token_Para_Credenciais_Validas()
    {
        var (service, _) = await CreateServiceAsync();

        var result = await service.AutenticarAsync(new LoginRequest
        {
            Login = "admin@teste",
            Senha = "Senha123!"
        });

        result.Token.Should().NotBeNullOrWhiteSpace();
        result.ExpiraEm.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task Deve_Rejeitar_Credenciais_Invalidas_Sem_Emitir_Token()
    {
        var (service, _) = await CreateServiceAsync();

        var action = () => service.AutenticarAsync(new LoginRequest
        {
            Login = "admin@teste",
            Senha = "senha-errada"
        });

        await action.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Deve_Rejeitar_Usuario_Inativo()
    {
        var (service, _) = await CreateServiceAsync(usuarioAtivo: false);

        var action = () => service.AutenticarAsync(new LoginRequest
        {
            Login = "admin@teste",
            Senha = "Senha123!"
        });

        await action.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Credenciais inválidas.");
    }

    [Fact]
    public async Task Deve_Bloquear_Temporariamente_Conta_Apos_Falhas_Consecutivas()
    {
        var (service, repository) = await CreateServiceAsync();

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.AutenticarAsync(new LoginRequest { Login = "admin@teste", Senha = "senha-errada" }));

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.AutenticarAsync(new LoginRequest { Login = "admin@teste", Senha = "senha-errada" }));

        var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.AutenticarAsync(new LoginRequest { Login = "admin@teste", Senha = "senha-errada" }));

        ex.Message.Should().NotBeNullOrEmpty();

        var usuario = await repository.GetByLoginAsync("admin@teste");
        usuario.Should().NotBeNull();
        usuario!.BloqueadoAte.Should().NotBeNull();
    }

    [Fact]
    public async Task Deve_Restabelecer_Contador_Apos_Expiracao_Do_Bloqueio()
    {
        var (service, repository) = await CreateServiceAsync();
        var usuario = await repository.GetByLoginAsync("admin@teste");
        usuario.Should().NotBeNull();
        usuario!.QuantidadeFalhas = 3;
        usuario.DataUltimaFalha = DateTime.UtcNow.AddMinutes(-6);
        usuario.BloqueadoAte = DateTime.UtcNow.AddSeconds(-1);
        await repository.AtualizarAsync(usuario);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.AutenticarAsync(new LoginRequest { Login = "admin@teste", Senha = "senha-errada" }));

        usuario = await repository.GetByLoginAsync("admin@teste");
        usuario.Should().NotBeNull();
        usuario!.QuantidadeFalhas.Should().Be(1);
        usuario.BloqueadoAte.Should().BeNull();
    }

    private static async Task<(AuthService service, UsuarioRepository repository)> CreateServiceAsync(bool usuarioAtivo = true)
    {
        var databasePath = Path.Combine(Path.GetTempPath(), $"auth-{Guid.NewGuid():N}.db");
        var factory = new DatabaseConnectionFactory($"Data Source={databasePath}");
        await factory.InitializeAsync();

        var passwordHasher = new PasswordHasherAdapter();
        var usuarioRepository = new UsuarioRepository(factory);
        await usuarioRepository.SalvarAsync(new Usuario
        {
            Id = Guid.NewGuid().ToString(),
            Login = "admin@teste",
            SenhaHash = passwordHasher.HashPassword("Senha123!"),
            Ativo = usuarioAtivo,
            DataCriacao = DateTime.UtcNow,
            QuantidadeFalhas = 0
        });

        var jwtFactory = new JwtTokenFactory(Options.Create(new JwtOptions
        {
            Key = "c2c5d363-7a3f-4b5a-8d7e-b1f4416b2d10-456789012345",
            Issuer = "Autenticador.Api",
            Audience = "Autenticador.Clientes",
            ExpiresInMinutes = 60
        }));

        return (new AuthService(usuarioRepository, passwordHasher, jwtFactory), usuarioRepository);
    }
}
