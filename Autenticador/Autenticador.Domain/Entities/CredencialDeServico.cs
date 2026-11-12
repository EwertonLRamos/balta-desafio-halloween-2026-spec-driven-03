namespace Autenticador.Domain.Entities;

public class CredencialDeServico
{
    public string Id { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string ChaveSecreta { get; set; } = string.Empty;
    public bool Ativa { get; set; }
}
