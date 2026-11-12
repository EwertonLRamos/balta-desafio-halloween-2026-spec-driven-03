# Tasks: Autenticação de Usuários com JWT

**Input**: Design documents from `/specs/001-jwt-user-auth/`

**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Tests**: Os testes abaixo estão incluídos porque o fluxo de autenticação e autorização exige validação de comportamento e contratos HTTP.

**Organization**: As tarefas foram agrupadas por história de usuário para permitir implementação e validação independentes.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Pode rodar em paralelo (arquivos diferentes, sem dependência de tarefas incompletas)
- **[Story]**: História de usuário a que a tarefa pertence (ex.: US1, US2, US3)
- Inclui caminhos exatos de arquivo em cada descrição

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Inicialização do projeto e estrutura base da solução .NET

- [ ] T001 Create solution and project structure per implementation plan in src/Autenticador.Api/, src/Autenticador.Application/, src/Autenticador.Domain/, src/Autenticador.Infrastructure/, tests/Autenticador.UnitTests/, and tests/Autenticador.IntegrationTests/
- [ ] T002 Initialize .NET 10 solution with ASP.NET Core, Dapper, JWT authentication, xUnit, FluentAssertions, and Microsoft.NET.Test.Sdk in src/Autenticador.Api/Autenticador.Api.csproj, src/Autenticador.Application/Autenticador.Application.csproj, src/Autenticador.Domain/Autenticador.Domain.csproj, src/Autenticador.Infrastructure/Autenticador.Infrastructure.csproj, tests/Autenticador.UnitTests/Autenticador.UnitTests.csproj, and tests/Autenticador.IntegrationTests/Autenticador.IntegrationTests.csproj
- [ ] T003 [P] Configure logging, environment-based configuration, and test settings in src/Autenticador.Api/appsettings.json, src/Autenticador.Api/appsettings.Development.json, and tests/Autenticador.IntegrationTests/TestEnvironment.cs

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Infraestrutura compartilhada que deve estar pronta antes de qualquer história de usuário

**⚠️ CRITICAL**: Nenhuma história de usuário pode começar antes deste bloco estar concluído.

- [ ] T004 Setup Dapper repository abstractions and database connection registration in src/Autenticador.Infrastructure/Data/DatabaseConnectionFactory.cs and src/Autenticador.Infrastructure/Data/DapperServiceCollectionExtensions.cs
- [ ] T005 [P] Implement password hashing and JWT configuration abstractions in src/Autenticador.Infrastructure/Security/PasswordHasherAdapter.cs and src/Autenticador.Infrastructure/Security/JwtOptions.cs
- [ ] T006 [P] Create shared ProblemDetails error contracts and HTTP response conventions in src/Autenticador.Api/Extensions/ProblemDetailsExtensions.cs and src/Autenticador.Api/Models/ApiErrorResponse.cs
- [ ] T007 Create base domain entities and repository contracts for Usuario and CredencialDeServico in src/Autenticador.Domain/Entities/Usuario.cs, src/Autenticador.Domain/Entities/CredencialDeServico.cs, src/Autenticador.Application/Interfaces/IUsuarioRepository.cs, and src/Autenticador.Application/Interfaces/ICredencialDeServicoRepository.cs

**Checkpoint**: A base de infraestrutura, hashing e contratos HTTP está pronta para a implementação das histórias de usuário.

---

## Phase 3: User Story 1 - Autenticar usuário (Priority: P1) 🎯 MVP

**Goal**: Permitir que um usuário ativo autentique com login e senha e receba um JWT temporário sem expor credenciais.

**Independent Test**: Enviar credenciais válidas de uma conta ativa e confirmar que a resposta inclui token JWT válido, expiraEm e não expõe a senha; repetir falhas para validar bloqueio temporário e resposta uniforme.

### Tests for User Story 1

