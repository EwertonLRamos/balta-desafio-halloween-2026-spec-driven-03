using Autenticador.Application.Interfaces;
using Autenticador.Domain.Entities;
using Dapper;

namespace Autenticador.Infrastructure.Data;

public class DatabaseInitializer
{
    private readonly IDatabaseConnectionFactory _databaseConnectionFactory;
    private readonly IPasswordHasher _passwordHasher;
    private readonly string _environmentName;

    public DatabaseInitializer(
        IDatabaseConnectionFactory databaseConnectionFactory,
        IPasswordHasher passwordHasher,
        string environmentName)
    {
        _databaseConnectionFactory = databaseConnectionFactory;
        _passwordHasher = passwordHasher;
        _environmentName = environmentName;
    }

    public async Task InitializeAsync()
    {
        await _databaseConnectionFactory.InitializeAsync();

        if (!string.Equals(_environmentName, "Development", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(_environmentName, "Testing", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        using var connection = _databaseConnectionFactory.CreateConnection();
        connection.Open();

        var users = await connection.QueryAsync<Usuario>("SELECT * FROM Usuarios");
        var existingLogins = users.Select(x => x.Login).ToHashSet(StringComparer.OrdinalIgnoreCase);

        var seedUsers = new[]
        {
            new Usuario
            {
                Id = Guid.NewGuid().ToString(),
                Login = "admin@teste",
                SenhaHash = _passwordHasher.HashPassword("Senha123!"),
                Ativo = true,
                DataCriacao = DateTime.UtcNow,
                QuantidadeFalhas = 0
            },
            new Usuario
            {
                Id = Guid.NewGuid().ToString(),
                Login = "operador@teste",
                SenhaHash = _passwordHasher.HashPassword("Senha123!"),
                Ativo = true,
                DataCriacao = DateTime.UtcNow,
                QuantidadeFalhas = 0
            },
            new Usuario
            {
                Id = Guid.NewGuid().ToString(),
                Login = "inativo@teste",
                SenhaHash = _passwordHasher.HashPassword("Senha123!"),
                Ativo = false,
                DataCriacao = DateTime.UtcNow,
                QuantidadeFalhas = 0
            }
        };

        foreach (var usuario in seedUsers)
        {
            if (existingLogins.Contains(usuario.Login))
            {
                continue;
            }

            await connection.ExecuteAsync(@"
                INSERT INTO Usuarios (Id, Login, SenhaHash, Ativo, DataCriacao, DataUltimaFalha, QuantidadeFalhas, BloqueadoAte)
                VALUES (@Id, @Login, @SenhaHash, @Ativo, @DataCriacao, @DataUltimaFalha, @QuantidadeFalhas, @BloqueadoAte);",
                usuario);
        }

        var hasServiceCredential = await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM CredenciaisDeServico WHERE Nome = @Nome",
            new { Nome = "api-interna" });

        if (hasServiceCredential == 0)
        {
            await connection.ExecuteAsync(@"
                INSERT INTO CredenciaisDeServico (Id, Nome, ChaveSecreta, Ativa)
                VALUES (@Id, @Nome, @ChaveSecreta, @Ativa);",
                new CredencialDeServico
                {
                    Id = Guid.NewGuid().ToString(),
                    Nome = "api-interna",
                    ChaveSecreta = "servico-secreto",
                    Ativa = true
                });
        }
    }
}
