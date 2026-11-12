# Contrato da API de Autenticação

## Endpoint: POST /api/auth/login

### Requisição

```json
{
  "login": "usuario@exemplo",
  "senha": "senhaSegura123"
}
```

### Regras

- `login` e `senha` são obrigatórios.
- `login` deve ser tratado sem distinção entre maiúsculas e minúsculas.
- `senha` nunca pode ser retornada em resposta.
- Se a conta estiver inativa, bloqueada temporariamente ou com credenciais inválidas, a autenticação deve falhar sem emitir um token.

### Resposta de sucesso

**Status**: `200 OK`

```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiraEm": "2026-11-12T12:00:00Z"
}
```

### Resposta de erro

**Status**: `400 Bad Request` para dados inválidos, `401 Unauthorized` para credenciais inválidas ou conta sem acesso

```json
{
  "type": "/errors/invalid-credentials",
  "title": "Credenciais inválidas",
  "status": 401,
  "detail": "Nenhuma autenticação foi emitida para as credenciais informadas."
}
```

## Endpoint: GET /api/auth/usuarios/{login}/status

### Autorização

- Requer header de credencial de serviço válido, por exemplo `X-Service-Credential: <token-assinado>`.
- Usuários comuns não devem ter acesso.

### Resposta de sucesso

**Status**: `200 OK`

```json
{
  "existe": true,
  "ativa": true
}
```

### Casos especiais

- Login inexistente:

```json
{
  "existe": false,
  "ativa": false
}
```

- Conta inativa:

```json
{
  "existe": true,
  "ativa": false
}
```

### Resposta de erro

**Status**: `401 Unauthorized` ou `403 Forbidden` para acesso sem autorização válida.

## Observações do contrato

- Os contratos devem ser descritos em português, conforme a constituição.
- Os objetos de erro devem seguir o formato ProblemDetails.
- Nenhuma resposta deve incluir `senha`, `hash` ou `perfis` sensíveis.

## Documento OpenAPI (ambiente de desenvolvimento)

- O registro dos serviços Swagger deve permitir a inicialização do host sem exceção de carregamento de tipo.
- Em `Development`, `GET /swagger/v1/swagger.json` deve retornar HTTP 200 e um documento OpenAPI válido que descreva os endpoints HTTP da API.
- A interface Swagger UI fica disponível em `/swagger` somente no ambiente de desenvolvimento, conforme a configuração atual da aplicação.
- Esta superfície não altera a autorização nem os contratos dos endpoints de autenticação; a documentação continua restrita ao ambiente previsto.
