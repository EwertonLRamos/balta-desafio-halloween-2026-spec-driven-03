# Contract: autenticação e status de usuário

## Endpoint 1: POST /api/auth/login

### Requisição

```json
{
  "login": "admin@teste",
  "senha": "Senha123!"
}
```

### Resposta de sucesso

**Status**: `200 OK`

```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiraEm": "2026-11-12T15:00:00Z"
}
```

### Resposta de erro

**Status**: `401 Unauthorized`

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.2",
  "title": "Unauthorized",
  "status": 401,
  "detail": "Credenciais inválidas.",
  "instance": "/api/auth/login"
}
```

### Regras do contrato
- O corpo deve conter `login` e `senha` preenchidos.
- A autenticação deve falhar com `401` e `ProblemDetails` quando as credenciais forem inválidas.
- O token retornado deve ser emitido com a configuração JWT do projeto.

## Endpoint 2: GET /api/auth/usuarios/{login}/status

### Requisição

**Header obrigatória**:

```http
X-Service-Credential: <secret>
```

### Resposta de sucesso

**Status**: `200 OK`

```json
{
  "existe": true,
  "ativa": true
}
```

### Resposta de erro

**Status**: `401 Unauthorized`

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.2",
  "title": "Unauthorized",
  "status": 401,
  "detail": "Credencial de serviço inválida.",
  "instance": "/api/auth/usuarios/admin@teste/status"
}
```

### Regras do contrato
- O header `X-Service-Credential` deve ser validado com comparação em tempo constante.
- O endpoint deve retornar `true`/`false` para `existe` e `ativa` sem expor detalhes sensíveis demais da entidade.
- Em caso de credencial inválida, a API deve responder com `401` e `ProblemDetails`.
