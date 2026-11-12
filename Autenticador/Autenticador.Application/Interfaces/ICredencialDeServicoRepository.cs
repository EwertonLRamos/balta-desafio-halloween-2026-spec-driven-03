using Autenticador.Domain.Entities;

namespace Autenticador.Application.Interfaces;

public interface ICredencialDeServicoRepository
{
    Task<CredencialDeServico?> ObterPorNomeAsync(string nome);
}