- [ ] T008 [P] [US1] Contract test for POST /api/auth/login success and invalid-credentials scenarios in tests/Autenticador.IntegrationTests/Controllers/AuthControllerTests.cs
- [ ] T009 [P] [US1] Unit tests for password verification, lockout window, and account inactivity in tests/Autenticador.UnitTests/Services/AuthServiceTests.cs

### Implementation for User Story 1

- [ ] T010 [US1] Implement Usuario domain model with fields Login, SenhaHash, Ativo, DataCriacao, DataUltimaFalha, QuantidadeFalhas, and BloqueadoAte as required by data-model.md in src/Autenticador.Domain/Entities/Usuario.cs
- [ ] T011 [US1] Add login DTOs and validation rules for required, trimmed, and case-insensitive login handling in src/Autenticador.Api/Models/LoginRequest.cs and src/Autenticador.Api/Models/LoginResponse.cs
- [ ] T012 [US1] Implement secure password verification and hash storage using a non-reversible algorithm with salt in src/Autenticador.Infrastructure/Security/PasswordHasherAdapter.cs
- [ ] T013 [US1] Implement user lookup and authentication repository methods for active users, failed-attempt counters, and lockout checks in src/Autenticador.Infrastructure/Repositories/UsuarioRepository.cs
- [ ] T014 [US1] Implement AuthService with rules for invalid login, inactive account, lockout window, and JWT issuance using only identity and expiration in src/Autenticador.Application/Services/AuthService.cs
- [ ] T015 [US1] Implement AuthController endpoint for POST /api/auth/login and enforce HTTP 400/401 contract semantics in src/Autenticador.Api/Controllers/AuthController.cs
- [ ] T016 [US1] Add service registration and dependency injection for AuthService, JWT authentication, and security services in src/Autenticador.Api/Program.cs and src/Autenticador.Api/Extensions/ServiceCollectionExtensions.cs

**Checkpoint**: User Story 1 deve estar funcional e testável independentemente, com autenticação e lockout operando conforme contrato e regras de segurança.

---

## Phase 4: User Story 2 - Consultar existência e atividade (Priority: P2)

**Goal**: Permitir que um consumidor autorizado consulte se um login existe e está ativo, sem expor dados pessoais ou senha.

**Independent Test**: Consumir o endpoint de consulta com credencial de serviço válida para login ativo, inativo e inexistente; verificar que a resposta contém somente `existe` e `ativa` e que acesso sem autorização é negado.

### Tests for User Story 2

- [ ] T017 [P] [US2] Contract test for GET /api/auth/usuarios/{login}/status success and unauthorized scenarios in tests/Autenticador.IntegrationTests/Controllers/UsuarioStatusControllerTests.cs
- [ ] T018 [P] [US2] Unit tests for service-credential validation and result mapping in tests/Autenticador.UnitTests/Services/UsuarioStatusServiceTests.cs

### Implementation for User Story 2

- [ ] T019 [US2] Implement service-credential model and authorization contract in src/Autenticador.Domain/Entities/CredencialDeServico.cs and src/Autenticador.Application/Interfaces/ICredencialDeServicoRepository.cs
- [ ] T020 [US2] Implement status query repository and service returning Existe and Ativa only in src/Autenticador.Infrastructure/Repositories/UsuarioStatusRepository.cs and src/Autenticador.Application/Services/UsuarioStatusService.cs
- [ ] T021 [US2] Add GET /api/auth/usuarios/{login}/status endpoint with service-credential validation and response model in src/Autenticador.Api/Controllers/UsuarioStatusController.cs and src/Autenticador.Api/Models/UsuarioStatusResponse.cs

**Checkpoint**: User Stories 1 e 2 podem ser validadas isoladamente com autenticação e consulta de status funcionando sem revelar dados sensíveis.

---

## Phase 5: User Story 3 - Usar contas de teste após inicialização (Priority: P3)

**Goal**: Garantir que três contas de teste sejam criadas somente em Development e Testing, sem duplicação ou efeito em produção.

