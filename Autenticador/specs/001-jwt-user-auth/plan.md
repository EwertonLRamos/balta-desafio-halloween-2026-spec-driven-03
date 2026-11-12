# Implementation Plan: Autenticação de Usuários com JWT

**Branch**: `001-jwt-user-auth` | **Date**: 2026-11-12 | **Spec**: `/specs/001-jwt-user-auth/spec.md`

**Input**: Feature specification from `/specs/001-jwt-user-auth/spec.md`

## Summary

Implementar uma API HTTP em .NET 10 para autenticar usuários por login/senha, emitir JWT com a identidade do usuário e prazo de expiração, consultar atividade por login com autorização de serviço, e inicializar três contas de teste somente em ambientes de desenvolvimento/teste. A solução será estruturada em controllers finos, serviços de autenticação e repositórios Dapper com hashing unidirecional de senhas e limitação de tentativas por conta.

## Technical Context

**Language/Version**: C# / .NET 10

**Primary Dependencies**: ASP.NET Core, Dapper, JWT (Microsoft.AspNetCore.Authentication.JwtBearer), xUnit, FluentAssertions (or equivalent) e Microsoft.NET.Test.Sdk

**Storage**: Banco relacional acessado por abstrações de persistência com Dapper; ambiente de testes com banco leve configurável (SQLite/PostgreSQL) e seed de usuários em desenvolvimento/teste

**Testing**: xUnit para testes unitários e de integração da API; testes de contrato HTTP com status e body de erro

**Target Platform**: API server executando em Linux/macOS/Windows via .NET 10, com foco em ambiente de desenvolvimento, teste e produção controlada

**Project Type**: web-service

**Performance Goals**: autenticação em <250 ms p95 em carga leve; suporte a dezenas de usuários com latência aceitável para endpoints de autenticação e consulta de status

**Constraints**: senhas nunca em texto puro; respostas de autenticação equivalentes para login inexistente e senha incorreta; contas de teste só criadas em desenvolvimento/teste; autorização por credencial de serviço para consulta de existência/atividade

**Scale/Scope**: API com poucos usuários pré-definidos e fluxos de autenticação bem delimitados; sem necessidade de multi-tenant ou grande volume de dados inicial

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- I. API em camadas e controllers finas: PASS. A arquitetura usará controllers finos delegando para serviços e repositórios dedicados.
- II. Persistência encapsulada: PASS. SQL e acesso ao banco ficarão na camada de persistência usando Dapper e abstrações que isolam a regra de negócio.
- III. Testes obrigatórios: PASS. A implementação incluirá testes unitários xUnit para regras de negócio e testes de integração para cada endpoint principal.
- IV. Contratos e idioma consistentes: PASS. Nomes, mensagens e respostas estarão em português; erros de validação retornarão HTTP 400 em ProblemDetails.
- V. Mudanças planejadas e arquivos coesos: PASS. O plano define um tipo de nível superior por arquivo e justifica dependências antes da adoção.

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
src/
├── Autenticador.Api/
│   ├── Controllers/
│   ├── Program.cs
│   ├── Extensions/
│   └── Models/
├── Autenticador.Application/
│   ├── Services/
│   ├── Interfaces/
│   └── Validators/
├── Autenticador.Domain/
│   ├── Entities/
│   ├── ValueObjects/
│   └── Enums/
├── Autenticador.Infrastructure/
│   ├── Data/
│   ├── Repositories/
│   └── Security/
└── Autenticador.Common/
    └── Helpers/

tests/
├── Autenticador.UnitTests/
│   ├── Services/
│   └── Security/
├── Autenticador.IntegrationTests/
│   ├── Controllers/
│   └── Contracts/
└── Autenticador.Testing/
    └── Fixtures/
```

**Structure Decision**: A implementação seguirá a separação em camadas exigida pela constituição, com API, aplicação, domínio e infraestrutura explícitos, e testes unitários/integrados abrangendo autenticação, consulta de atividade e seed de contas de teste.

## Complexity Tracking

Sem violações da constituição que requeiram justificativa adicional.
