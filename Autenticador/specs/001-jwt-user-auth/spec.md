# Feature Specification: Autenticação de Usuários com JWT

**Feature Branch**: `(não definido)`

**Created**: 2026-11-11

**Status**: Draft

**Input**: User description: "Uma API para autenticar usuários com login e senha e retornar um token JWT com suas informações de acesso. Ao inicializar, deve gravar três usuários para testes. Deve verificar se um usuário existe e está ativo. As senhas devem ser protegidas na base."

## Clarifications

### Session 2026-11-11

- Q: Em quais ambientes as três contas de teste devem ser criadas automaticamente? → A: Somente em desenvolvimento e teste.
- Q: Quem deve poder consultar se um login existe e está ativo? → A: Apenas consumidores autorizados por credencial de serviço.
- Q: Que informações de autorização, além da identidade, o token deve incluir? → A: Somente a identidade do usuário.
- Q: Como a API deve reagir a muitas tentativas de login malsucedidas? → A: Pausar temporariamente novas tentativas após várias falhas, sem bloqueio permanente.

### Session 2026-11-12

- Q: Após quantas tentativas consecutivas malsucedidas a conta deve ser bloqueada temporariamente, e por quanto tempo? → A: 3 falhas consecutivas; primeiro bloqueio de 10 minutos.
- Q: Quando os 10 minutos de bloqueio terminarem, o contador de falhas deve ser zerado ou mantido? → A: Após cada bloqueio, permitir mais três falhas consecutivas; cada novo bloqueio dobra a duração, até o limite de 24 horas.
- Q: Se o usuário conseguir entrar com sucesso depois de um bloqueio, a duração do próximo bloqueio deve voltar a 10 minutos? → A: Sim. Um login válido zera as falhas e reinicia a duração para 10 minutos.
- Q: Por quanto tempo um token JWT de acesso deve continuar válido após o login? → A: 1 hora.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Autenticar usuário (Priority: P1)

Um usuário com uma conta ativa informa seu login e senha para receber um token JWT temporário que comprova sua identidade e permite acessar recursos que aceitam usuários autenticados.

**Why this priority**: A emissão de credenciais para usuários válidos é o propósito central do serviço.

**Independent Test**: Enviar credenciais válidas de uma conta de teste ativa e confirmar que o resultado contém um token JWT utilizável e sem expor a senha.

**Acceptance Scenarios**:

1. **Given** uma conta de teste existente, ativa e com credenciais válidas, **When** o usuário solicita autenticação, **Then** recebe um token JWT temporário com sua identidade.
2. **Given** um login inexistente ou uma senha incorreta, **When** o usuário solicita autenticação, **Then** a autenticação é recusada e nenhum token é emitido.
3. **Given** uma conta existente, mas inativa, **When** o usuário solicita autenticação, **Then** a autenticação é recusada e nenhum token é emitido.
4. **Given** um token JWT emitido após autenticação bem-sucedida, **When** seu prazo de validade termina, **Then** ele deixa de ser aceito como credencial de acesso.
5. **Given** dois pedidos de autenticação, um com login inexistente e outro com senha incorreta, **When** ambos são recusados, **Then** as respostas não permitem distinguir qual credencial estava incorreta.
6. **Given** um pedido sem login ou senha, **When** o usuário solicita autenticação, **Then** o pedido é recusado e nenhum token é emitido.
7. **Given** três tentativas consecutivas malsucedidas para uma conta sem bloqueios anteriores, **When** a terceira falha ocorre, **Then** novas tentativas ficam bloqueadas por 10 minutos e são liberadas automaticamente, sem bloqueio permanente da conta.
8. **Given** que um bloqueio anterior terminou, **When** ocorrem outras três falhas consecutivas, **Then** a conta é bloqueada novamente pelo dobro da duração do bloqueio anterior, até o limite de 24 horas.
9. **Given** uma conta que já sofreu bloqueios crescentes, **When** uma autenticação é bem-sucedida, **Then** a contagem de falhas é zerada e a duração de um bloqueio futuro volta a 10 minutos.
10. **Given** uma conta durante um bloqueio ativo, **When** uma nova tentativa de autenticação é recebida, **Then** a tentativa é recusada sem prolongar o prazo nem alterar os contadores.

---

### User Story 2 - Consultar existência e atividade (Priority: P2)

Um consumidor autorizado informa o login de uma pessoa para verificar se a conta existe e está ativa, sem autenticar essa pessoa nem obter seus dados de acesso.

