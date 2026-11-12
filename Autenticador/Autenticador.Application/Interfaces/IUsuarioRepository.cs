using Autenticador.Domain.Entities;
using Autenticador.Domain.Models;

namespace Autenticador.Application.Interfaces;

public interface IUsuarioRepository
{
    Task<Usuario?> GetByLoginAsync(string login);
    Task AtualizarAsync(Usuario usuario);
    Task<ResultadoConsultaLogin> ObterStatusAsync(string login);
    Task SalvarAsync(Usuario usuario);
    Task<IEnumerable<Usuario>> ListarAsync();
}
