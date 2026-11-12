# Implementation Plan: Autenticação de Usuários com JWT

**Branch**: `001-jwt-user-auth` | **Date**: 2026-11-12 | **Spec**: `/specs/001-jwt-user-auth/spec.md`

**Input**: Feature specification from `/specs/001-jwt-user-auth/spec.md`

## Summary

Implementar uma API HTTP em .NET 10 para autenticar usuários por login/senha, emitir JWT com a identidade do usuário e prazo de expiração, consultar atividade por login com autorização de serviço e inicializar três contas de teste somente em ambientes de desenvolvimento/teste. O plano também resolve a falha de inicialização `TypeLoadException` em `AddSwaggerGen`: alinhar Swashbuckle com o .NET 10, verificar a compatibilidade das assemblies efetivamente resolvidas e validar a geração do documento OpenAPI. A solução será estruturada em controllers finos, serviços de autenticação e repositórios Dapper com hashing unidirecional de senhas e limitação de tentativas por conta.

## Technical Context

**Language/Version**: C# / .NET 10

**Primary Dependencies**: ASP.NET Core 10, Dapper, JWT (Microsoft.AspNetCore.Authentication.JwtBearer), Swashbuckle.AspNetCore 10.0.0 ou posterior compatível com .NET 10 (família Swagger alinhada), xUnit, FluentAssertions (or equivalent) e Microsoft.NET.Test.Sdk

**Storage**: Banco relacional acessado por abstrações de persistência com Dapper; ambiente de testes com banco leve configurável (SQLite/PostgreSQL) e seed de usuários em desenvolvimento/teste

**Testing**: xUnit para testes unitários e de integração da API; testes de contrato HTTP com status e body de erro

**Target Platform**: API server executando em Linux/macOS/Windows via .NET 10, com foco em ambiente de desenvolvimento, teste e produção controlada

**Project Type**: web-service

**Performance Goals**: autenticação em <250 ms p95 em carga leve; suporte a dezenas de usuários com latência aceitável para endpoints de autenticação e consulta de status

**Constraints**: senhas nunca em texto puro; respostas de autenticação equivalentes para login inexistente e senha incorreta; contas de teste só criadas em desenvolvimento/teste; autorização por credencial de serviço para consulta de existência/atividade; dependências Swagger/OpenAPI compatíveis com .NET 10 e assemblies resolvidas sem mistura de versões incompatíveis

**Scale/Scope**: API com poucos usuários pré-definidos e fluxos de autenticação bem delimitados; sem necessidade de multi-tenant ou grande volume de dados inicial

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- I. API em camadas e controllers finas: PASS. A arquitetura usará controllers finos delegando para serviços e repositórios dedicados.
- II. Persistência encapsulada: PASS. SQL e acesso ao banco ficarão na camada de persistência usando Dapper e abstrações que isolam a regra de negócio.
- III. Testes obrigatórios: PASS. A implementação incluirá testes unitários xUnit para regras de negócio e testes de integração para cada endpoint principal.
- IV. Contratos e idioma consistentes: PASS. Nomes, mensagens e respostas estarão em português; erros de validação retornarão HTTP 400 em ProblemDetails.
- V. Mudanças planejadas e arquivos coesos: PASS. O plano define um tipo de nível superior por arquivo e justifica a atualização major de Swashbuckle necessária para compatibilidade com .NET 10 antes da adoção.

**Status**: PASS (sem violações que exijam justificativa) 

## Project Structure

### Documentation (this feature)

```text
specs/001-jwt-user-auth/
├── plan.md              # This file (/speckit-plan command output)
├── research.md          # Phase 0 output (/speckit-plan command)
├── data-model.md        # Phase 1 output (/speckit-plan command)
├── quickstart.md        # Phase 1 output (/speckit-plan command)
├── contracts/           # Phase 1 output (/speckit-plan command)
└── tasks.md             # Phase 2 output (/speckit-tasks command - NOT created by /speckit-plan)
```

### Source Code (repository root)

```text
Autenticador.Api/
├── Controllers/
├── Program.cs
├── Extensions/
└── Models/
Autenticador.Application/
├── Services/
└── Interfaces/
Autenticador.Domain/
├── Entities/
└── Models/
Autenticador.Infrastructure/
├── Data/
├── Repositories/
└── Security/
Autenticador.UnitTests/
└── Services/
Autenticador.IntegrationTests/
├── Controllers/
├── Fixtures/
└── Startup/
    └── SwaggerStartupTests.cs  # Inicialização e documento OpenAPI em Development
```

**Structure Decision**: A implementação seguirá a separação em camadas exigida pela constituição, com projetos na raiz do repositório, e testes unitários/integrados abrangendo autenticação, consulta de atividade e seed de contas de teste.

**Compatibilidade Swagger/OpenAPI**: Atualizar a referência do pacote meta `Swashbuckle.AspNetCore` para uma versão estável 10.0.0+ compatível com .NET 10; manter os componentes Swagger/SwaggerGen/SwaggerUI na mesma família resolvida, revisar alterações de API da migração v9→v10 e evitar referências diretas conflitantes a `Microsoft.OpenApi`. Confirmar a árvore resolvida e as assemblies efetivamente carregadas antes de atribuir a causa apenas ao cache/binários antigos.

**Validação de inicialização**: Com configurações locais válidas e ambiente `Development`, iniciar a API, confirmar que `AddSwaggerGen` não lança `TypeLoadException` e obter HTTP 200 em `/swagger/v1/swagger.json`. Incluir a verificação no fluxo de integração e no quickstart.

**Constitution Check pós-design**: PASS. A atualização major de Swashbuckle tem justificativa explícita de compatibilidade com .NET 10; não adiciona tipos de domínio nem altera a arquitetura em camadas. Testes de inicialização/documentação atendem ao princípio III, e os demais princípios permanecem preservados.

## Complexity Tracking

Sem violações da constituição que requeiram justificativa adicional.
