<!--
Sync Impact Report
Version change: unversioned scaffold → 1.0.0
Modified principles:
- Princípio 1 sem título → I. API em camadas e controllers finas
- Princípio 2 sem título → II. Persistência encapsulada
- Princípio 3 sem título → III. Testes obrigatórios
- Princípio 4 sem título → IV. Contratos e idioma consistentes
- Princípio 5 sem título → V. Mudanças planejadas e arquivos coesos
Added sections: Additional Constraints; Development Workflow
Removed sections: none
Follow-up TODOs: Confirm the original ratification date.
-->
# Autenticador API Constitution

## Core Principles

### I. API em camadas e controllers finas
A API DEVE ser construída em C# sobre .NET 10 e usar controllers. A solução DEVE separar
responsabilidades em camadas; controllers DEVEM cuidar apenas do protocolo HTTP e delegar
o processamento às camadas apropriadas. Regras de negócio NÃO DEVEM residir em controllers.
Essa separação mantém as regras testáveis sem dependência do transporte HTTP.

### II. Persistência encapsulada
A persistência DEVE usar Dapper e permitir trocar o provider de banco de dados sem alterar
as regras de negócio. SQL DEVE ficar concentrado na camada de persistência, e não espalhado
por controllers ou serviços de negócio. Essa organização reduz acoplamento e torna explícitos
os limites de acesso a dados.

### III. Testes obrigatórios
Cada regra de negócio DEVE ter testes unitários executados com xUnit. Cada endpoint DEVE ter
testes de integração que exercitem seu caminho HTTP de sucesso e pelo menos um caminho de
erro. Os testes de integração DEVEM verificar o contrato HTTP, inclusive o status e o corpo
de erro quando aplicável. Nenhuma regra de negócio está coberta sem teste unitário.

### IV. Contratos e idioma consistentes
Nomes de tipos e arquivos, bem como mensagens de erro, DEVEM ser escritos em português.
Erros de validação DEVEM retornar HTTP 400 no formato ProblemDetails. Essa uniformidade
torna o contrato da API previsível para consumidores e para quem mantém o código.

### V. Mudanças planejadas e arquivos coesos
Cada arquivo de código-fonte DEVE declarar no máximo um tipo de nível superior, como classe,
interface, record ou enum. Toda dependência nova DEVE ter justificativa explícita no plano
antes de ser introduzida. Se um plano contradisser esta constituição, o plano DEVE ser
atualizado antes da implementação; a constituição prevalece. Essas regras limitam acoplamento
e mantêm decisões técnicas revisáveis.

## Additional Constraints

- O acesso a dados DEVE ser implementado com Dapper por meio de abstrações que permitam
  substituir o provider sem propagar detalhes do provider para as regras de negócio.
- Comandos e consultas SQL DEVEM permanecer na camada de persistência; é vedado espalhá-los
  por controllers ou pela camada de negócio.
- Tipos de nível superior DEVEM respeitar o limite de um por arquivo.

## Development Workflow

- A implementação de uma regra de negócio DEVE incluir seu teste unitário xUnit.
- A criação ou alteração de um endpoint DEVE incluir pelo menos um teste de integração do
  caminho de sucesso e um teste de integração de erro, usando o caminho HTTP do endpoint.
- Uma dependência nova só pode ser adicionada após sua justificativa constar no plano.
- Mensagens de commit DEVEM seguir `tipo[escopo]: descrição [identificador-do-requisito]`,
  incluindo o identificador do requisito relacionado.

## Governance

Esta constituição prevalece sobre planos, tarefas e convenções locais que a contradigam.
Alterações nela DEVEM ser explícitas, revisadas e acompanhadas da atualização deste relatório
de impacto e da versão. Revisões de planos e mudanças de código DEVEM verificar a conformidade
com todos os princípios e requisitos aplicáveis; desvios DEVEM ser corrigidos no artefato
conflitante ou aprovados por meio de uma emenda à constituição.

A versão segue SemVer: MAJOR para remoção ou redefinição incompatível de princípios ou regras;
MINOR para novos princípios, seções ou expansão material das obrigações; PATCH para
esclarecimentos e ajustes sem alteração de significado. Cada emenda DEVE atualizar a data de
última alteração e registrar a justificativa do incremento.

**Version**: 1.0.0 | **Ratified**: TODO(RATIFICATION_DATE): data original de adoção desconhecida | **Last Amended**: 2026-11-11
