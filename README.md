# StockControl — PIM IV (ADS UNIP)

Sistema de controle de estoque para um mercadinho: sistema web (ASP.NET Core MVC) + API REST para o app mobile + banco SQL Server.

Baseado no projeto do PIM III (`D:\PIM-3`), reescrito para atender aos requisitos do PIM IV.

## Andamento

| Etapa | Situação |
|---|---|
| 1. Diagnóstico do projeto base | ✅ |
| 2. Script SQL + MER + modelo lógico | ✅ |
| 3. AppDbContext + Fluent API + DAOs + Services | ✅ |
| 4. Login (cookie) + CRUDs de Produtos, Categorias e Fornecedores | ✅ |
| 5. API para o mobile + JWT + Swagger | ✅ |
| 6. Movimentações, Dashboard, Usuários | ✅ |
| 7. Revisão final e lista de prints | ⏳ |

## Estrutura

```
database/script_stockcontrol.sql   → cria o banco inteiro (modelo físico)
docs/mer.md                         → MER/DER em Mermaid
docs/modelo-logico.md               → tabelas, colunas, tipos, chaves, índices
docs/relatorio/                     → textos prontos para o relatório (arquitetura, banco, back, front, segurança, API, testes)
StockControl/                       → projeto ASP.NET Core MVC (.NET 8)
  Controllers/                      → MVC (telas)
  Controllers/Api/                  → endpoints JSON para o app mobile (etapa 5)
  Models/                           → entidades (EntidadeBase, Produto, ...)
  Models/Consultas/                 → resultados das procedures e do dashboard
  Views/                            → telas Razor
  Services/                         → regras de negócio (interface + implementação)
  DAO/                              → acesso a dados (interface + implementação)
  Data/AppDbContext.cs              → mapeamento do banco
  Data/Configurations/              → Fluent API, uma classe por tabela
  Excecoes/                         → RegraNegocioException
```

Fluxo de uma requisição: **Controller → Service → DAO → AppDbContext → SQL Server**

## Como rodar o sistema

1. Crie o banco (seção abaixo).
2. Confira a connection string em `StockControl/appsettings.json` (padrão: `localhost\SQLEXPRESS`).
3. Abra a pasta `StockControl` no terminal e rode `dotnet run`, ou abra o `StockControl.csproj` no Visual Studio e aperte F5.

## Como criar o banco

1. Instale o SQL Server Express e o SSMS (SQL Server Management Studio).
2. No SSMS, conecte em `localhost\SQLEXPRESS` (ou `(localdb)\MSSQLLocalDB`).
3. Abra `database/script_stockcontrol.sql` e aperte **F5**.

Usuários de teste:

| E-mail | Senha | Perfil |
|---|---|---|
| admin@stockcontrol.local | Admin@123 | Admin |
| gerente@stockcontrol.local | Gerente@123 | Gerente |
| operador@stockcontrol.local | Operador@123 | Operador |
