# Implementation Plan: Autenticação de Usuários com JWT

**Branch**: `001-jwt-user-auth` | **Date**: 2026-11-12 | **Spec**: `/specs/001-jwt-user-auth/spec.md`

**Input**: Feature specification from `/specs/001-jwt-user-auth/spec.md`

## Summary

Implementar uma API HTTP em .NET 10 para autenticar usuários por login/senha, emitir JWT válido por uma hora com a identidade do usuário, consultar atividade por login com autorização de serviço e inicializar três contas de teste somente em ambientes de desenvolvimento/teste. O plano também corrige a política de bloqueio: três falhas levam a 10 minutos de bloqueio, a duração dobra a cada novo bloqueio até 24 horas, e login válido reinicia a sequência. Para resolver a falha `TypeLoadException` em `AddSwaggerGen`, alinhar Swashbuckle à versão 10 compatível com .NET 10 e com Microsoft.OpenApi 2.x, verificar assemblies resolvidas e validar a geração OpenAPI.

## Technical Context

**Language/Version**: C# / .NET 10

**Primary Dependencies**: ASP.NET Core 10, Dapper, JWT (Microsoft.AspNetCore.Authentication.JwtBearer), Swashbuckle.AspNetCore 10.0.0 ou posterior compatível com .NET 10 (família Swagger alinhada), xUnit, FluentAssertions (or equivalent) e Microsoft.NET.Test.Sdk

**Storage**: Banco relacional acessado por abstrações de persistência com Dapper; implementação atual usa SQLite. Preservar dados existentes ao adicionar o estado persistido da sequência de bloqueios e manter seed de usuários apenas em desenvolvimento/teste.

**Testing**: xUnit para testes unitários e de integração da API; testes de contrato HTTP com status e body de erro; relógio controlável para validar expiração de token e escalada/reset de bloqueios sem esperas reais

**Target Platform**: API server executando em Linux/macOS/Windows via .NET 10, com foco em ambiente de desenvolvimento, teste e produção controlada

**Project Type**: web-service

**Performance Goals**: autenticação em <250 ms p95 em carga leve; suporte a dezenas de usuários com latência aceitável para endpoints de autenticação e consulta de status

**Constraints**: senhas nunca em texto puro; respostas de autenticação equivalentes para login inexistente e senha incorreta; JWT válido por 60 minutos; bloqueio após três falhas consecutivas com backoff exponencial desde 10 minutos até 24 horas, reiniciado por login válido; contas de teste só criadas em desenvolvimento/teste; autorização por credencial de serviço para consulta de existência/atividade; dependências Swagger/OpenAPI binariamente compatíveis com .NET 10

**Scale/Scope**: API com poucos usuários pré-definidos e fluxos de autenticação bem delimitados; sem necessidade de multi-tenant ou grande volume de dados inicial. Disponibilidade/SLO e observabilidade avançada não foram especificados e não fazem parte deste escopo.

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

**Compatibilidade Swagger/OpenAPI**: O grafo NuGet observado resolve `Microsoft.OpenApi` 2.12.2 via `Microsoft.AspNetCore.OpenApi` 10.0.14 junto com Swashbuckle 9.0.3. A assinatura binária de Microsoft.OpenApi 2.x não é compatível com Swashbuckle 9; isso é a causa provável da falha de carregamento em `AddSwaggerGen`. Atualizar o pacote meta `Swashbuckle.AspNetCore` para a versão estável 10.0.0+ compatível com .NET 10, manter `Swagger`, `SwaggerGen` e `SwaggerUI` na mesma família, verificar o grafo e as assemblies de runtime, e aplicar os ajustes de API OpenAPI v1→v2 somente onde necessários. Não adicionar referência direta a Microsoft.OpenApi sem justificativa.

**Política de bloqueio**: Persistir separadamente as falhas consecutivas desde o último bloqueio e a quantidade de bloqueios consecutivos desde um login válido. Cada bloco de três falhas aplica `min(10 minutos × 2^(quantidade de bloqueios anteriores), 24 horas)`; ao iniciar o bloqueio, zerar as falhas correntes e persistir o prazo. Recusas enquanto o bloqueio está ativo não avançam os contadores. Login válido zera ambos os contadores. Atualizações concorrentes de falhas devem ser atômicas para que solicitações simultâneas não ultrapassem a política.

**Evolução do armazenamento**: Adicionar `QuantidadeBloqueiosConsecutivos` à tabela `Usuarios` com migração aditiva e idempotente em `Autenticador.Infrastructure/Data/DatabaseConnectionFactory.cs` (ou mecanismo de migração já adotado); inicializar contas existentes com zero e preservar seus dados.

**Validação de inicialização**: Com configurações locais válidas e ambiente `Development`, iniciar a API, confirmar ausência de `TypeLoadException` em `AddSwaggerGen` e obter HTTP 200 em `/swagger/v1/swagger.json`. Incluir teste de integração de inicialização/documentação.

**Constitution Check pós-design**: PASS. A atualização major de Swashbuckle tem justificativa explícita de compatibilidade com .NET 10; a alteração da entidade e da persistência permanece encapsulada em camadas. Testes unitários da nova regra e de integração da inicialização/documentação atendem ao princípio III; demais princípios preservados.

## Complexity Tracking

Sem violações da constituição que requeiram justificativa adicional.
