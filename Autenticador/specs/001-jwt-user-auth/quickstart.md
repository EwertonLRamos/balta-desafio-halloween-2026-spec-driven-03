# Quickstart: Validação da funcionalidade de autenticação

## Pré-requisitos

- .NET 10 SDK instalado
- Banco de dados configurado via string de conexão para o ambiente de teste
- Variáveis de ambiente configuradas para `ASPNETCORE_ENVIRONMENT` e a chave JWT
- Credencial de serviço configurada para consultar existência/atividade

## Configuração inicial

1. Restaurar dependências:
   - `dotnet restore`
2. Compilar a solução:
   - `dotnet build`
3. Executar testes:
   - `dotnet test --logger "console;verbosity=minimal"`

## Cenários de validação

### 1. Inicialização com contas de teste

- Ambiente: `Development` ou `Testing`
- Ação: iniciar a API sem contas de teste previamente criadas
- Esperado:
  - três contas de teste ficam disponíveis;
  - pelo menos duas estão ativas;
  - uma permanece inativa;
  - nenhuma duplicação ocorre em reinicialização.

### 2. Login com credenciais válidas

- Ação: enviar `POST /api/auth/login` com um usuário ativo e senha correta
- Esperado:
  - resposta HTTP 200;
  - token JWT presente;
  - token contém apenas identidade e expiração, com validade de 60 minutos;
  - a senha não aparece no corpo nem no token.

### 3. Login com usuário inexistente ou senha incorreta

- Ação: enviar credenciais inválidas
- Esperado:
  - resposta HTTP 401 ou 400 de forma equivalente;
  - nenhuma distinção entre usuário inexistente e senha incorreta;
  - ausência de token.

### 4. Conta inativa

- Ação: tentar autenticar com conta inativa
- Esperado:
  - resposta HTTP 401/403 equivalente;
  - token não emitido;
  - resposta não revela que a conta existe e está inativa, a menos que o endpoint dedicado de consulta seja usado por um consumidor autorizado.

### 5. Limite de tentativas malsucedidas

- Ação: produzir três falhas consecutivas para a mesma conta ativa.
- Esperado:
  - o terceiro erro inicia um bloqueio de 10 minutos;
  - a conta é liberada automaticamente ao fim do prazo;
  - a conta não é permanentemente desativada.
- Repetir três falhas após a liberação: o próximo bloqueio dura 20 minutos; repetições seguintes dobram a duração até 24 horas.
- Confirmar que falhas recebidas durante bloqueio ativo não incrementam a contagem nem alteram o prazo; um login válido após a liberação reinicia o contador e a duração inicial para 10 minutos.
- Os testes automatizados devem controlar o relógio para verificar expiração, duração escalonada, limite de 24 horas e reset sem esperar o tempo real.
- Enviar falhas simultâneas para a mesma conta e verificar que a atualização atômica não perde incrementos nem permite ultrapassar o limite definido.

### 6. Consulta de existência e atividade

- Ação: consumir `GET /api/auth/usuarios/{login}/status` com credencial de serviço válida
- Esperado:
  - login ativo: `{ "existe": true, "ativa": true }`
  - login inativo: `{ "existe": true, "ativa": false }`
  - login inexistente: `{ "existe": false, "ativa": false }`
  - sem autorização: HTTP 401/403.

### 7. Inicialização da API e geração OpenAPI

- Pré-condições: restaurar os pacotes, configurar valores locais válidos para JWT e credencial de serviço, e selecionar o ambiente `Development`.
- Verificação do grafo: executar `dotnet list Autenticador.Api/Autenticador.Api.csproj package --include-transitive` e confirmar que os componentes Swashbuckle resolvidos pertencem à mesma família compatível com .NET 10.
- Ação: iniciar `Autenticador.Api` com `dotnet run --project Autenticador.Api/Autenticador.Api.csproj` e solicitar `GET /swagger/v1/swagger.json`.
- Esperado:
  - a aplicação inicia sem `TypeLoadException` durante `AddSwaggerGen`;
  - o endpoint retorna HTTP 200 com JSON OpenAPI válido;
  - a documentação inclui os endpoints da API e a interface está disponível em `/swagger` somente em `Development`.
- Diagnóstico: se a exceção persistir, comparar o grafo NuGet restaurado com as assemblies informadas no arquivo `.deps.json` e efetivamente carregadas, em vez de tratar a limpeza de saídas como correção suficiente.

## Critérios de sucesso

- Todos os testes de integração passam para fluxos de sucesso e erro.
- Nenhuma senha é persistida em texto puro.
- Não há criação de contas de teste em ambiente de produção.
- Os contratos de resposta e erro permanecem consistentes em português e com ProblemDetails para validações.
- A inicialização no ambiente de desenvolvimento não apresenta incompatibilidade binária do Swagger e o documento OpenAPI retorna HTTP 200.
- O JWT tem validade de 60 minutos e o bloqueio progressivo segue os limites e resets descritos em [data-model.md](./data-model.md) e [auth-api.md](./contracts/auth-api.md).
