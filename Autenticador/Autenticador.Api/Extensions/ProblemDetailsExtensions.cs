using Microsoft.AspNetCore.Mvc;

namespace Autenticador.Api.Extensions;

public static class ProblemDetailsExtensions
{
    public static ProblemDetails CreateUnauthorizedProblem(string detail)
    {
        return new ProblemDetails
        {
            Type = "/errors/unauthorized",
            Title = "Acesso não autorizado",
            Status = StatusCodes.Status401Unauthorized,
            Detail = detail
        };
    }

    public static ProblemDetails CreateValidationProblem(string detail)
    {
        return new ProblemDetails
        {
            Type = "/errors/invalid-request",
            Title = "Requisição inválida",
            Status = StatusCodes.Status400BadRequest,
            Detail = detail
        };
    }
}
