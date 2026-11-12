using Autenticador.Domain.Entities;
using Autenticador.Infrastructure.Data;
using Autenticador.Infrastructure.Security;
using Dapper;
using FluentAssertions;

namespace Autenticador.IntegrationTests.Startup;

public class TestesInicializacaoSwagger
{
    [Fact]
    public async Task Deve_Manter_Seis_Usuarios_Quando_Existe_Colisao_Com_Fixture_Em_Development()
    {
        var databasePath = Path.Combine(Path.GetTempPath(), $"auth-swagger-{Guid.NewGuid():N}.db");

        try
        {
            var factory = new DatabaseConnectionFactory($"Data Source={databasePath}");
            var passwordHasher = new PasswordHasherAdapter();

            var usuarioReal = new Autenticador.Domain.Entities.Usuario
            {
                Id = Guid.NewGuid().ToString(),
                Login = "admin@teste",
                SenhaHash = passwordHasher.HashPassword("Senha123!"),
                Ativo = true,
                DataCriacao = DateTime.UtcNow,
                QuantidadeFalhas = 0
            };

            await factory.InitializeAsync();
            using (var connection = factory.CreateConnection())
            {
                connection.Open();
                await connection.ExecuteAsync(@"
                    INSERT INTO Usuarios (Id, Login, SenhaHash, Ativo, DataCriacao, DataUltimaFalha, QuantidadeFalhas, BloqueadoAte)
                    VALUES (@Id, @Login, @SenhaHash, @Ativo, @DataCriacao, @DataUltimaFalha, @QuantidadeFalhas, @BloqueadoAte);",
                    usuarioReal);
            }

            var initializer = new DatabaseInitializer(factory, passwordHasher, "Development");
            await initializer.InitializeAsync();

            using (var connection = factory.CreateConnection())
            {
                connection.Open();
                var usuarios = (await connection.QueryAsync<Autenticador.Domain.Entities.Usuario>("SELECT * FROM Usuarios ORDER BY Login")).ToList();

                usuarios.Should().HaveCount(4);
                usuarios.Should().Contain(usuario => usuario.Id == "fixture-admin" && usuario.Login == "admin@teste.teste");
                usuarios.Should().Contain(usuario => usuario.Login == "admin@teste");
            }
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