using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Autenticador.Api;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace Autenticador.IntegrationTests.Controllers;

public class AuthControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public AuthControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Login_Deve_Retornar_Token_Quando_Credenciais_Sao_Validas()
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            Login = "admin@teste",
            Senha = "Senha123!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        payload.Should().NotBeNull();
        payload!["token"].ToString().Should().NotBeNullOrWhiteSpace();
        payload["expiraEm"].ToString().Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Login_Deve_Retornar_Unauthorized_Quando_Credenciais_Sao_Invalidas()
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            Login = "admin@teste",
            Senha = "senha-errada"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Status_Deve_Exigir_Credencial_De_Servico_Valida()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/auth/usuarios/admin@teste/status");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        client.DefaultRequestHeaders.Add("X-Service-Credential", "servico-secreto");
        var ok = await client.GetAsync("/api/auth/usuarios/admin@teste/status");

        ok.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await ok.Content.ReadFromJsonAsync<Dictionary<string, bool>>();
        payload.Should().NotBeNull();
        payload!["existe"].Should().BeTrue();
        payload["ativa"].Should().BeTrue();
    }

    [Theory]
    [InlineData("inativo@teste", true, false)]
    [InlineData("nao-existe@teste", false, false)]
    public async Task Status_Deve_Retornar_Somente_Existencia_E_Atividade(
        string login,
        bool existe,
        bool ativa)
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Service-Credential", "servico-secreto");

        var response = await client.GetAsync($"/api/auth/usuarios/{login}/status");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using var payload = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        payload.RootElement.EnumerateObject()
            .Select(property => property.Name)
            .Should().BeEquivalentTo("existe", "ativa");
        payload.RootElement.GetProperty("existe").GetBoolean().Should().Be(existe);
        payload.RootElement.GetProperty("ativa").GetBoolean().Should().Be(ativa);
    }

    [Fact]
    public async Task Login_Deve_Retornar_Mesmo_Erro_Para_Login_Inexistente_E_Senha_Incorreta()
    {
        using var client = _factory.CreateClient();
        var senhaIncorreta = await client.PostAsJsonAsync("/api/auth/login", new
        {
            Login = "admin@teste",
            Senha = "senha-errada"
        });
        var loginInexistente = await client.PostAsJsonAsync("/api/auth/login", new
        {
            Login = "nao-existe@teste",
            Senha = "senha-errada"
        });

        senhaIncorreta.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        loginInexistente.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        (await senhaIncorreta.Content.ReadAsStringAsync())
            .Should().Be(await loginInexistente.Content.ReadAsStringAsync());
    }
}

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databasePath = Path.Combine(Path.GetTempPath(), $"auth-{Guid.NewGuid():N}.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:AutenticadorDatabase"] = $"Data Source={_databasePath}",
                ["Jwt:Key"] = "c2c5d363-7a3f-4b5a-8d7e-b1f4416b2d10-456789012345",
                ["Jwt:Issuer"] = "Autenticador.Api",
                ["Jwt:Audience"] = "Autenticador.Clientes",
                ["ServiceCredential:Secret"] = "servico-secreto"
            });
        });
    }
}
