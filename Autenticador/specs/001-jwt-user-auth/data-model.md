# Data Model: Autenticação de Usuários com JWT

## Entidades

### Usuario

| Campo | Tipo | Regras |
|---|---|---|
| Id | Guid / int | Identificador único da conta |
| Login | string | Obrigatório, único, normalizado em minúsculas, sem espaços em branco |
| SenhaHash | string | Armazenado em formato hash unidirecional, nunca em texto simples |
| Ativo | bool | Determina se a conta pode autenticar |
| DataCriacao | datetime | Data de criação da conta |
| DataUltimaFalha | datetime? | Marca a última falha de autenticação |
| QuantidadeFalhas | int | Falhas consecutivas desde o último bloqueio; zera ao aplicar bloqueio ou após autenticação bem-sucedida |
| QuantidadeBloqueiosConsecutivos | int | Bloqueios consecutivos desde a última autenticação bem-sucedida; inicia em zero e é persistido entre reinicializações |
| BloqueadoAte | datetime? | Momento em que a conta volta a aceitar novas tentativas |

#### Regras de validação

- O `login` deve ser informado, possuir conteúdo útil e ser comparado sem diferenciar maiúsculas/minúsculas.
- A senha deve ser validada somente por hash, sem retorno em texto plano.
- A conta deve estar `Ativo = true` para emitir token.
- A conta deve ser rejeitada quando `BloqueadoAte > now` mesmo que a senha seja correta, até o término do bloqueio temporário.
- Cada três falhas consecutivas resultam em bloqueio; ao aplicar o bloqueio, `QuantidadeFalhas` volta a zero e `QuantidadeBloqueiosConsecutivos` é incrementado.
- A duração do primeiro bloqueio é 10 minutos; a duração seguinte dobra a cada bloqueio consecutivo e não excede 24 horas.
- Recusas durante bloqueio ativo não incrementam contadores nem alteram o prazo do bloqueio.
- Uma autenticação bem-sucedida zera `QuantidadeFalhas`, `QuantidadeBloqueiosConsecutivos`, `DataUltimaFalha` e `BloqueadoAte`.
- Atualizações concorrentes dos contadores e do prazo devem ser atômicas no repositório.
- A migração do armazenamento existente deve inicializar `QuantidadeBloqueiosConsecutivos` como zero sem perder contas nem outros estados persistidos.

### CredencialDeServico

| Campo | Tipo | Regras |
|---|---|---|
| Id | Guid / int | Identificador interno |
| Nome | string | Nome descritivo da credencial |
| ChaveSecreta | string | Valor sensível armazenado em ambiente seguro |
| Ativa | bool | Habilita autorização para consulta |

#### Regras de validação

- A autorização de consulta por login deve exigir credencial de serviço válida.
- Usuários comuns não podem consultarem a existência/atividade de outras contas.

### ResultadoConsultaLogin

| Campo | Tipo | Regras |
|---|---|---|
| Existe | bool | Indica se a conta existe |
| Ativa | bool | Indica se a conta está ativa |

#### Regras de validação

- Para login inexistente: `Existe = false`, `Ativa = false`.
- Para conta inativa: `Existe = true`, `Ativa = false`.
- Nenhum campo adicional deve ser retornado.

### ResultadoAutenticacao

| Campo | Tipo | Regras |
|---|---|---|
| Token | string | JWT válido emitido somente para credenciais corretas |
| ExpiraEm | datetime | Data/hora de expiração do token |

#### Regras de validação

- O token deve conter somente a identidade do usuário e a validade.
- O token não pode conter senha, hash ou dados extras de autorização.
- O token expira após 60 minutos da emissão.

## Estados e transições

### Estado da conta

- `Ativa`: aceita autenticação quando a senha coincide e o lockout não está ativo.
- `Inativa`: não aceita autenticação; consulta retorna `Existe = true` e `Ativa = false`.
- `EmBloqueioTemporario`: após cada grupo de três falhas consecutivas, a conta fica bloqueada por `min(10 minutos × 2^(QuantidadeBloqueiosConsecutivos - 1), 24 horas)`. A função de autenticação recusa tentativas até `BloqueadoAte`; o término libera novas tentativas sem apagar a sequência de bloqueios.

### Fluxo principal

1. Usuário tenta autenticar com login e senha.
2. Sistema valida formato e login sem distinção de caixa.
3. Sistema verifica existência, estado e tempo de bloqueio.
4. Sistema valida hash da senha.
5. Se válida e ativa: emite token JWT de 60 minutos e zera os contadores e o estado de bloqueio.
6. Se inválida: incrementa as falhas; na terceira falha, incrementa a sequência de bloqueios, define o prazo escalonado e zera as falhas correntes.

## Regras de negócio críticas

- Login e senha faltando, vazios ou compostos somente por espaços devem ser rejeitados antes da autenticação.
- Falhas para login inexistente e senha incorreta devem produzir a mesma resposta externa.
- Contas de teste devem ter nomes conhecidos e unívocos para validação e não podem ser criadas em produção.
- A inicialização repetida deve idempotência: se a conta existir, a criação é ignorada.
