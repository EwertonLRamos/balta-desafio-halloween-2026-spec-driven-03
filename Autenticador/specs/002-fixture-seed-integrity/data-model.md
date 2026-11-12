# Data Model: Fixture de contas de teste

## 1. Usuario

| Campo | Tipo | Obrigatório | Descrição |
|---|---|---:|---|
| Id | string | Sim | Identificador interno do usuário; persiste entre reinicializações e pode ser usado para reconhecer a conta real ou a fixture. |
| Login | string | Sim | Login do usuário, normalizado e comparado com `StringComparer.OrdinalIgnoreCase`. |
| SenhaHash | string | Sim | Hash da senha, nunca armazenado em texto claro. |
| Ativo | bool | Sim | Indica se a conta pode autenticar. |
| DataCriacao | DateTime | Sim | Instante em que a conta foi criada. |
| DataUltimaFalha | DateTime? | Não | Última falha de autenticação. |
| QuantidadeFalhas | int | Sim | Número de falhas consecutivas. |
| BloqueadoAte | DateTime? | Não | Data/hora do bloqueio por excesso de falhas. |

### Regras de validação
- `Login` deve ser não nulo, não vazio e trimado antes da comparação.
- O login deve ser tratado em comparação case-insensitive para evitar colisões por maiúsculas/minúsculas.
- Contas reais e fixtures devem ser distinguíveis por um identificador persistido, não pelo login.

### Estado e transições
- `Inativo` (registro existe, mas `Ativo == false`) pode continuar em base de dados sem ser confundido com fixture.
- `Bloqueado` é um estado transitório causado por falhas consecutivas.
- `Autenticado` não altera a identidade da fixture; apenas reseta falhas e bloqueio quando a autenticação tiver sucesso.

## 2. FixtureDeContaTeste

| Campo | Tipo | Obrigatório | Descrição |
|---|---|---:|---|
| Id | string | Sim | Identidade persistida da fixture, usada para identificar a mesma conta em futuras inicializações. |
| LoginPreferencial | string | Sim | Login base da fixture, por exemplo `admin@teste`. |
| LoginAtual | string | Sim | Login efetivo da conta após resolução de colisão. Pode ser `admin@teste`, `admin@teste-2` ou outro alternativo. |
| Ambiente | string | Sim | Ambiente de execução em que a fixture deve existir (`Development` ou `Testing`). |
| Ativa | bool | Sim | Determina se a fixture está habilitada no ambiente atual. |

### Regras de validação
- A camada de inicialização deve considerar a fixture por `Id`, não por `LoginAtual`.
- O `LoginAtual` deve ser escolhido deterministically com a sequência: `<login>.teste`, `<login>.teste-2`, `<login>.teste-3`...
- Se a conta real utilizar o mesmo login preferencial, a conta real deve permanecer inalterada e a fixture usa o próximo valor livre.

### Estado e transições
- `Não inicializada` → a fixture ainda não existe no banco.
- `Preparada` → a fixture existe e foi reconhecida por identidade persistida.
- `Reaproveitada` → a inicialização detecta que a fixture já foi criada e não gera duplicata.
- `Colidida` → conforme o login preferencial esteja ocupado, a fixture recebe o próximo login livre sem conflitos.

## 3. CredencialDeServico

| Campo | Tipo | Obrigatório | Descrição |
|---|---|---:|---|
| Id | string | Sim | Identificador da credencial de serviço. |
| Nome | string | Sim | Nome da credencial, por exemplo `api-interna`. |
| ChaveSecreta | string | Sim | Segredo compartilhado usado para autenticar chamadas do status de usuário. |
| Ativa | bool | Sim | Booleano que desativa ou ativa a credencial. |

### Regras de validação
- Uma credencial de serviço deve ser criada apenas se ainda não existir para o mesmo nome.
- A validação da credencial permanece na camada de HTTP e usa comparação de tempo constante.

## Relacionamentos
- `Usuario` é a entidade principal para autenticação; `CredencialDeServico` é independente e administra acesso ao endpoint de status.
- As fixtures devem ser modeladas como instâncias de `Usuario` com identidade persistida, e não como subtipos separadores em tabelas distintas.

## Observações de design
- O banco já fornece uma tabela `Usuarios`; a solução preferida é manter esse modelo e ampliar a lógica na `DatabaseInitializer` sem criar nova estrutura de dados para a feature.
- A comparação de login deve usar a mesma regra do sistema existente (`OrdinalIgnoreCase`), a fim de evitar colisões ocultas por caixa de texto.