**Why this priority**: Permite que consumidores verifiquem a elegibilidade da conta sem conceder credenciais de acesso.

**Independent Test**: Um consumidor autorizado consulta logins de contas ativas, inativas e inexistentes e confirma que cada resposta informa apenas existência e atividade; um consumidor sem autorização não obtém esses indicadores.

**Acceptance Scenarios**:

1. **Given** uma conta existente e ativa, **When** o consumidor verifica o login, **Then** recebe os indicadores de existência e atividade como verdadeiros.
2. **Given** uma conta existente e inativa, **When** o consumidor verifica o login, **Then** recebe existência verdadeira e atividade falsa.
3. **Given** um login sem conta correspondente, **When** o consumidor verifica o login, **Then** recebe existência falsa e atividade falsa, sem dados pessoais adicionais.
4. **Given** um consumidor sem autorização, **When** tenta consultar um login, **Then** não recebe os indicadores de existência ou atividade.

---

### User Story 3 - Usar contas de teste após inicialização (Priority: P3)

Uma pessoa desenvolvedora ou responsável por testes inicia o serviço e encontra três contas de teste disponíveis para exercitar autenticação e consulta de atividade.

**Why this priority**: Contas previsíveis permitem demonstrar e validar os fluxos principais sem cadastrar usuários manualmente.

**Independent Test**: Inicializar o serviço em ambiente de desenvolvimento ou teste e verificar que há três contas de teste, incluindo contas ativas e uma inativa, prontas para os cenários previstos.

**Acceptance Scenarios**:

1. **Given** uma instalação sem as contas de teste, **When** o serviço é inicializado em ambiente de desenvolvimento ou teste, **Then** as três contas de teste ficam disponíveis.
2. **Given** que as contas de teste já foram criadas, **When** o serviço é inicializado novamente, **Then** não são criadas cópias adicionais dessas contas.
3. **Given** que o serviço é inicializado em produção, **When** as contas de teste não existem, **Then** nenhuma conta de teste é criada.

---

### Edge Cases

- Login ou senha ausentes, vazios ou compostos apenas por espaços devem ser rejeitados sem emitir token.
- Variações de maiúsculas e minúsculas no login devem seguir uma regra consistente; assume-se comparação sem distinção entre maiúsculas e minúsculas.
- Uma conta desativada entre duas solicitações não pode obter um novo token após ser desativada.
- Respostas de autenticação não devem distinguir se a falha ocorreu porque o login não existe ou porque a senha está incorreta.
- Após três tentativas consecutivas de autenticação malsucedidas para uma conta, novas tentativas devem ser bloqueadas por 10 minutos na primeira ocorrência. Após cada bloqueio, o contador de falhas é reiniciado; cada novo grupo de três falhas consecutivas dobra a duração do bloqueio anterior, até o máximo de 24 horas. Um login válido zera as falhas e reinicia a duração para 10 minutos.
- Tentativas recebidas enquanto um bloqueio está ativo devem ser recusadas sem alterar os contadores nem prolongar o bloqueio.
- A consulta de existência deve retornar indicadores falsos para login desconhecido e não deve revelar atributos adicionais da conta.
- As contas de teste não devem ser criadas em ambiente de produção nem substituir contas reais existentes.
- A aplicação deve iniciar corretamente com configurações válidas e disponibilizar a documentação da API no ambiente de desenvolvimento.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: O serviço MUST aceitar login e senha para autenticar um usuário.
- **FR-002**: O serviço MUST emitir, por uma hora, um token JWT contendo somente a identidade do usuário e sua validade, somente quando a conta existir, estiver ativa e a senha corresponder.
- **FR-003**: O serviço MUST recusar a autenticação para credenciais inválidas ou contas inativas e MUST NOT emitir token nesses casos.
- **FR-004**: O token MUST conter um prazo de validade e MUST NOT conter a senha nem seu valor protegido.
- **FR-005**: O serviço MUST oferecer a consumidores autorizados por credencial de serviço uma consulta de conta por login que informe separadamente se a conta existe e se está ativa, sem exigir a senha do usuário consultado.
- **FR-006**: Para login inexistente, a consulta MUST informar existência falsa e atividade falsa; para conta existente e inativa, MUST informar existência verdadeira e atividade falsa.
- **FR-007**: A consulta de existência e atividade MUST NOT retornar senha, informações de autorização ou outros dados pessoais.
- **FR-008**: O serviço MUST disponibilizar exatamente três contas de teste, incluindo ao menos duas contas ativas e uma inativa, exclusivamente em ambientes de desenvolvimento e teste, e MUST NOT criá-las em produção.
- **FR-009**: A inicialização repetida MUST manter as três contas de teste sem duplicá-las nem substituir contas reais.
- **FR-010**: Senhas MUST ser armazenadas em forma protegida não reversível, adequada à verificação de credenciais, e MUST NOT ser armazenadas ou retornadas em texto simples.
- **FR-011**: Falhas de autenticação MUST usar uma resposta equivalente para login inexistente e senha incorreta, evitando revelar qual caso ocorreu.
- **FR-012**: O serviço MUST recusar pedidos de autenticação com login ou senha ausentes, vazios ou compostos apenas por espaços.
- **FR-013**: O serviço MUST recusar a consulta de existência e atividade a consumidores sem autorização, sem revelar os indicadores consultados.
- **FR-014**: Após três tentativas consecutivas de autenticação malsucedidas para uma conta, o serviço MUST bloquear novas tentativas por 10 minutos na primeira ocorrência e MUST restabelecê-las automaticamente. Após cada bloqueio, o contador de falhas MUST ser reiniciado; cada novo grupo de três falhas consecutivas MUST dobrar a duração do bloqueio anterior, limitada a 24 horas. Um login válido MUST zerar as falhas e reiniciar a duração para 10 minutos. Tentativas durante bloqueio ativo MUST ser recusadas sem alterar contadores ou prazo, sem bloqueio permanente da conta.
- **FR-015**: A aplicação MUST iniciar corretamente com configurações válidas e disponibilizar a documentação da API no ambiente de desenvolvimento.

