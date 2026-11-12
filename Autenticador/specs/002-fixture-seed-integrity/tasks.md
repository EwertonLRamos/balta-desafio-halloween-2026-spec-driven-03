# Tasks: Integridade das contas de teste

**Input**: Design documents from `/specs/002-fixture-seed-integrity/`

**Prerequisites**: `plan.md` (required), `spec.md` (required for user stories), `research.md`, `data-model.md`, `contracts/`

**Tests**: The feature explicitly requires unit and integration coverage; the tasks below include both test-first tasks and implementation work.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Verify the shared .NET solution baseline and confirm the feature is aligned with the repository structure.

- [x] T001 [P] Confirm the .NET solution and test projects are ready in `Autenticador.slnx`, `Autenticador.UnitTests/Autenticador.UnitTests.csproj`, and `Autenticador.IntegrationTests/Autenticador.IntegrationTests.csproj`
- [x] T002 [P] Validate the existing layered architecture and the feature plan in `Autenticador.Api/Program.cs`, `Autenticador.Application/`, and `Autenticador.Infrastructure/`
- [x] T003 Establish the Portuguese naming and single-type-per-file rules in `specs/002-fixture-seed-integrity/plan.md` and `specs/002-fixture-seed-integrity/tasks.md`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Build the seed strategy and repository contract before story work begins.

**⚠️ CRITICAL**: No User Story work can begin until this phase is complete.

- [x] T004 [P] Extend the repository contract for stable fixture identity and collision-safe lookup in `Autenticador.Application/Interfaces/IUsuarioRepository.cs`
- [x] T005 [P] Implement the development/test environment gate and idempotent seed behavior in `Autenticador.Infrastructure/Data/DatabaseInitializer.cs`
- [x] T006 [P] Add deterministic login resolution for collision handling in `Autenticador.Infrastructure/Data/DatabaseInitializer.cs` using the sequence `<login>.teste`, `<login>.teste-2`, `<login>.teste-3`, and subsequent numbered suffixes
- [x] T007 Ensure database initialization preserves real accounts and never creates fixtures in production in `Autenticador.Api/Program.cs` and `Autenticador.Infrastructure/Data/DatabaseInitializer.cs`

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel.

---

## Phase 3: User Story 1 - Disponibilizar contas de teste sem afetar contas reais (Priority: P1) 🎯 MVP

**Goal**: Ensure exactly three stable test-user identities exist in Development and Testing without modifying real accounts or creating duplicates.

**Independent Test**: Seed real accounts with the same login values as the fixtures, run the app in Development/Testing, and verify that the three fixtures exist with stable login aliases while preserving all real accounts.

### Tests for User Story 1

- [x] T008 [P] [US1] Create failing unit tests for collision handling and idempotent re-seeding in `Autenticador.UnitTests/Services/TestesRepositorioUsuario.cs`
- [x] T009 [P] [US1] Create failing integration tests for startup seeding and production bypass in `Autenticador.IntegrationTests/Startup/TestesInicializacaoSwagger.cs`

### Implementation for User Story 1

- [x] T010 [US1] Implement stable fixture identity persistence and comparison logic in `Autenticador.Infrastructure/Data/DatabaseInitializer.cs`
- [x] T011 [US1] Resolve login collisions deterministically without altering real accounts in `Autenticador.Infrastructure/Data/DatabaseInitializer.cs`
- [x] T012 [US1] Prevent duplicate fixture creation and preserve existing real-user state in `Autenticador.Infrastructure/Repositories/UsuarioRepository.cs`
- [x] T013 [US1] Enforce production environment exclusion so no test accounts are created in `Autenticador.Api/Program.cs`
- [x] T014 [US1] Add validation for `Login` case-insensitive collision handling and stable fixture reconciliation in `Autenticador.Infrastructure/Data/DatabaseInitializer.cs`
- [x] T015 [US1] Run the focused unit and integration verification for the seed flow with `dotnet test` for the relevant projects

**Checkpoint**: At this point, User Story 1 should be fully functional and independently testable.

---

## Phase 4: User Story 2 - Manter nomes de código em conformidade com a constituição (Priority: P2)

**Goal**: Keep the repository and startup test artifacts in Portuguese and compliant with the project constitution.

