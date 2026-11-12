# Research: Integridade das contas de teste

## Decision: Persistir uma identidade estável por fixture
Decision: Cada conta de teste receberá uma chave persistida e estável, separada do login atual, e a inicialização usará essa identidade como referência para reconhecer a mesma fixture em reinicializações e mudanças de login.

Rationale: A especificação exige que a conta real seja preservada e que o sistema reconheça a fixture mesmo quando o login preferencial for alterado ou colidir com outra conta. Login não é uma chave confiável porque é mutável e sujeito a colisão.

Alternatives considered: usar somente o login como identidade; comparar a conta pela senha hash; recriar registros por login; usar um identificador aleatório sem persistência. Todas foram rejeitadas porque não resolvem colisão, não preservam a conta real e quebram a repetibilidade da inicialização.

## Decision: Resolver colisões por sequência determinística de logins alternativos
Decision: Quando o login preferencial estiver ocupado, o sistema deve escolher o primeiro login livre na sequência `<login>.teste`, `<login>.teste-2`, `<login>.teste-3`, e assim por diante, em ordem crescente.

Rationale: A sequência é determinística, previsível e mantém o comportamento estável entre reinicializações, além de evitar que a fixture tenha um login escolhido de forma aleatória ou conflitante.

Alternatives considered: escolher aleatoriamente, incrementar em contador global ou usar um sufixo de data. Esses padrões foram rejeitados porque quebrariam a idempotência e dificultariam a reconciliação de fixtures já existentes.

## Decision: Semear somente em ambientes de desenvolvimento e teste
Decision: A preparação das fixtures será executada somente quando `ASPNETCORE_ENVIRONMENT` ou o nome do ambiente for `Development` ou `Testing`.

Rationale: A funcionalidade exige que a criação de contas de teste nunca ocorra em produção e que o ambiente siga a política de segregação de ambientes.

Alternatives considered: rodar a preparação sempre e validar em produção com uma flag; manter a lógica no repositório sem ambiente; usar `IsDevelopment()` e ignorar testes. Rejeitado porque quebraria a regra especificada e poderia acidentalmente expor dados de teste em produção.

## Decision: Nomear artefatos em português, conforme a constituição
Decision: Os testes e arquivos propostos serão nomeados `TestesRepositorioUsuario` e `TestesInicializacaoSwagger`, com no máximo um tipo de nível superior por arquivo.

Rationale: A constituição exige idioma consistente em português e um único tipo por arquivo, além de garantir legibilidade e alinhamento com o restante do código.

Alternatives considered: manter nomes em inglês ou misturar português e inglês; agrupar múltiplos tipos em um único arquivo. Ambos foram rejeitados porque violam o princípio constitucional e aumentam a ambiguidade dos artefatos.

## Decision: Manter a semeadora idempotente
Decision: A inicialização deve procurar as fixtures por identidade persistida e apenas criar registros ausentes, sem duplicar contas nem reescrever dados de contas reais.

Rationale: Isso atende ao requisito de não criação de duplicatas e à necessidade de reexecutar a preparação em qualquer inicialização sem efeitos colaterais.

Alternatives considered: inserir sempre novos usuários e remover os antigos; sobrescrever por login; limpar contas públicas antes de reinicializar. Rejeitados porque alteram estado real e quebram a integração da base operacional.