### Key Entities *(include if feature involves data)*

- **Conta de usuário**: Representa uma pessoa que pode autenticar-se; inclui login, estado de atividade e senha protegida.
- **Token de acesso**: Credencial temporária emitida para uma conta ativa após autenticação bem-sucedida; contém a identidade do usuário e sua validade, sem informações de autorização.
- **Conta de teste**: Conta previamente disponibilizada em ambiente de desenvolvimento ou teste para validar autenticação e consulta de atividade.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Em todos os testes de aceitação, credenciais válidas de uma conta ativa resultam em token JWT e credenciais inválidas ou inativas nunca resultam em token.
- **SC-002**: Em todos os testes de aceitação, a consulta identifica corretamente contas ativas, inativas e inexistentes, sem retornar dados além dos indicadores solicitados.
- **SC-003**: Em ambiente de desenvolvimento ou teste, as três contas previstas ficam disponíveis após a inicialização e permanecem sem duplicatas após pelo menos cinco reinicializações.
- **SC-004**: Em uma amostra de todas as contas armazenadas, nenhuma senha pode ser recuperada em texto simples ou aparecer em respostas da autenticação e consulta.
- **SC-005**: Pelo menos 95% de 100 solicitações de autenticação e consulta concluídas em condições normais de teste recebem resultado em até um segundo.
- **SC-006**: Em todos os testes de aceitação, cada grupo de três falhas consecutivas bloqueia novas tentativas: 10 minutos na primeira ocorrência, o dobro da duração anterior nas ocorrências seguintes, até o máximo de 24 horas; as tentativas são liberadas automaticamente ao fim do prazo, tentativas no período de bloqueio não alteram o prazo ou contadores e um login válido reinicia a duração para 10 minutos.
- **SC-007**: Em ambiente de desenvolvimento com configurações válidas, a inicialização da API tem sucesso em 100% das execuções de validação e a documentação da API retorna HTTP 200 com uma descrição válida dos endpoints.
- **SC-008**: Todo token de acesso emitido deixa de ser aceito após uma hora da emissão.

## Assumptions

- O login é único por conta e a comparação não diferencia maiúsculas de minúsculas.
- O token comprova a identidade do usuário, sem declarar papéis ou permissões; recursos que exigem autorização adicional devem obtê-la por outro meio.
- Os tokens de acesso têm validade de uma hora.
- As três contas pré-carregadas são fixtures de demonstração e ficam restritas a ambientes de desenvolvimento e teste. Ao menos uma delas é inativa para permitir validar esse fluxo.
- A expressão “senhas encriptografadas” é tratada como armazenamento protegido não reversível, apropriado para senhas; não se presume que seja possível recuperar a senha original.
- O formato detalhado das respostas e os códigos de erro serão definidos no planejamento, mantendo as diferenças funcionais descritas nesta especificação.
