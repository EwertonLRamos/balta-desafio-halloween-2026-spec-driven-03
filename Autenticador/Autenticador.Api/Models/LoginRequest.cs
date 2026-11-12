using System.ComponentModel.DataAnnotations;

namespace Autenticador.Api.Models;

public class LoginRequest
{
    [Required]
    [RegularExpression(@".*\S.*")]
    public string? Login { get; set; }

    [Required]
    [RegularExpression(@".*\S.*")]
    public string? Senha { get; set; }
}
