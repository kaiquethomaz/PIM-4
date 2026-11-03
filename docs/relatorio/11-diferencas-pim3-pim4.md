# 11. Diferenças entre o PIM III e o PIM IV

O StockControl do PIM IV foi construído **com base no projeto do PIM III** (sistema de controle de estoque do mesmo grupo). A ideia do sistema foi mantida: login com perfis, cadastro de produtos, categorias e fornecedores, entradas e saídas de estoque e relatórios. O que mudou foi **a forma de construir**, porque o PIM IV pede outra arquitetura, outro banco de dados e outra forma de criar o banco.

O projeto do PIM III não foi alterado. O PIM IV é um projeto novo, que reaproveita as regras de negócio e a identidade visual do anterior.

## 11.1 Quadro comparativo

| Assunto | PIM III | PIM IV |
|---|---|---|
| **Front-end** | Páginas HTML soltas com JavaScript, servidas por um pequeno servidor em Node.js (`server.js`) | Telas **Razor (ASP.NET Core MVC)** dentro do próprio projeto .NET |
| **Back-end** | Somente uma API, com a maior parte das rotas e regras em um único `Program.cs` (cerca de mil linhas) | **MVC + Web API no mesmo projeto**, organizado em Controllers, Services e DAO |
| **Camadas** | As rotas acessavam o banco diretamente | **Controller → Service → DAO → AppDbContext → SQL Server** |
| **Padrão DAO** | Não utilizado | Interface + implementação para cada entidade, com uma base genérica `DAOBase<T>` |
| **POO** | Entidades simples | `EntidadeBase` (Id e AtualizadoEm), encapsulamento (o estoque só muda por movimentação) e interfaces |
| **Banco de dados** | SQLite (ou MySQL) | **SQL Server** |
| **Criação do banco** | Migrations do Entity Framework | **Um script SQL único** (`database/script_stockcontrol.sql`), sem migrations |
| **Mapeamento** | Convenções do Entity Framework | **Fluent API**, com uma classe de configuração por tabela (`IEntityTypeConfiguration<T>`) |
| **Regras no banco** | Nenhuma; tudo ficava no código C# | **3 procedures** (registrar movimentação, produtos abaixo do mínimo, relatório por período) e **2 triggers** (atualizar o estoque e gravar a auditoria) |
| **Auditoria** | Não existia | Tabela `LogAuditoria` alimentada por trigger (mudança de preço e exclusão de produto), com tela para o Admin |
| **Autenticação** | JWT guardado no navegador | **Cookie** no sistema web e **JWT** somente na API do aplicativo mobile |
| **Segurança** | JWT e senhas com BCrypt | Também proteção **CSRF** nos formulários, **HTTPS**, **CORS** para o mobile e validação com mensagens em português |
| **Vários estabelecimentos** | Tabela de empresas, com o filtro feito manualmente em cada consulta | **Filtro automático** do Entity Framework (Global Query Filter) e o estabelecimento informado nas procedures; 17 testes sem vazamento de dados entre mercadinhos |
| **Visual** | Tema azul | Mesma paleta azul, com modo escuro, confirmação de exclusão em janela e acessibilidade (contraste WCAG, navegação pelo teclado, leitor de tela) |
| **Documentação** | READMEs | MER, modelo lógico e esta pasta de textos para o relatório |

## 11.2 O que foi deixado de fora de propósito

| Item do PIM III | Motivo |
|---|---|
| Previsão de demanda com ML.NET | Não faz parte dos requisitos do PIM IV e deixaria o projeto mais complexo |
| Testes automatizados (xUnit) | O enunciado do PIM IV pede para evitá-los; os testes foram feitos manualmente e registrados em [07-testes-realizados.md](07-testes-realizados.md) |
| Suporte a SQLite e MySQL | O PIM IV exige SQL Server, com procedures e triggers em T-SQL |

## 11.3 O que foi reaproveitado

- As **regras de negócio**: não vender sem estoque, perfis Admin, Gerente e Operador, e estoque mínimo para reposição.
- A **ideia de vários estabelecimentos** no mesmo sistema, agora com isolamento automático.
- A **identidade visual** do grupo (paleta em tons de azul).
- O uso de **BCrypt** para as senhas e de **JWT** para a API.

## 11.4 Texto sugerido para o relatório

> O sistema do PIM IV reaproveitou a ideia e a identidade visual do projeto desenvolvido no PIM III, mas foi reescrito para atender aos novos requisitos. A aplicação passou de uma API única, com o front-end em páginas HTML separadas, para um projeto ASP.NET Core MVC com Web API, organizado em camadas (Controller, Service e DAO). O banco de dados, antes criado por migrations em SQLite, passou a ser criado por um script único em SQL Server, que inclui procedures, triggers e auditoria. Também foram acrescentados a autenticação por cookie no sistema web, a proteção contra CSRF, o isolamento automático dos dados entre estabelecimentos e recursos de acessibilidade.
