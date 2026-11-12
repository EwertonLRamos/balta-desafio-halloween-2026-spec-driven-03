# Research: Autenticação de Usuários com JWT

## Decision

- A API será implementada em .NET 10 com ASP.NET Core, seguindo a arquitetura em camadas exigida pela constituição.
- A autenticação será feita por login e senha com verificação de usuário ativo e senha hashada por PBKDF2-HMAC-SHA256 (ou equivalente via abstração de hashing do ASP.NET Core) antes de persistir.
- A emissão de JWT usará a identidade do usuário como único payload obrigatório, com prazo de expiração explícito e sem incluir senha ou valor protegido.
- A consulta de existência/atividade será exposta a consumidores autorizados por credencial de serviço, retornando somente os indicadores `existe` e `ativa`.
- As falhas de autenticação serão tratadas com resposta uniforme, sem distinguir login inexistente de senha incorreta.
- O controle de tentativas malsucedidas será por janela deslizante e bloqueio temporário por conta, sem desativação permanente.
- Três contas de teste serão inicializadas somente quando `ASPNETCORE_ENVIRONMENT` for `Development` ou `Testing` e a base ainda não contiver esses usuários.

## Rationale

Esses padrões atendem às regras funcionais e às ressalvas de segurança descritas na especificação. A escolha de hashing unidirecional e a ausência de senha no token atendem aos requisitos de proteção de credenciais. A resposta uniforme evita vazamento de informação sobre a conta e reduz a chance de enumeração. O bloco temporário por falhas atende ao requisito de “pausar temporariamente novas tentativas” sem bloquear permanentemente a conta. O seed limitado em ambientes específicos protege a produção de contas de teste.

## Alternatives considered

- Armazenar senha em texto simples: rejeitado porque viola explicitamente a exigência de proteção e o princípio de persistência segura.
- Usar JWT com claims extras além da identidade: rejeitado porque a especificação exige que o token contenha apenas a identidade do usuário e validade.
- Bloquear permanentemente a conta após falhas: rejeitado porque a funcionalidade exige pausa temporária e sem desativação definitiva.
- Expôr o resultado de login como “usuário inexistente” versus “senha incorreta”: rejeitado por risco de enumeração e por requisito explícito de igualdade de respostas.
- Criar contas de teste em todos os ambientes: rejeitado porque a especificação exige exclusividade em desenvolvimento/teste e nenhuma criação em produção.

## Research tasks resolved

- Autenticação com JWT: usar claims `sub`/`user_id` e `exp` como padrão do ASP.NET Core.
- Armazenamento seguro de senha: usar `PBKDF2`/`Rfc2898DeriveBytes` ou `PasswordHasher` com salt e hash de credencial.
- Controle de tentativas: manter contador e tempo de bloqueio por login, validando a janela de falhas em cada tentativa.
- Seed de usuários: usar inicialização da aplicação e verificação por login único para evitar duplicidade.