**Independent Test**: Confirm the files `TestesRepositorioUsuario.cs` and `TestesInicializacaoSwagger.cs` are in Portuguese, each file declares only one top-level type, and the HTTP contract remains covered by unit and integration tests.

### Tests for User Story 2

- [x] T016 [P] [US2] Rename the repository unit tests to the exact required class name `TestesRepositorioUsuario` in `Autenticador.UnitTests/Services/TestesRepositorioUsuario.cs`
- [x] T017 [P] [US2] Rename the startup integration tests to the exact required class name `TestesInicializacaoSwagger` in `Autenticador.IntegrationTests/Startup/TestesInicializacaoSwagger.cs`
- [x] T018 [US2] Add unit-test assertions for collision edge cases, including `Login` comparison ignoring case and deterministic variant selection in `Autenticador.UnitTests/Services/TestesRepositorioUsuario.cs`
- [x] T019 [US2] Add integration-test coverage for success and error HTTP contracts in `Autenticador.IntegrationTests/Startup/TestesInicializacaoSwagger.cs`

### Implementation for User Story 2

- [x] T020 [P] [US2] Review and update any code-file names in `Autenticador.UnitTests/` and `Autenticador.IntegrationTests/` so every artifact is in Portuguese and each file contains at most one top-level type
- [x] T021 [US2] Validate the naming and file-scope rules in `Autenticador.UnitTests/Services/TestesRepositorioUsuario.cs` and `Autenticador.IntegrationTests/Startup/TestesInicializacaoSwagger.cs`
- [x] T022 [US2] Confirm the updated tests cover the contract and constitutional obligations for unit and integration testing across the changed business rules and endpoints

**Checkpoint**: At this point, User Stories 1 and 2 should both be independently functional and compliant with the constitution.

---

## Phase 5: Polish & Cross-Cutting Concerns

**Purpose**: Final review, documentation, and execution validation across the feature.

- [x] T023 [P] Update the validation guide and environment expectations in `specs/002-fixture-seed-integrity/quickstart.md`
- [x] T024 [P] Review the final contract and data-model alignment in `specs/002-fixture-seed-integrity/contracts/auth-contract.md` and `specs/002-fixture-seed-integrity/data-model.md`
- [x] T025 Run the full relevant verification with `dotnet test` and fix any failing assertions or contract mismatches across `Autenticador.UnitTests` and `Autenticador.IntegrationTests`
- [x] T026 Final constitutional compliance review for naming, file scoping, and test coverage before marking the feature ready for implementation

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies; can start immediately.
- **Foundational (Phase 2)**: Depends on Setup completion; blocks all user story work.
- **User Stories (Phase 3+)**: Depend on Foundational completion; can proceed in priority order or in parallel if team capacity allows.
- **Polish (Final Phase)**: Depends on all desired stories being complete.

### User Story Dependencies

- **User Story 1 (P1)**: No dependency on User Story 2; it is the MVP and the core functionality.
- **User Story 2 (P2)**: Depends on the seed behavior being stable; it refines naming and test compliance, but should be independently verifiable following the same feature work.

### Parallel Opportunities

- Setup tasks T001-T003 can run in parallel.
- Foundational tasks T004-T007 can run in parallel within the same phase.
- The two test-authoring tasks T008 and T009 can be developed in parallel.
- Task T020 can proceed while T018-T019 are being completed if the test files are already updated.
- Final validation tasks T023-T026 can be executed in parallel for documentation and review, then consolidated before sign-off.

---

## Parallel Example: User Story 1

```bash
# Execute targeted test authoring in parallel
Task: "Create failing unit tests for collision handling and idempotent re-seeding in Autenticador.UnitTests/Services/TestesRepositorioUsuario.cs"
Task: "Create failing integration tests for startup seeding and production bypass in Autenticador.IntegrationTests/Startup/TestesInicializacaoSwagger.cs"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup.
2. Complete Phase 2: Foundational.
3. Complete Phase 3: User Story 1.
4. Validate the fixture behavior independently and only then continue to naming/test compliance work.

### Incremental Delivery

1. Stabilize fixture identity, login collision behavior, and production guard.
2. Add the Portuguese names and compliance validations for test artifacts.
3. Run the full relevant verification, update docs, and confirm the feature remains aligned to the constitution.
