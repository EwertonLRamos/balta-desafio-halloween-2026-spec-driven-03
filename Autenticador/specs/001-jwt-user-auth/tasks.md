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

- [X] T001 Create solution and project structure per implementation plan in Autenticador.Api/, Autenticador.Application/, Autenticador.Domain/, Autenticador.Infrastructure/, Autenticador.UnitTests/, and Autenticador.IntegrationTests/
- [X] T002 Initialize .NET 10 solution with ASP.NET Core, Dapper, JWT authentication, xUnit, FluentAssertions, and Microsoft.NET.Test.Sdk in the six project files under the corresponding project directories
- [X] T003 [P] Configure logging, environment-based configuration, and test settings in Autenticador.Api/appsettings.json, Autenticador.Api/appsettings.Development.json, and Autenticador.IntegrationTests/Controllers/AuthControllerTests.cs

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Infraestrutura compartilhada que deve estar pronta antes de qualquer história de usuário

**⚠️ CRITICAL**: Nenhuma história de usuário pode começar antes deste bloco estar concluído.

- [X] T004 Setup Dapper repository abstractions and database connection registration in Autenticador.Infrastructure/Data/DatabaseConnectionFactory.cs and Autenticador.Api/Program.cs
- [X] T005 [P] Implement password hashing and JWT configuration abstractions in Autenticador.Infrastructure/Security/PasswordHasherAdapter.cs and Autenticador.Infrastructure/Security/JwtOptions.cs
- [X] T006 [P] Create shared ProblemDetails error contracts and HTTP response conventions in Autenticador.Api/Extensions/ProblemDetailsExtensions.cs
- [X] T007 Create base domain entities and repository contracts for Usuario and CredencialDeServico in Autenticador.Domain/Entities/Usuario.cs, Autenticador.Domain/Entities/CredencialDeServico.cs, Autenticador.Application/Interfaces/IUsuarioRepository.cs, and Autenticador.Application/Interfaces/ICredencialDeServicoRepository.cs

**Checkpoint**: A base de infraestrutura, hashing e contratos HTTP está pronta para a implementação das histórias de usuário.

---

## Phase 3: User Story 1 - Autenticar usuário (Priority: P1) 🎯 MVP

**Goal**: Permitir que um usuário ativo autentique com login e senha e receba um JWT temporário sem expor credenciais.

**Independent Test**: Enviar credenciais válidas de uma conta ativa e confirmar que a resposta inclui token JWT válido, expiraEm e não expõe a senha; repetir falhas para validar bloqueio temporário e resposta uniforme.

### Tests for User Story 1

- [X] T008 [P] [US1] Contract test for POST /api/auth/login success and invalid-credentials scenarios in Autenticador.IntegrationTests/Controllers/AuthControllerTests.cs
- [X] T009 [P] [US1] Unit tests for password verification, lockout window, and account inactivity in Autenticador.UnitTests/Services/AuthServiceTests.cs

### Implementation for User Story 1

- [X] T010 [US1] Implement Usuario domain model with fields Login, SenhaHash, Ativo, DataCriacao, DataUltimaFalha, QuantidadeFalhas, and BloqueadoAte as required by data-model.md in Autenticador.Domain/Entities/Usuario.cs
- [X] T011 [US1] Add login DTOs and validation rules for required, non-whitespace, and case-insensitive login handling in Autenticador.Api/Models/LoginRequest.cs and Autenticador.Api/Models/LoginResponse.cs
- [X] T012 [US1] Implement secure password verification and hash storage using a non-reversible algorithm with salt in Autenticador.Infrastructure/Security/PasswordHasherAdapter.cs
- [X] T013 [US1] Implement user lookup and authentication repository methods for active users, failed-attempt counters, and lockout checks in Autenticador.Infrastructure/Repositories/UsuarioRepository.cs
- [X] T014 [US1] Implement AuthService with rules for invalid login, inactive account, lockout window, and JWT issuance using only identity and expiration in Autenticador.Application/Services/AuthService.cs
- [X] T015 [US1] Implement AuthController endpoint for POST /api/auth/login and enforce HTTP 400/401 contract semantics in Autenticador.Api/Controllers/AuthController.cs
- [X] T016 [US1] Add service registration and dependency injection for AuthService, JWT authentication, and security services in Autenticador.Api/Program.cs

