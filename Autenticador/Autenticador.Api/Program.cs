
using System.Text;
using Autenticador.Application.Interfaces;
using Autenticador.Application.Services;
using Autenticador.Infrastructure.Data;
using Autenticador.Infrastructure.Repositories;
using Autenticador.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Autenticador.Api;

public partial class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var connectionString = builder.Configuration.GetConnectionString("AutenticadorDatabase") ?? "Data Source=autenticador.db";
        var jwtOptions = builder.Configuration.GetSection("Jwt").Get<JwtOptions>() ?? new JwtOptions();
        var serviceCredentialSecret = builder.Configuration["ServiceCredential:Secret"];

        if (string.IsNullOrWhiteSpace(jwtOptions.Key) || Encoding.UTF8.GetByteCount(jwtOptions.Key) < 32)
        {
            throw new InvalidOperationException("A chave JWT deve ser configurada e possuir pelo menos 32 bytes.");
        }

        if (string.IsNullOrWhiteSpace(jwtOptions.Issuer)
            || string.IsNullOrWhiteSpace(jwtOptions.Audience)
            || jwtOptions.ExpiresInMinutes <= 0)
        {
            throw new InvalidOperationException("A configuração JWT está incompleta ou inválida.");
        }

        if (string.IsNullOrWhiteSpace(serviceCredentialSecret))
        {
            throw new InvalidOperationException("A credencial de serviço deve ser configurada.");
        }

        builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
        builder.Services.AddSingleton<IPasswordHasher, PasswordHasherAdapter>();
        builder.Services.AddSingleton<IJwtTokenFactory, JwtTokenFactory>();
        builder.Services.AddSingleton<IDatabaseConnectionFactory>(_ => new DatabaseConnectionFactory(connectionString));
        builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        builder.Services.AddScoped<ICredencialDeServicoRepository, CredencialDeServicoRepository>();
        builder.Services.AddScoped<AuthService>(serviceProvider => new AuthService(
            serviceProvider.GetRequiredService<IUsuarioRepository>(),
            serviceProvider.GetRequiredService<IPasswordHasher>(),
            serviceProvider.GetRequiredService<IJwtTokenFactory>(),
            jwtOptions.ExpiresInMinutes));
        builder.Services.AddScoped<UsuarioStatusService>();

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtOptions.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(1)
                };
            });

        builder.Services.AddAuthorization();
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
            var factory = scope.ServiceProvider.GetRequiredService<IDatabaseConnectionFactory>();
            var initializer = new DatabaseInitializer(factory, passwordHasher, app.Environment.EnvironmentName);
            await initializer.InitializeAsync();
        }

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        app.Run();
    }
}

public partial class Program { }
