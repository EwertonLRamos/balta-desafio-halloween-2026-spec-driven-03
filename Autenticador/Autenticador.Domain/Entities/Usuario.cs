namespace Autenticador.Domain.Entities;

public class Usuario
{
    public string Id { get; set; } = string.Empty;
    public string Login { get; set; } = string.Empty;
    public string SenhaHash { get; set; } = string.Empty;
    public bool Ativo { get; set; }
    public DateTime DataCriacao { get; set; }
    public DateTime? DataUltimaFalha { get; set; }
    public int QuantidadeFalhas { get; set; }
    public DateTime? BloqueadoAte { get; set; }
}