**Checkpoint**: User Story 1 deve estar funcional e testável independentemente, com autenticação e lockout operando conforme contrato e regras de segurança.

---

## Phase 4: User Story 2 - Consultar existência e atividade (Priority: P2)

**Goal**: Permitir que um consumidor autorizado consulte se um login existe e está ativo, sem expor dados pessoais ou senha.

**Independent Test**: Consumir o endpoint de consulta com credencial de serviço válida para login ativo, inativo e inexistente; verificar que a resposta contém somente `existe` e `ativa` e que acesso sem autorização é negado.

### Tests for User Story 2

- [X] T017 [P] [US2] Contract test for GET /api/auth/usuarios/{login}/status success, unauthorized, inactive, and missing-user scenarios in Autenticador.IntegrationTests/Controllers/AuthControllerTests.cs
- [X] T018 [P] [US2] Unit tests for status result mapping in Autenticador.UnitTests/Services/UsuarioStatusServiceTests.cs; credential validation is covered by endpoint integration tests

### Implementation for User Story 2

- [X] T019 [US2] Implement service-credential model and authorization contract in Autenticador.Domain/Entities/CredencialDeServico.cs and Autenticador.Application/Interfaces/ICredencialDeServicoRepository.cs
- [X] T020 [US2] Implement status query repository and service returning Existe and Ativa only in Autenticador.Infrastructure/Repositories/UsuarioRepository.cs and Autenticador.Application/Services/UsuarioStatusService.cs
- [X] T021 [US2] Add GET /api/auth/usuarios/{login}/status endpoint with service-credential validation and response model in Autenticador.Api/Controllers/UsuarioStatusController.cs and Autenticador.Api/Models/UsuarioStatusResponse.cs

**Checkpoint**: User Stories 1 e 2 podem ser validadas isoladamente com autenticação e consulta de status funcionando sem revelar dados sensíveis.

---

## Phase 5: User Story 3 - Usar contas de teste após inicialização (Priority: P3)

**Goal**: Garantir que três contas de teste sejam criadas somente em Development e Testing, sem duplicação ou efeito em produção.

**Independent Test**: Inicializar a aplicação em ambiente de desenvolvimento ou teste sem contas pré-existentes e confirmar que exatamente três contas ficam disponíveis; reinicializar em produção e confirmar que nada é criado.

### Tests for User Story 3

- [X] T022 [P] [US3] Integration test for application startup seed and idempotent initialization in Autenticador.IntegrationTests/Startup/TestDataInitializerTests.cs

### Implementation for User Story 3

- [X] T023 [US3] Create startup seeding logic that creates exactly three known test accounts only in Development or Testing and skips duplicates in Autenticador.Infrastructure/Data/DatabaseInitializer.cs
- [X] T024 [US3] Register environment-aware initialization during app bootstrap and ensure production does not create test accounts in Autenticador.Api/Program.cs
- [X] T025 [US3] Add seed validation and test account fixture definitions in Autenticador.IntegrationTests/Fixtures/TestAccountsFixture.cs and Autenticador.IntegrationTests/Fixtures/TestAccountData.cs

**Checkpoint**: A aplicação agora inicializa corretamente em ambientes permitidos e mantém contas de teste idempotentes sem afetar produção.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Revisão final, refinamento e validação da solução completa

- [X] T026 [P] Review security hardening across authentication, password storage, and JWT claims in Autenticador.Application/Services/AuthService.cs, Autenticador.Infrastructure/Security/PasswordHasherAdapter.cs, and Autenticador.Infrastructure/Security/JwtTokenFactory.cs
- [X] T027 [P] Harden shared request validation and log generic authentication/authorization failures without recording secrets in Autenticador.Api/Models/LoginRequest.cs, Autenticador.Api/Controllers/AuthController.cs, and Autenticador.Api/Controllers/UsuarioStatusController.cs
- [ ] T028 Run the full solution validation workflow using the scenarios in specs/001-jwt-user-auth/quickstart.md and verify all xUnit and integration tests pass with dotnet test --logger "console;verbosity=minimal" against Autenticador.slnx

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

- [X] Todos os IDs seguem o padrão T001, T002, ...
- [X] Todas as tarefas em fases de história possuem [US1], [US2] ou [US3]
- [X] Fases de setup/fundação e polish não possuem label de história
- [X] Cada tarefa inclui caminho de arquivo explícito
- [X] A ordem de execução mantém dependências entre infra, US1, US2 e US3
