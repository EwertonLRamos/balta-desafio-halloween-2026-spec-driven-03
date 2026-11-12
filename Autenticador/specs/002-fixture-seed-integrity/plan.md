# Implementation Plan: Integridade das contas de teste

**Branch**: `002-fixture-seed-integrity` | **Date**: 2026-11-12 | **Spec**: `/specs/002-fixture-seed-integrity/spec.md`

**Input**: Feature specification from `/specs/002-fixture-seed-integrity/spec.md`

## Summary

A feature precisa garantir exatamente três contas de teste em desenvolvimento e teste, preservando contas reais mesmo quando os logins preferenciais colidem, usando uma identidade persistida para cada fixture e uma sequência determinística de logins alternativos. O plano mantém a solução na arquitetura existente em camadas (.NET 10 + ASP.NET Core + Dapper + xUnit) e segue a constituição do projeto, com nomes em português, testes obrigatórios e um único tipo de nível superior por arquivo.

## Technical Context

**Language/Version**: C# / .NET 10 (ASP.NET Core)

**Primary Dependencies**: ASP.NET Core, Dapper, JWT bearer auth, xUnit, SQLite/connection string-based persistence

**Storage**: SQLite or database connection specified through `AutenticadorDatabase`; repositories use Dapper and persist users in `Usuarios` and service credentials in `CredenciaisDeServico`

**Testing**: xUnit unit tests for business rules and ASP.NET Core integration tests for HTTP contract validation; run with `dotnet test`

**Target Platform**: Backend API service for development and test environments; production must not seed fixtures

**Project Type**: web-service / backend API

**Performance Goals**: startup seed should complete within a few seconds for small developer/test databases; no large-scale throughput requirement for this feature

**Constraints**: exact three fixtures only in Development/Testing, deterministic idempotent seeding, preserve all preexisting real accounts, no fixture creation in Production, all names and error messages in Portuguese, one top-level type per file

**Scale/Scope**: single authentication service with three standard fixtures, one seed initializer, repository access layer, authorization endpoints, and matching tests

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- PASS: API em camadas e controllers finas. `AuthController` and `UsuarioStatusController` delegate to application services rather than embedding persistence or business decisions.
- PASS: Persistência encapsulada. `UsuarioRepository` and `CredencialDeServicoRepository` centralize Dapper SQL and expose domain-facing methods.
- PASS: Testes obrigatórios. The feature requires unit tests for each modified rule and integration tests for each changed HTTP endpoint (success and error paths).
- PASS: Contratos e idioma consistentes. Error messages and endpoint contracts are written in Portuguese; `ProblemDetails` is used for validation and unauthorized responses.
- PASS: Mudanças planejadas e arquivos coesos. The implementation will respect one top-level type per file and keep naming in Portuguese.

No constitutional violations require complexity tracking. This plan remains compliant with the project constitution and does not require a justification exception.

## Project Structure

### Documentation (this feature)

```text
specs/002-fixture-seed-integrity/
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
│   ├── AuthController.cs
│   └── UsuarioStatusController.cs
├── Extensions/
├── Models/
├── Program.cs
└── appsettings*.json

Autenticador.Application/
├── Interfaces/
├── Services/
│   ├── AuthService.cs
│   └── UsuarioStatusService.cs
└── *.cs

Autenticador.Domain/
├── Entities/
├── Models/
└── *.cs

Autenticador.Infrastructure/
├── Data/
│   ├── DatabaseConnectionFactory.cs
│   └── DatabaseInitializer.cs
├── Repositories/
├── Security/
└── *.cs

Autenticador.UnitTests/
├── Services/
└── *.cs

Autenticador.IntegrationTests/
├── Controllers/
├── Fixtures/
├── Infrastructure/
├── Startup/
└── *.cs
```

**Structure Decision**: The feature fits a single backend web service with the existing layered layout: API controllers in `Autenticador.Api`, business logic in `Autenticador.Application`, domain entities in `Autenticador.Domain`, Dapper persistence in `Autenticador.Infrastructure`, and test coverage in `Autenticador.UnitTests` and `Autenticador.IntegrationTests`.

## Complexity Tracking

No constitution violations require an exception or additional complexity documentation.
