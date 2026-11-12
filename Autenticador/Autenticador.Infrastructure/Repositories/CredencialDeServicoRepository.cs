using Autenticador.Application.Interfaces;
using Autenticador.Domain.Entities;
using Autenticador.Infrastructure.Data;
using Dapper;

namespace Autenticador.Infrastructure.Repositories;

public class CredencialDeServicoRepository : ICredencialDeServicoRepository
{
    private readonly IDatabaseConnectionFactory _databaseConnectionFactory;

    public CredencialDeServicoRepository(IDatabaseConnectionFactory databaseConnectionFactory)
    {
        _databaseConnectionFactory = databaseConnectionFactory;
    }

    public async Task<CredencialDeServico?> ObterPorNomeAsync(string nome)
    {
        using var connection = _databaseConnectionFactory.CreateConnection();
        connection.Open();

        return await connection.QuerySingleOrDefaultAsync<CredencialDeServico>(@"
            SELECT *
            FROM CredenciaisDeServico
            WHERE Nome = @Nome AND Ativa = 1",
            new { Nome = nome });
    }
}
