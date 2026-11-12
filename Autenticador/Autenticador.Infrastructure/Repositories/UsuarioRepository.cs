using System.Data;
using Autenticador.Application.Interfaces;
using Autenticador.Domain.Entities;
using Autenticador.Domain.Models;
using Dapper;
using Autenticador.Infrastructure.Data;

namespace Autenticador.Infrastructure.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly IDatabaseConnectionFactory _databaseConnectionFactory;

    public UsuarioRepository(IDatabaseConnectionFactory databaseConnectionFactory)
    {
        _databaseConnectionFactory = databaseConnectionFactory;
    }

    public async Task<Usuario?> GetByLoginAsync(string login)
    {
        using var connection = _databaseConnectionFactory.CreateConnection();
        connection.Open();
        return await connection.QuerySingleOrDefaultAsync<Usuario>(@"
            SELECT *
            FROM Usuarios
            WHERE LOWER(Login) = LOWER(@Login)",
            new { Login = login.Trim() });
    }

    public async Task AtualizarAsync(Usuario usuario)
    {
        using var connection = _databaseConnectionFactory.CreateConnection();
        connection.Open();
        await connection.ExecuteAsync(@"
            UPDATE Usuarios
            SET Login = @Login,
                SenhaHash = @SenhaHash,
                Ativo = @Ativo,
                DataUltimaFalha = @DataUltimaFalha,
                QuantidadeFalhas = @QuantidadeFalhas,
                BloqueadoAte = @BloqueadoAte
            WHERE Id = @Id",
            usuario);
    }

    public async Task<ResultadoConsultaLogin> ObterStatusAsync(string login)
    {
        using var connection = _databaseConnectionFactory.CreateConnection();
        connection.Open();

        var usuario = await connection.QuerySingleOrDefaultAsync<Usuario>(@"
            SELECT *
            FROM Usuarios
            WHERE LOWER(Login) = LOWER(@Login)",
            new { Login = login.Trim() });

        if (usuario is null)
        {
            return new ResultadoConsultaLogin { Existe = false, Ativa = false };
        }

        return new ResultadoConsultaLogin
        {
            Existe = true,
            Ativa = usuario.Ativo
        };
    }

    public async Task SalvarAsync(Usuario usuario)
    {
        using var connection = _databaseConnectionFactory.CreateConnection();
        connection.Open();

        await connection.ExecuteAsync(@"
            INSERT INTO Usuarios (Id, Login, SenhaHash, Ativo, DataCriacao, DataUltimaFalha, QuantidadeFalhas, BloqueadoAte)
            VALUES (@Id, @Login, @SenhaHash, @Ativo, @DataCriacao, @DataUltimaFalha, @QuantidadeFalhas, @BloqueadoAte);",
            usuario);
    }

    public async Task<IEnumerable<Usuario>> ListarAsync()
    {
        using var connection = _databaseConnectionFactory.CreateConnection();
        connection.Open();
        return await connection.QueryAsync<Usuario>("SELECT * FROM Usuarios ORDER BY Login");
    }
}
