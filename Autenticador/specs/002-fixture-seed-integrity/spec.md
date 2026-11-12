# Feature Specification: Integridade das Contas de Teste

**Feature Branch**: `002-fixture-seed-integrity`

**Created**: 2026-11-12

**Status**: Draft

**Input**: User description: "Renomear os arquivos e tipos propostos para português. Definir o comportamento de colisão — por exemplo, usar um login alternativo determinístico para a fixture — preservando contas reais e garantindo três contas de teste. Sempre seguir o @constitution.md. Nunca fazer que vá de encontro a ele."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Disponibilizar contas de teste sem afetar contas reais (Priority: P1)

Uma pessoa desenvolvedora ou responsável por testes precisa iniciar a aplicação com três contas de teste conhecidas, mesmo quando os logins preferenciais já pertencem a contas reais.

**Why this priority**: As contas de teste são necessárias para validar os fluxos existentes, e uma colisão não pode sobrescrever nem remover dados reais.

**Independent Test**: Preparar contas reais com logins iguais aos preferenciais das fixtures, iniciar em ambiente de desenvolvimento ou teste e verificar a disponibilidade das três fixtures e a preservação integral das contas preexistentes.

**Acceptance Scenarios**:

1. **Given** que o login preferencial de uma fixture pertence a uma conta real, **When** as contas de teste são preparadas, **Then** a conta real permanece inalterada e a fixture recebe um login alternativo determinístico.
2. **Given** que o login preferencial e o primeiro alternativo já pertencem a contas reais, **When** as contas de teste são preparadas, **Then** a fixture recebe o próximo login alternativo livre, seguindo a sequência determinística `<login>.teste`, `<login>.teste-2`, `<login>.teste-3` e assim por diante.
3. **Given** que as três fixtures já foram preparadas com logins alternativos, **When** a preparação é repetida, **Then** cada fixture mantém sua identidade e login, não são criadas duplicatas e nenhuma conta real é alterada.
4. **Given** que a aplicação inicia em ambiente de produção, **When** a inicialização é concluída, **Then** nenhuma conta de teste é criada.

### User Story 2 - Manter nomes de código em conformidade com a constituição (Priority: P2)

Uma pessoa mantenedora precisa encontrar os testes propostos por nomes em português e garantir que os artefatos planejados respeitem a constituição do projeto.

**Why this priority**: Nomes coerentes tornam os artefatos compreensíveis e evitam que o planejamento contradiga uma regra obrigatória do projeto.

**Independent Test**: Revisar os nomes de arquivos e tipos propostos para os testes de repositório e de inicialização e confirmar que estão em português e que cada arquivo de código declara no máximo um tipo de nível superior.

**Acceptance Scenarios**:

1. **Given** os nomes propostos `UsuarioRepositoryTests.cs` e `SwaggerStartupTests.cs`, **When** o plano e as tarefas forem atualizados, **Then** eles serão substituídos, respectivamente, por `TestesRepositorioUsuario.cs` e `TestesInicializacaoSwagger.cs`, com tipos de mesmo nome.
2. **Given** qualquer novo arquivo ou tipo proposto neste escopo, **When** sua conformidade é revisada, **Then** seu nome está em português e cada arquivo de código contém no máximo um tipo de nível superior.
3. **Given** uma alteração de regra de negócio ou endpoint neste escopo, **When** ela é concluída, **Then** sua cobertura de testes atende às obrigações da constituição para testes unitários e de integração.

### Edge Cases

- O login alternativo inicial também pode estar ocupado por uma conta real; a busca deve continuar pela sequência numérica sem modificar essa conta.
- Uma inicialização posterior deve reconhecer cada fixture pela sua identidade estável, mesmo que o login tenha sido alternado por colisão.
- Logins que diferem apenas por maiúsculas/minúsculas devem ser tratados como colisões conforme as regras de comparação de login do sistema existente.
- Uma fixture previamente identificada não deve ser confundida com uma conta real que tenha o mesmo login preferencial.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: O sistema MUST disponibilizar exatamente três identidades de contas de teste em ambientes de desenvolvimento e teste.
- **FR-002**: Cada fixture MUST possuir uma identidade estável, independente do login escolhido para ela, para que inicializações repetidas reconheçam a mesma conta.
- **FR-003**: Quando um login preferencial estiver ocupado por uma conta que não seja a fixture correspondente, o sistema MUST preservar essa conta sem alterar seus dados, removê-la, desativá-la ou identificá-la como fixture.
- **FR-004**: Para resolver uma colisão, o sistema MUST escolher o primeiro login livre na sequência `<login>.teste`, `<login>.teste-2`, `<login>.teste-3` e números inteiros crescentes.
- **FR-005**: A escolha do login alternativo MUST ser determinística e permanecer estável para a mesma fixture em inicializações posteriores.
- **FR-006**: A preparação das fixtures MUST ser idempotente: executar novamente não cria identidades duplicadas nem altera contas reais.
- **FR-007**: O sistema MUST NOT criar contas de teste em ambiente de produção.
- **FR-008**: Os nomes dos arquivos e tipos propostos para os testes de repositório e de inicialização MUST ser `TestesRepositorioUsuario.cs` / `TestesRepositorioUsuario` e `TestesInicializacaoSwagger.cs` / `TestesInicializacaoSwagger`, respectivamente.
- **FR-009**: Outros arquivos e tipos propostos neste escopo MUST ser nomeados em português e cada arquivo de código MUST declarar no máximo um tipo de nível superior, conforme a constituição do projeto.
- **FR-010**: Cada regra de negócio alterada MUST ter testes unitários, e cada endpoint alterado MUST ter testes de integração que cubram sucesso e erro, verificando o contrato HTTP conforme a constituição do projeto.

### Key Entities

- **Fixture de conta de teste**: Uma das três identidades de conta reservadas para validações automatizadas, com identidade estável e login escolhido sem colisão.
- **Conta real preexistente**: Conta existente que não corresponde à identidade de uma fixture e cujos dados e estado devem ser preservados.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Em 100% das inicializações nos ambientes de desenvolvimento e teste, as três identidades de fixtures estão disponíveis, inclusive quando todos os logins preferenciais colidem com contas reais.
- **SC-002**: Em 100% dos cenários de colisão testados, nenhuma conta real preexistente é alterada, removida, desativada ou convertida em fixture.
- **SC-003**: Após pelo menos cinco inicializações consecutivas com a mesma base de dados, continuam disponíveis exatamente três identidades de fixture, com logins estáveis e sem duplicatas.
- **SC-004**: A revisão dos artefatos desta funcionalidade confirma que todos os nomes de arquivos e tipos propostos estão em português e respeitam o limite constitucional de um tipo de nível superior por arquivo de código.
- **SC-005**: 100% das regras de negócio alteradas têm cobertura unitária e 100% dos endpoints alterados têm cobertura de integração para sucesso e erro.

## Assumptions

- As três identidades lógicas das fixtures e seus logins preferenciais já são definidos pela funcionalidade de autenticação existente.
- Uma conta preexistente só é reconhecida como fixture quando corresponde à identidade estável daquela fixture; a coincidência de login, isoladamente, não é suficiente.
- A criação de fixtures permanece restrita aos ambientes de desenvolvimento e teste; sua disponibilidade em produção está fora do escopo.
- As regras existentes de normalização e comparação de logins continuam vigentes.
