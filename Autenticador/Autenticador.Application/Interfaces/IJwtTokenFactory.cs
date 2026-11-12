namespace Autenticador.Application.Interfaces;

public interface IJwtTokenFactory
{
    string CreateToken(string login, DateTime expiresAt);
}
