using Autenticador.Domain.Entities;
using Autenticador.Infrastructure.Data;
using Autenticador.Infrastructure.Repositories;
using Autenticador.Infrastructure.Security;
using FluentAssertions;

namespace Autenticador.UnitTests.Services;

public class TestesRepositorioUsuario
{
    [Fact]
    public async Task Deve_Manter_Conta_Real_Quando_Login_Da_Fixture_Colide()
    {
        var databasePath = Path.Combine(Path.GetTempPath(), $"auth-collision-{Guid.NewGuid():N}.db");

        try
        {
            var factory = new DatabaseConnectionFactory($"Data Source={databasePath}");
            await factory.InitializeAsync();

            var passwordHasher = new PasswordHasherAdapter();
            var repository = new UsuarioRepository(factory);

            await repository.SalvarAsync(new Usuario
            {
                Id = Guid.NewGuid().ToString(),
                Login = "admin@teste",
                SenhaHash = passwordHasher.HashPassword("Senha123!"),
                Ativo = true,
                DataCriacao = DateTime.UtcNow,
                QuantidadeFalhas = 0
            });

            var initializer = new DatabaseInitializer(factory, passwordHasher, "Testing");
            await initializer.InitializeAsync();

            var usuarios = (await repository.ListarAsync()).ToList();
            usuarios.Should().ContainSingle(usuario => usuario.Login.Equals("admin@teste", StringComparison.OrdinalIgnoreCase));
            usuarios.Should().Contain(usuario => usuario.Id == "fixture-admin" && usuario.Login.Equals("admin@teste.teste", StringComparison.OrdinalIgnoreCase));
        }
        finally
        {
            DatabaseConnectionFactory.ClearAllPools();
            if (File.Exists(databasePath))
            {
                File.Delete(databasePath);
            }
        }
    }
}