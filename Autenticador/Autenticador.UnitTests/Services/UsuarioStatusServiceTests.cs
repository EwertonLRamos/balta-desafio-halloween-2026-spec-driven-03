using Autenticador.Application.Services;
using Autenticador.Domain.Entities;
using Autenticador.Infrastructure.Data;
using Autenticador.Infrastructure.Repositories;
using FluentAssertions;

namespace Autenticador.UnitTests.Services;

public class UsuarioStatusServiceTests
{
    [Fact]
    public async Task Deve_Mapear_Usuarios_Ativos_Inativos_E_Inexistentes()
    {
        var databasePath = Path.Combine(Path.GetTempPath(), $"auth-status-{Guid.NewGuid():N}.db");
        try
        {
            var factory = new DatabaseConnectionFactory($"Data Source={databasePath}");
            await factory.InitializeAsync();
            var repository = new UsuarioRepository(factory);
            await repository.SalvarAsync(new Usuario
            {
                Id = Guid.NewGuid().ToString(),
                Login = "ativo@teste",
                SenhaHash = "hash",
                Ativo = true,
                DataCriacao = DateTime.UtcNow
            });
            await repository.SalvarAsync(new Usuario
            {
                Id = Guid.NewGuid().ToString(),
                Login = "inativo@teste",
                SenhaHash = "hash",
                Ativo = false,
                DataCriacao = DateTime.UtcNow
            });

            var service = new UsuarioStatusService(repository);

            (await service.ConsultarStatusAsync("ativo@teste"))
                .Should().BeEquivalentTo(new { Existe = true, Ativa = true });
            (await service.ConsultarStatusAsync("inativo@teste"))
                .Should().BeEquivalentTo(new { Existe = true, Ativa = false });
            (await service.ConsultarStatusAsync("inexistente@teste"))
                .Should().BeEquivalentTo(new { Existe = false, Ativa = false });
        }
        finally
        {
            if (File.Exists(databasePath))
            {
                File.Delete(databasePath);
            }
        }
    }
}
