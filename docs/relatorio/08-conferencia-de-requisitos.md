# 8. Conferência de requisitos

Comparação, item por item, entre o que foi pedido para o PIM IV (Sistema Web e Banco de Dados) e o que foi entregue. Conferência feita em 27/10/2026, com o sistema rodando do zero (banco recriado pelo script + `dotnet run`).

Legenda: ✅ atendido · ➕ entregue além do pedido

## 8.1 Stack e arquitetura

| Requisito | Situação | Onde está |
|---|---|---|
| ASP.NET Core MVC com Views Razor (front e back no mesmo projeto) | ✅ | `StockControl/Controllers`, `StockControl/Views` |
| Web API no mesmo projeto, em `Controllers/Api` | ✅ | `StockControl/Controllers/Api` |
| Entity Framework Core com `AppDbContext` e `DbSet<T>` | ✅ | `Data/AppDbContext.cs` |
| Fluent API (`OnModelCreating`, `ToTable`, `HasKey`, `IsRequired`, `HasMaxLength`) | ✅ | `Data/Configurations/*` |
| Uma classe de configuração por entidade (`IEntityTypeConfiguration<T>`) | ✅ | 6 classes em `Data/Configurations` |
| Padrão DAO (interface + implementação) | ✅ | `DAO/` (`IProdutoDAO`/`ProdutoDAO` etc.) ➕ com base genérica `DAOBase<T>` |
| SQL Server | ✅ | SQL Server 2025 Express |
| Sem Blazor, Razor Pages, SignalR ou WPF | ✅ | — |
| Fluxo Controller → Service → DAO → AppDbContext → Banco | ✅ | [01-visao-geral-e-arquitetura.md](01-visao-geral-e-arquitetura.md) |
| Pastas Controllers, Controllers/Api, Models, ViewModels, Views, Services, DAO, Data, Data/Configurations | ✅ | ➕ também `Dtos`, `Excecoes`, `Extensions` |
| Services e DAOs registrados com injeção de dependência no `Program.cs` | ✅ | `Program.cs`, seção 6 |
| POO visível: `EntidadeBase` com `Id` e `AtualizadoEm`, encapsulamento, interfaces | ✅ | [03-back-end.md](03-back-end.md#33-programação-orientada-a-objetos) |

## 8.2 Banco de dados (Etapa 7 do manual)

| Requisito | Situação | Onde está |
|---|---|---|
| Script SQL único `database/script_stockcontrol.sql` | ✅ | `database/` |
| Sem migrations do EF Core (o EF só mapeia) | ✅ | Não há pasta `Migrations` nem pacote de migrations |
| Entidades Usuario, Categoria, Fornecedor, Produto, Movimentacao, LogAuditoria com os campos pedidos | ✅ | ➕ `AtualizadoEm` em todos os cadastros (para a `EntidadeBase` e a sincronização do app) |
| "Venda" como Movimentação do tipo SAIDA | ✅ | — |
| `sp_RegistrarMovimentacao` (valida estoque, transação) | ✅ | ➕ trava a linha do produto contra vendas simultâneas |
| `sp_ProdutosAbaixoDoMinimo` (usada no dashboard) | ✅ | Usada no Dashboard web e em `GET /api/dashboard/resumo` |
| `sp_RelatorioMovimentacoes` (por período) | ✅ | Usada na tela Movimentações e em `GET /api/movimentacoes` |
| `trg_AtualizaEstoque` (a procedure NÃO atualiza o estoque) | ✅ | — |
| `trg_AuditoriaProduto` (preço e exclusão em LogAuditoria) | ✅ | — |
| Índices únicos em CodigoBarras, Email, CNPJ | ✅ | — |
| Índices nas FKs e em `Movimentacao.Data` | ✅ | ➕ índice em `Produto.AtualizadoEm` |
| `HasTrigger(...)` nas tabelas com trigger | ✅ | Movimentacao **e** Produto |
| Procedures chamadas pelo EF com SQL interpolado | ✅ | `MovimentacaoDAO`, `ProdutoDAO` |
| Ordem do script: CREATE DATABASE → tabelas → índices → procedures → triggers → seed → testes comentados | ✅ | — |
| PK, FK, UNIQUE e CHECK (ex.: Quantidade > 0, Tipo IN ...) | ✅ | — |
| Seed: 1 admin, categorias, fornecedores, ~15 produtos, movimentações | ✅ | ➕ 3 usuários (um por perfil), 38 movimentações |
| MER em Mermaid (`docs/mer.md`) | ✅ | `docs/mer.md` |
| Modelo lógico (`docs/modelo-logico.md`) | ✅ | `docs/modelo-logico.md` |
| Modelo físico = o script | ✅ | — |

## 8.3 Sistema web (Etapa 4 do manual)

| Requisito | Situação | Onde está |
|---|---|---|
| Tela de Login | ✅ | `/Conta/Login` |
| Dashboard: totais, produtos abaixo do mínimo, últimas movimentações | ✅ | `/` ➕ botão "Repor" e atalhos de venda/entrada |
| Produtos: CRUD com busca | ✅ | `/Produtos` |
| Categorias: CRUD | ✅ | `/Categorias` |
| Fornecedores: CRUD | ✅ | `/Fornecedores` ➕ validação do dígito verificador do CNPJ |
| Movimentações: registrar entrada/saída + listagem por período | ✅ | `/Movimentacoes`, `/Movimentacoes/Registrar` |
| Usuários: CRUD somente Admin (estrutura administrativa) | ✅ | `/Usuarios` ➕ proteção do último administrador |
| Layout simples, responsivo e acessível | ✅ | [04-front-end.md](04-front-end.md) |

## 8.4 API para o mobile

| Requisito | Situação |
|---|---|
| `POST /api/auth/login` → JWT | ✅ |
| `GET /api/produtos` com `?atualizadoDesde=` | ✅ |
| `GET /api/produtos/codigo-barras/{codigo}` | ✅ |
| `POST /api/movimentacoes` (registro rápido) | ✅ ➕ aceita o Id ou o código de barras e devolve o estoque atualizado |
| `GET /api/dashboard/resumo` | ✅ |
| CRUDs de produtos, categorias e fornecedores | ✅ |
| Documentação com Swagger | ✅ ➕ comentários em português e botão Authorize |

## 8.5 Autenticação e segurança

| Requisito | Situação |
|---|---|
| Web: autenticação por cookie | ✅ |
| `[Authorize]` e `[Authorize(Roles = "Admin")]` | ✅ |
| API: JWT nos endpoints `/api` | ✅ (o cookie não dá acesso à API) |
| Senhas com hash BCrypt | ✅ |
| Validação de entrada (DataAnnotations / ModelState) | ✅ |
| Proteção CSRF nos formulários MVC | ✅ (testado: POST sem token → 400) |
| CORS configurado para o mobile | ✅ |
| HTTPS | ✅ (http → https; HSTS em produção) |
| Regras de negócio no Service | ✅ |

## 8.6 Forma de trabalho

| Pedido | Situação |
|---|---|
| Comentários no código em português, soluções didáticas | ✅ |
| Analisar o projeto existente antes de alterar | ✅ (diagnóstico da etapa 1; o PIM III foi preservado) |
| Avisar antes de mudanças de estrutura | ✅ |
| Ao final de cada etapa, dizer o que foi feito e como testar | ✅ |
| Documentação para o relatório (back, front e banco) | ✅ (esta pasta) |
| Lista de prints para o relatório | ✅ [09-lista-de-prints.md](09-lista-de-prints.md) |

## 8.7 Resultado da revisão final

- **Compilação:** 0 erros, 0 avisos.
- **Banco recriado do zero pelo script:** 3 usuários, 5 categorias, 3 fornecedores, 15 produtos e 38 movimentações; 5 produtos abaixo do mínimo.
- **Teste de todas as telas com os 3 perfis:** 51 de 51 combinações (17 páginas × 3 perfis) com o comportamento esperado (abre ou "Acesso negado").
- **Outros:** id inexistente → 404; sem login → tela de login; `http://` → `https://`; Swagger no ar.
- **Total de testes manuais registrados:** ver [07-testes-realizados.md](07-testes-realizados.md).
