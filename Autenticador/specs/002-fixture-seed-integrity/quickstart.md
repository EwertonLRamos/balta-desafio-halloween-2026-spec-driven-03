# Quickstart: validação da integridade das fixtures

## Pré-requisitos

- .NET 10 SDK instalado
- Solução `Autenticador.slnx` disponível no repositório
- Banco de dados local configurado via connection string `AutenticadorDatabase` (ou arquivo local padrão `autenticador.db`)

## Cenário 1: inicialização em ambiente de desenvolvimento

1. Configure o ambiente para `Development`.
2. Execute a API:

```bash
dotnet run --project Autenticador.Api/Autenticador.Api.csproj
```

3. Verifique no banco que existem exatamente três contas de teste disponíveis.
4. Confirme que as contas reais, caso existam com logins preferenciais idênticos, foram preservadas e não alteradas.

### Resultado esperado
- A aplicação sobe sem erro.
- Os usuários de teste aparecem com logins determinísticos e sem duplicatas.
- Nenhuma conta real preexistente foi modificada, removida ou convertida em fixture.

## Cenário 2: colisão determinística

1. Crie ou mantenha uma conta real com login `admin@teste`.
2. Inicie em `Development` ou `Testing`.
3. Observe a fixture resultante.

### Resultado esperado
- O login da fixture passa para o próximo valor livre (`admin@teste`, `admin@teste-2`, etc.) seguindo a sequência definida.
- A conta real continua com o login original e seu estado é preservado.

## Cenário 3: idempotência

1. Execute a API em `Development` ou `Testing` cinco vezes consecutivas.
2. Consulte os registros de `Usuarios` após cada inicialização.

### Resultado esperado
- O número de fixtures permanece constante em três.
- Os logins permanecem estáveis para cada fixture.
- Nenhum usuário é duplicado.

## Cenário 4: produção

1. Configure o ambiente para `Production`.
2. Inicie a API.

### Resultado esperado
- A semeadora não cria nenhuma fixture.
- Nenhuma conta de teste aparece no banco.

## Validação automatizada

```bash
dotnet test Autenticador.UnitTests/Autenticador.UnitTests.csproj
dotnet test Autenticador.IntegrationTests/Autenticador.IntegrationTests.csproj
```

Ao concluir, a suíte deve validar:
- regras de negócio do fluxo de autenticação e de semeadora;
- integração com o endpoint HTTP de login e status;
- contrato HTTP de sucesso e erro conforme a constituição.