**Independent Test**: Inicializar a aplicação em ambiente de desenvolvimento ou teste sem contas pré-existentes e confirmar que exatamente três contas ficam disponíveis; reinicializar em produção e confirmar que nada é criado.

### Tests for User Story 3

- [ ] T022 [P] [US3] Integration test for application startup seed and idempotent initialization in tests/Autenticador.IntegrationTests/Startup/TestDataInitializerTests.cs

### Implementation for User Story 3

- [ ] T023 [US3] Create startup seeding logic that creates exactly three known test accounts only in Development or Testing and skips duplicates in src/Autenticador.Infrastructure/Data/DatabaseInitializer.cs
- [ ] T024 [US3] Register environment-aware initialization during app bootstrap and ensure production does not create test accounts in src/Autenticador.Api/Program.cs
- [ ] T025 [US3] Add seed validation and test account fixture definitions in src/Autenticador.Testing/Fixtures/TestAccountsFixture.cs and src/Autenticador.Testing/Fixtures/TestAccountData.cs

**Checkpoint**: A aplicação agora inicializa corretamente em ambientes permitidos e mantém contas de teste idempotentes sem afetar produção.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Revisão final, refinamento e validação da solução completa

- [ ] T026 [P] Review security hardening across authentication, password storage, and JWT claims in src/Autenticador.Application/Services/AuthService.cs, src/Autenticador.Infrastructure/Security/PasswordHasherAdapter.cs, and src/Autenticador.Infrastructure/Security/JwtTokenFactory.cs
- [ ] T027 [P] Clean up shared validation and logging for endpoints, lockout behavior, and authorization errors in src/Autenticador.Api/Extensions/ValidationExtensions.cs and src/Autenticador.Api/Extensions/LoggingExtensions.cs
- [ ] T028 Run the full solution validation workflow using the scenarios in specs/001-jwt-user-auth/quickstart.md and verify all xUnit and integration tests pass with dotnet test --logger "console;verbosity=minimal" in tests/Autenticador.UnitTests/ and tests/Autenticador.IntegrationTests/

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: sem dependência, pode iniciar imediatamente
- **Foundational (Phase 2)**: depende do setup e bloqueia todas as histórias de usuário
- **User Story 1 (Phase 3)**: depende da base fundamental; é a história principal e MVP
- **User Story 2 (Phase 4)**: depende da infraestrutura compartilhada e do fluxo de autenticação do US1
- **User Story 3 (Phase 5)**: depende da infraestrutura compartilhada e pode ocorrer em paralelo com US1 e US2 após a base estiver pronta
- **Polish (Phase 6)**: depende de todas as histórias de usuário terem concluído

### Parallel execution examples

- US1: T008 e T009 podem rodar em paralelo; T012 e T013 podem ser implementados em paralelo antes de T014
- US2: T017 e T018 podem rodar em paralelo; T020 pode ser desenvolvido em paralelo com o endpoint de T021
- US3: T022 pode ser executado em paralelo com a finalização da base e também com a parte de inicialização de T023/T024

## Implementation Strategy

- **MVP first**: concluir User Story 1 completamente com autenticação, lockout temporário e JWT válido como primeiro incremento entregável
- **Incremental delivery**: depois do MVP, adicionar User Story 2 para consulta autorizada de status, e por fim User Story 3 para seed e idempotência de ambientes
- **Validation before expansion**: cada história deve ter seu critério de teste independente confirmado antes da próxima fase que dependa dela

## Validation Checklist

- [ ] Todos os IDs seguem o padrão T001, T002, ...
- [ ] Todas as tarefas em fases de história possuem [US1], [US2] ou [US3]
- [ ] Fases de setup/fundação e polish não possuem label de história
- [ ] Cada tarefa inclui caminho de arquivo explícito
- [ ] A ordem de execução mantém dependências entre infra, US1, US2 e US3
