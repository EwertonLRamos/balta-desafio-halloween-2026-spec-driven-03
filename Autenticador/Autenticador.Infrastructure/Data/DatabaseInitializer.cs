using Autenticador.Application.Interfaces;
using Autenticador.Domain.Entities;
using Dapper;

namespace Autenticador.Infrastructure.Data;

public class DatabaseInitializer
{
    private const string NomeCredencialDeServico = "api-interna";
    private const string SenhaFixturePadrao = "Senha123!";

    private static readonly FixtureTeste[] Fixtures =
    [
        new("fixture-admin", "admin@teste", true),
        new("fixture-operador", "operador@teste", true),
        new("fixture-inativo", "inativo@teste", false)
    ];

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

        var usuarios = (await connection.QueryAsync<Usuario>("SELECT * FROM Usuarios")).ToList();
        var loginsExistentes = usuarios
            .Select(usuario => usuario.Login)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var fixture in Fixtures)
        {
            var usuarioExistente = usuarios.FirstOrDefault(usuario =>
                string.Equals(usuario.Id, fixture.Id, StringComparison.Ordinal));

            if (usuarioExistente is not null)
            {
                loginsExistentes.Add(usuarioExistente.Login);
                continue;
            }

            var loginDisponivel = ObterLoginDisponivel(fixture.LoginPreferencial, loginsExistentes);
            var usuarioFixture = new Usuario
            {
                Id = fixture.Id,
                Login = loginDisponivel,
                SenhaHash = _passwordHasher.HashPassword(SenhaFixturePadrao),
                Ativo = fixture.Ativo,
                DataCriacao = DateTime.UtcNow,
                DataUltimaFalha = null,
                QuantidadeFalhas = 0,
                BloqueadoAte = null
            };

            await connection.ExecuteAsync(@"
                INSERT INTO Usuarios (Id, Login, SenhaHash, Ativo, DataCriacao, DataUltimaFalha, QuantidadeFalhas, BloqueadoAte)
                VALUES (@Id, @Login, @SenhaHash, @Ativo, @DataCriacao, @DataUltimaFalha, @QuantidadeFalhas, @BloqueadoAte);",
                usuarioFixture);

            usuarios.Add(usuarioFixture);
            loginsExistentes.Add(loginDisponivel);
        }

        var hasServiceCredential = await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM CredenciaisDeServico WHERE Nome = @Nome",
            new { Nome = NomeCredencialDeServico });

        if (hasServiceCredential == 0)
        {
            await connection.ExecuteAsync(@"
                INSERT INTO CredenciaisDeServico (Id, Nome, ChaveSecreta, Ativa)
                VALUES (@Id, @Nome, @ChaveSecreta, @Ativa);",
                new CredencialDeServico
                {
                    Id = Guid.NewGuid().ToString(),
                    Nome = NomeCredencialDeServico,
                    ChaveSecreta = "servico-secreto",
                    Ativa = true
                });
        }
    }

    private static string ObterLoginDisponivel(string loginPreferencial, ISet<string> loginsExistentes)
    {
        if (!loginsExistentes.Contains(loginPreferencial))
        {
            return loginPreferencial;
        }

        for (var indice = 1; ; indice++)
        {
            var sufixo = indice == 1 ? ".teste" : $".teste-{indice}";
            var loginAlternativo = $"{loginPreferencial}{sufixo}";

            if (!loginsExistentes.Contains(loginAlternativo))
            {
                return loginAlternativo;
            }
        }
    }

    private sealed record FixtureTeste(string Id, string LoginPreferencial, bool Ativo);
}
