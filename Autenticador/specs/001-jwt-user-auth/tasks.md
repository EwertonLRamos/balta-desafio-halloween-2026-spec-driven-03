# Tasks: Autenticação de Usuários com JWT

**Input**: Design documents from `/specs/001-jwt-user-auth/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/auth-api.md, quickstart.md

**Tests**: Testes incluídos porque a constituição exige teste unitário para cada regra de negócio e testes de integração para endpoints e inicialização relevantes.

**Organization**: Tarefas agrupadas por história de usuário; tarefas marcadas `[X]` já foram concluídas antes desta geração. As tarefas novas permanecem `[ ]`.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: pode ser executada em paralelo sem depender de alteração incompleta em outro arquivo
- **[Story]**: história relacionada; obrigatória nas fases de história do usuário
- Todos os itens possuem ID sequencial e caminho de arquivo explícito

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Inicialização da solução e configuração compartilhada.

- [X] T001 Criar solução e estrutura dos projetos `Autenticador.Api/`, `Autenticador.Application/`, `Autenticador.Domain/`, `Autenticador.Infrastructure/`, `Autenticador.UnitTests/` e `Autenticador.IntegrationTests/` conforme `Autenticador/Autenticador.slnx`
- [X] T002 Configurar .NET 10, ASP.NET Core, Dapper, autenticação JWT e dependências xUnit nos arquivos `Autenticador.Api/Autenticador.Api.csproj`, `Autenticador.Application/Autenticador.Application.csproj`, `Autenticador.Domain/Autenticador.Domain.csproj`, `Autenticador.Infrastructure/Autenticador.Infrastructure.csproj`, `Autenticador.UnitTests/Autenticador.UnitTests.csproj` e `Autenticador.IntegrationTests/Autenticador.IntegrationTests.csproj`
- [X] T003 [P] Configurar valores por ambiente e banco de teste em `Autenticador.Api/appsettings.json`, `Autenticador.Api/appsettings.Development.json` e `Autenticador.IntegrationTests/Controllers/AuthControllerTests.cs`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Infraestrutura comum necessária aos fluxos de autenticação, autorização e inicialização.

- [X] T004 Isolar conexão SQLite e inicialização do esquema atrás das abstrações Dapper em `Autenticador.Infrastructure/Data/DatabaseConnectionFactory.cs` e registrar dependências em `Autenticador.Api/Program.cs`
- [X] T005 [P] Configurar abstrações de hashing de senha e opções JWT em `Autenticador.Infrastructure/Security/PasswordHasherAdapter.cs` e `Autenticador.Infrastructure/Security/JwtOptions.cs`
- [X] T006 [P] Definir respostas HTTP de erro e ProblemDetails compartilhados em `Autenticador.Api/Extensions/ProblemDetailsExtensions.cs`
- [X] T007 Criar entidades e contratos de repositório de usuário e credencial de serviço em `Autenticador.Domain/Entities/Usuario.cs`, `Autenticador.Domain/Entities/CredencialDeServico.cs`, `Autenticador.Application/Interfaces/IUsuarioRepository.cs` e `Autenticador.Application/Interfaces/ICredencialDeServicoRepository.cs`

**Checkpoint**: A infraestrutura comum está pronta; as histórias podem ser entregues em incrementos independentes.

---

## Phase 3: User Story 1 - Autenticar usuário (Priority: P1) 🎯 MVP

**Goal**: Autenticar contas ativas, emitir JWT com validade de 60 minutos e aplicar bloqueio progressivo após falhas consecutivas.

**Independent Test**: Usar a conta ativa de teste em `Autenticador.IntegrationTests/Fixtures/TestAccountData.cs` para verificar login válido, rejeição uniforme, expiração do token, bloqueio no terceiro erro, escalada até 24 horas, liberação automática e reset após login válido.

### Tests for User Story 1

- [X] T008 [P] [US1] Testar contrato HTTP de sucesso e credenciais inválidas em `Autenticador.IntegrationTests/Controllers/AuthControllerTests.cs`
- [X] T009 [P] [US1] Testar verificação de senha, bloqueio básico e conta inativa em `Autenticador.UnitTests/Services/AuthServiceTests.cs`
- [ ] T010 [P] [US1] Adicionar testes unitários com relógio controlável para primeiro bloqueio de 10 minutos, segundo de 20 minutos, progressão dobrada limitada a 24 horas, três falhas por bloqueio, falha durante bloqueio sem alteração de estado e reset após login válido em `Autenticador.UnitTests/Services/AuthServiceTests.cs`
- [ ] T011 [P] [US1] Verificar no contrato HTTP que respostas para conta bloqueada continuam equivalentes às demais credenciais inválidas e não expõem duração ou contadores em `Autenticador.IntegrationTests/Controllers/AuthControllerTests.cs`
- [ ] T012 [P] [US1] Testar migração de esquema existente e incrementos concorrentes de falhas sem perda de atualizações em `Autenticador.IntegrationTests/Repositories/UsuarioRepositoryTests.cs`
- [ ] T013 [US1] Verificar que JWT emitido expira exatamente 60 minutos após emissão e não contém claims além da identidade e validade em `Autenticador.UnitTests/Services/AuthServiceTests.cs`

### Implementation for User Story 1

- [X] T014 [US1] Criar a entidade de usuário com login único, hash, atividade, data de criação, falhas e prazo de bloqueio em `Autenticador.Domain/Entities/Usuario.cs`
- [X] T015 [US1] Implementar DTOs e validação de login e senha obrigatórios, sem espaços e com comparação de login sem distinção de caixa em `Autenticador.Api/Models/LoginRequest.cs` e `Autenticador.Api/Models/LoginResponse.cs`
- [X] T016 [US1] Implementar hash de senha não reversível com salt em `Autenticador.Infrastructure/Security/PasswordHasherAdapter.cs`
- [X] T017 [US1] Implementar consulta e persistência de usuário e estado de falhas em `Autenticador.Infrastructure/Repositories/UsuarioRepository.cs`
- [X] T018 [US1] Implementar autenticação uniforme para credenciais inválidas e emissão JWT após validar conta ativa em `Autenticador.Application/Services/AuthService.cs`
- [X] T019 [US1] Implementar `POST /api/auth/login` com respostas HTTP e erros ProblemDetails do contrato em `Autenticador.Api/Controllers/AuthController.cs`
- [X] T020 [US1] Registrar autenticação, JWT e serviços no contêiner de dependências em `Autenticador.Api/Program.cs`
- [ ] T021 [US1] Adicionar `QuantidadeBloqueiosConsecutivos` à entidade, mantendo `QuantidadeFalhas` como falhas desde o bloqueio e zerando a sequência apenas no login válido, em `Autenticador.Domain/Entities/Usuario.cs`
- [ ] T022 [US1] Criar migração aditiva e idempotente para adicionar `QuantidadeBloqueiosConsecutivos INTEGER NOT NULL DEFAULT 0` à tabela `Usuarios`, preservando registros existentes, em `Autenticador.Infrastructure/Data/DatabaseConnectionFactory.cs`
- [ ] T023 [US1] Persistir a nova coluna nas operações de leitura, inserção e atualização e criar operação atômica para registrar falha, aplicar bloqueio e resetar contadores em `Autenticador.Infrastructure/Repositories/UsuarioRepository.cs` e `Autenticador.Application/Interfaces/IUsuarioRepository.cs`
- [ ] T024 [US1] Injetar `TimeProvider` e usar o mesmo instante UTC para verificar bloqueio, calcular expiração e testar a política sem aguardar tempo real em `Autenticador.Application/Services/AuthService.cs` e `Autenticador.Api/Program.cs`
- [ ] T025 [US1] Implementar política de três falhas consecutivas: primeiro bloqueio de 10 minutos, duração duplicada a cada bloqueio consecutivo até 24 horas, recusa durante bloqueio sem mutação e reset de ambos os contadores após login válido em `Autenticador.Application/Services/AuthService.cs`
- [ ] T026 [US1] Garantir que o token e `expiraEm` usam validade exata de 60 minutos conforme a configuração da feature em `Autenticador.Infrastructure/Security/JwtTokenFactory.cs` e `Autenticador.Api/appsettings.json`

**Checkpoint**: US1 funciona isoladamente e satisfaz os cenários de autenticação, validade do JWT e bloqueio progressivo.

---

## Phase 4: User Story 2 - Consultar existência e atividade (Priority: P2)

**Goal**: Permitir que apenas consumidores autorizados consultem se uma conta existe e está ativa, sem expor outros dados.

**Independent Test**: Consultar contas ativa, inativa e inexistente com credencial de serviço; confirmar resposta apenas com `existe` e `ativa` e negar acesso sem credencial válida.

### Tests for User Story 2

- [X] T027 [P] [US2] Testar respostas autorizadas e não autorizadas do contrato `GET /api/auth/usuarios/{login}/status` em `Autenticador.IntegrationTests/Controllers/AuthControllerTests.cs`
- [X] T028 [P] [US2] Testar mapeamento de consulta para conta ativa, inativa e inexistente em `Autenticador.UnitTests/Services/UsuarioStatusServiceTests.cs`

### Implementation for User Story 2

- [X] T029 [US2] Implementar entidade e contrato de autorização da credencial de serviço em `Autenticador.Domain/Entities/CredencialDeServico.cs` e `Autenticador.Application/Interfaces/ICredencialDeServicoRepository.cs`
- [X] T030 [US2] Implementar consulta de estado que retorna somente existência e atividade em `Autenticador.Infrastructure/Repositories/UsuarioRepository.cs` e `Autenticador.Application/Services/UsuarioStatusService.cs`
- [X] T031 [US2] Implementar `GET /api/auth/usuarios/{login}/status` e resposta restrita a `existe` e `ativa` em `Autenticador.Api/Controllers/UsuarioStatusController.cs` e `Autenticador.Api/Models/UsuarioStatusResponse.cs`

**Checkpoint**: US2 é verificável sem expor senha, hash ou dados pessoais.

---

## Phase 5: User Story 3 - Usar contas de teste após inicialização (Priority: P3)

**Goal**: Disponibilizar exatamente três contas de teste somente em Development e Testing, sem duplicação e sem substituir usuários existentes.

**Independent Test**: Inicializar em Development/Testing, confirmar três contas com ao menos duas ativas e uma inativa, reiniciar cinco vezes sem duplicatas e confirmar que produção não cria contas.

### Tests for User Story 3

- [X] T032 [P] [US3] Testar seed, idempotência e ausência de criação de contas em produção em `Autenticador.IntegrationTests/Startup/TestDataInitializerTests.cs`

### Implementation for User Story 3

- [X] T033 [US3] Criar seed das três contas conhecidas somente em Development e Testing, sem substituir logins existentes, em `Autenticador.Infrastructure/Data/DatabaseInitializer.cs`
- [X] T034 [US3] Registrar inicialização de banco e seed conforme ambiente em `Autenticador.Api/Program.cs`
- [X] T035 [US3] Definir fixtures com contas de teste ativas e inativa em `Autenticador.IntegrationTests/Fixtures/TestAccountData.cs` e `Autenticador.IntegrationTests/Fixtures/TestAccountsFixture.cs`

**Checkpoint**: US3 mantém o seed restrito aos ambientes permitidos e idempotente.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Corrigir compatibilidade de Swagger/OpenAPI e executar validação integrada de todas as histórias.

- [X] T036 [P] Revisar segurança da autenticação, hashing e claims JWT em `Autenticador.Application/Services/AuthService.cs`, `Autenticador.Infrastructure/Security/PasswordHasherAdapter.cs` e `Autenticador.Infrastructure/Security/JwtTokenFactory.cs`
- [X] T037 [P] Reforçar validação dos pedidos e garantir que logs não registrem segredos em `Autenticador.Api/Models/LoginRequest.cs`, `Autenticador.Api/Controllers/AuthController.cs` e `Autenticador.Api/Controllers/UsuarioStatusController.cs`
- [ ] T038 [P] Criar teste de inicialização em Development que inicia o host e valida HTTP 200 e JSON OpenAPI válido em `/swagger/v1/swagger.json` em `Autenticador.IntegrationTests/Startup/SwaggerStartupTests.cs`
- [ ] T039 Atualizar `Swashbuckle.AspNetCore` para versão estável 10.0.0 ou posterior compatível com .NET 10 no arquivo `Autenticador.Api/Autenticador.Api.csproj`; manter os componentes Swagger na mesma versão resolvida e corrigir usos OpenAPI incompatíveis com v2 em `Autenticador.Api/Program.cs`
- [ ] T040 Verificar que o grafo NuGet e as assemblies efetivamente carregadas usam Swashbuckle 10+ compatível com Microsoft.OpenApi 2.x e que a API inicia sem `TypeLoadException` em `Autenticador.Api/Autenticador.Api.csproj` e `Autenticador.Api/bin/Debug/net10.0/Autenticador.Api.deps.json`
- [X] T041 [P] Documentar os cenários executáveis de JWT, bloqueio escalonado e Swagger em `specs/001-jwt-user-auth/quickstart.md`
- [ ] T042 Executar o quickstart completo e todos os testes xUnit da solução `Autenticador/Autenticador.slnx`, corrigindo falhas relacionadas a esta feature

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: concluído; sem dependências.
- **Foundational (Phase 2)**: concluído; bloqueava todas as histórias.
- **User Story 1 (Phase 3)**: depende da fundação; bloqueia somente a entrega dos fluxos que compartilham autenticação, mas não impede a validação isolada de US2/US3.
- **User Story 2 (Phase 4)**: depende da fundação; pode ser validada independentemente de US1.
- **User Story 3 (Phase 5)**: depende da fundação; pode ser validada independentemente de US1/US2.
- **Polish (Phase 6)**: tarefa T038 deve preceder T039; T042 depende da conclusão das alterações de bloqueio e Swagger e da revisão do quickstart.

### User Story Dependencies

- **US1 (P1)**: iniciar após a fundação; inclui migração compatível com base existente e regra de bloqueio progressivo.
- **US2 (P2)**: após a fundação; não exige completar US1 para testar a consulta.
- **US3 (P3)**: após a fundação; não exige completar US1/US2 para testar o seed.

### Parallel Opportunities

- US1: T010, T011 e T012 atuam em arquivos de teste diferentes e podem ser iniciados em paralelo; T013 depende da preparação do relógio e da fábrica de token; T021 depende dos testes; T022 depende de T021; T023 depende de T021/T022; T024 e T026 podem ocorrer em paralelo após os testes; T025 depende de T023/T024.
- US2: T027 e T028 podem ser desenvolvidos em paralelo; implementação já concluída.
- US3: T032 e as alterações da implementação de seed podem ser trabalhadas em arquivos separados; implementação já concluída.
- Polish: T038 e T041 podem ocorrer em paralelo; T039 depende de T038; T040 depende de T039; T042 é a validação final.

## Parallel Example: User Story 1

```text
Implementar em paralelo:
- T010: testes unitários da progressão de bloqueios em Autenticador.UnitTests/Services/AuthServiceTests.cs
- T011: teste do contrato HTTP para conta bloqueada em Autenticador.IntegrationTests/Controllers/AuthControllerTests.cs
- T012: testes de migração e concorrência em Autenticador.IntegrationTests/Repositories/UsuarioRepositoryTests.cs
```

## Implementation Strategy

- **MVP**: US1, incluindo testes da política progressiva, migração de dados e emissão JWT por 60 minutos.
- **Entrega incremental**: US2 e US3 já estão implementadas; validar de forma independente e preservar seus contratos.
- **Correção de inicialização**: concluir testes e migração do Swagger no Polish, verificando a causa do `TypeLoadException` no grafo e em runtime.
- **Validação final**: executar o quickstart e a suíte da solução somente após completar as tarefas pendentes da política de bloqueio e Swagger.

## Validation Checklist

- [X] Histórias estão ordenadas por prioridade da especificação.
- [X] Cada tarefa tem checkbox, ID sequencial, label de história somente nas fases de US e caminho de arquivo.
- [X] Tarefas de teste cobrem as regras de negócio alteradas e as superfícies HTTP/documentação relevantes.
- [X] Os passos não concluídos estão marcados com `[ ]`; trabalho já existente mantém o estado `[X]`.
