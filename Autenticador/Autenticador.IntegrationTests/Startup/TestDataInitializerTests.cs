using Autenticador.IntegrationTests.Fixtures;
using Autenticador.Infrastructure.Data;
using Autenticador.Infrastructure.Repositories;
using Autenticador.Infrastructure.Security;
using FluentAssertions;

namespace Autenticador.IntegrationTests.Startup;

public class TestDataInitializerTests
{
    [Fact]
    public async Task Deve_Criar_Tres_Contas_De_Forma_Idempotente_Em_Testing()
    {
        var databasePath = Path.Combine(Path.GetTempPath(), $"auth-seed-{Guid.NewGuid():N}.db");
        try
        {
            var factory = new DatabaseConnectionFactory($"Data Source={databasePath}");
            var initializer = new DatabaseInitializer(factory, new PasswordHasherAdapter(), "Testing");

            for (var initialization = 0; initialization < 5; initialization++)
            {
                await initializer.InitializeAsync();
            }

            var users = (await new UsuarioRepository(factory).ListarAsync()).ToArray();
            users.Should().HaveCount(3);
            users.Select(usuario => usuario.Login)
                .Should().BeEquivalentTo(TestAccountsFixture.Accounts.Select(account => account.Login));
            foreach (var account in TestAccountsFixture.Accounts)
            {
                users.Single(usuario => usuario.Login == account.Login)
                    .Ativo.Should().Be(account.Active);
            }

            users.Should().OnlyContain(usuario => usuario.SenhaHash.Contains(':'));
        }
        finally
        {
            if (File.Exists(databasePath))
            {
                File.Delete(databasePath);
            }
        }
    }

    [Fact]
    public async Task Deve_Ignorar_Seed_Em_Producao()
    {
        var databasePath = Path.Combine(Path.GetTempPath(), $"auth-production-{Guid.NewGuid():N}.db");
        try
        {
            var factory = new DatabaseConnectionFactory($"Data Source={databasePath}");
            var initializer = new DatabaseInitializer(factory, new PasswordHasherAdapter(), "Production");

            await initializer.InitializeAsync();

            var users = await new UsuarioRepository(factory).ListarAsync();
            users.Should().BeEmpty();
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
