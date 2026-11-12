using System.Data;
using Dapper;
using Microsoft.Data.Sqlite;

namespace Autenticador.Infrastructure.Data;

public interface IDatabaseConnectionFactory
{
    IDbConnection CreateConnection();
    Task InitializeAsync();
}

public class DatabaseConnectionFactory : IDatabaseConnectionFactory
{
    private readonly string _connectionString;

    public DatabaseConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    public IDbConnection CreateConnection()
    {
        return new SqliteConnection(_connectionString);
    }

    public async Task InitializeAsync()
    {
        using var connection = CreateConnection();
        connection.Open();

        await connection.ExecuteAsync(@"
            CREATE TABLE IF NOT EXISTS Usuarios (
                Id TEXT PRIMARY KEY,
                Login TEXT NOT NULL UNIQUE,
                SenhaHash TEXT NOT NULL,
                Ativo INTEGER NOT NULL,
                DataCriacao TEXT NOT NULL,
                DataUltimaFalha TEXT NULL,
                QuantidadeFalhas INTEGER NOT NULL DEFAULT 0,
                BloqueadoAte TEXT NULL
            );");

        await connection.ExecuteAsync(@"
            CREATE TABLE IF NOT EXISTS CredenciaisDeServico (
                Id TEXT PRIMARY KEY,
                Nome TEXT NOT NULL UNIQUE,
                ChaveSecreta TEXT NOT NULL,
                Ativa INTEGER NOT NULL
            );");
    }
}
