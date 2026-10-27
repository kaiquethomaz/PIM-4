# Documentação para o relatório — StockControl

Textos prontos (ou quase) para copiar para o relatório do PIM IV. Cada arquivo cobre uma parte do sistema e é atualizado ao final de cada etapa do desenvolvimento.

| Arquivo | Conteúdo | Onde usar no relatório |
|---|---|---|
| [01-visao-geral-e-arquitetura.md](01-visao-geral-e-arquitetura.md) | O que é o sistema, tecnologias, arquitetura em camadas, integração entre módulos | Etapa 4 — Arquitetura da aplicação / Integração entre módulos |
| [02-banco-de-dados.md](02-banco-de-dados.md) | MER, modelo lógico e físico, procedures, triggers, índices, normalização | Etapa 7 — Banco de Dados |
| [03-back-end.md](03-back-end.md) | Camadas (Controller, Service, DAO), POO, regras de negócio, EF Core | Etapa 4 — Arquitetura / Estrutura administrativa |
| [04-front-end.md](04-front-end.md) | Telas, layout, responsividade, acessibilidade | Etapa 4 — Sistema Web / Etapa de acessibilidade e inclusão |
| [05-seguranca.md](05-seguranca.md) | Autenticação, autorização por perfil, senhas, CSRF, validação | Etapa 4 — Autenticação e Segurança |
| [06-api-rest.md](06-api-rest.md) | Endpoints REST para o app mobile, JWT, Swagger | Etapa 4 — APIs REST |
| [07-testes-realizados.md](07-testes-realizados.md) | Testes manuais executados e resultados | Testes / Validação |
| [08-conferencia-de-requisitos.md](08-conferencia-de-requisitos.md) | Cada requisito do PIM IV × o que foi entregue | Conclusão / Checklist de entrega |
| [09-lista-de-prints.md](09-lista-de-prints.md) | Checklist dos 57 prints, na ordem certa, com legendas sugeridas | Montagem do relatório |

Arquivos de apoio:
- [`../mer.md`](../mer.md) — diagrama MER/DER (Mermaid)
- [`../modelo-logico.md`](../modelo-logico.md) — tabelas, tipos, chaves e índices
- [`../../database/script_stockcontrol.sql`](../../database/script_stockcontrol.sql) — modelo físico (script completo)

> **Diagramas:** os blocos ```` ```mermaid ```` viram imagem colando o código em <https://mermaid.live> → *Actions* → *PNG/SVG*.
