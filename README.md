# StockControl — PIM IV (ADS UNIP)

Sistema de controle de estoque para mercadinhos: sistema web (ASP.NET Core MVC) + API REST para o app mobile + banco SQL Server. Vários estabelecimentos usam o mesmo sistema, e cada um só enxerga os próprios dados.

Baseado no projeto do PIM III, reescrito para atender aos requisitos do PIM IV.

## Funcionalidades

- **Cadastro do estabelecimento**: o dono cadastra o mercadinho, vira Admin e cria o acesso dos funcionários
- **Isolamento dos dados** entre estabelecimentos (banco + filtro automático do Entity Framework)
- **Login** com perfis **Admin**, **Gerente** e **Operador** (cookie no site, JWT na API)
- **Dashboard** com totais do dia, produtos que precisam de reposição e últimas movimentações
- **Vendas e entradas** de estoque, com bloqueio de venda sem estoque
- **Tela de Vendas** com total vendido em R$, itens e valor médio por período
- **Cadastros** de produtos (com busca), categorias, fornecedores e usuários
- **Relatório de movimentações** por período
- **Auditoria automática** de mudanças de preço e exclusões (trigger)
- **API REST** documentada com Swagger para o aplicativo mobile
- Layout **responsivo** e **acessível** (WCAG)

## Tecnologias

.NET 8 · ASP.NET Core MVC + Web API · Entity Framework Core 8 (Fluent API, sem migrations) · SQL Server · BCrypt · JWT · Swagger · HTML/CSS/JS puro

## Como rodar do zero

### 1. Instalar (uma vez só)

| Programa | Download |
|---|---|
| .NET 8 SDK | https://dotnet.microsoft.com/pt-br/download/dotnet/8.0 |
| SQL Server Express | https://www.microsoft.com/pt-br/sql-server/sql-server-downloads (opção **Express** → instalação **Básica**) |
| SQL Server Management Studio (SSMS) | https://learn.microsoft.com/pt-br/ssms/install/install |

### 2. Criar o banco

1. Abra o SSMS e conecte:
   - **Nome do Servidor:** `localhost\SQLEXPRESS`
   - **Autenticação:** Autenticação do Windows
   - Marque **"Certificado do Servidor de Confiança"**
2. **Arquivo → Abrir → Arquivo...** → `database/script_stockcontrol.sql`
3. Aperte **F5**. No painel da esquerda, **Bancos de Dados → Atualizar**: deve aparecer o **StockControl**.

> Rodar o script de novo recria o banco do zero (apaga tudo e volta aos dados de teste).

### 3. Confiar no certificado HTTPS de desenvolvimento (uma vez só)

No terminal, rode o comando abaixo e clique em **Sim** na janela do Windows:

```bash
dotnet dev-certs https --trust
```

Sem isso, o navegador mostra o aviso "Sua conexão não é particular" ao abrir o sistema.

### 4. Rodar o sistema

```bash
cd StockControl
dotnet run
```

Abra **https://localhost:7080** e entre com um dos usuários de teste, ou clique em **Cadastrar meu estabelecimento** para criar um mercadinho novo:

| E-mail | Senha | Perfil | Estabelecimento |
|---|---|---|---|
| admin@stockcontrol.local | Admin@123 | Admin | Mercadinho Bom Preço |
| gerente@stockcontrol.local | Gerente@123 | Gerente | Mercadinho Bom Preço |
| operador@stockcontrol.local | Operador@123 | Operador | Mercadinho Bom Preço |
| maria@donamaria.local | Maria@123 | Admin | Mercearia Dona Maria |

- Documentação da API: **https://localhost:7080/swagger**
- Para o **celular** acessar a API pela rede: `dotnet run --urls "http://0.0.0.0:5080"` e use `http://<IP do PC>:5080/api` (detalhes em [`docs/relatorio/06-api-rest.md`](docs/relatorio/06-api-rest.md#69-como-o-app-mobile-se-conecta-guia-para-o-colega-do-grupo))
- Se o SQL Server tiver outro nome, ajuste `ConnectionStrings:StockControl` em `StockControl/appsettings.json`
- Verificação de saúde (monitoramento): **`GET /health`**. Responde `200` com `{"status":"ok","banco":"conectado"}`, ou `503` se o banco estiver fora.

## Publicação em nuvem / container (informações para Cloud e DevOps)

- **Um único serviço:** o sistema web (MVC) e a API REST estão no **mesmo projeto** (`StockControl/StockControl.csproj`). Basta **um** container ou App Service para os dois.
- **Banco:** precisa ser **SQL Server** (ou Azure SQL), porque o projeto usa procedures e triggers em T-SQL. Crie o banco executando `database/script_stockcontrol.sql` (por exemplo: `sqlcmd -S servidor -U usuario -P senha -i database/script_stockcontrol.sql`).
- **Build e publicação:** `dotnet publish StockControl/StockControl.csproj -c Release -o publicar` (requer o SDK do .NET 8).
- **Configuração por variáveis de ambiente** (sobrescrevem o `appsettings.json`, sem mudar o código):

| Variável | Exemplo | Para quê |
|---|---|---|
| `ConnectionStrings__StockControl` | `Server=sql;Database=StockControl;User Id=sa;Password=SuaSenhaForte!;TrustServerCertificate=True;` | Conexão com o banco. No container use **usuário e senha** do SQL Server: a "Autenticação do Windows" (`Trusted_Connection`) não funciona fora do Windows. |
| `Jwt__Chave` | uma frase longa e secreta (32+ caracteres) | Assinatura dos tokens da API. **Troque a chave de desenvolvimento em produção.** |
| `ASPNETCORE_ENVIRONMENT` | `Production` | Desliga o Swagger e mostra páginas de erro genéricas. Use `Development` só para testes. |
| `ASPNETCORE_URLS` | `http://+:8080` | Porta em que o sistema escuta dentro do container. |
| `Cors__OrigensPermitidas__0` | `https://app.exemplo.com` | Endereço do app mobile, se ele rodar no navegador. |

- **Health check do container:** `GET /health` (sem login).
- **Logs:** o ASP.NET Core escreve os logs no console (saída padrão), que é o que o Docker e as plataformas de nuvem coletam.

## Estrutura

```
database/script_stockcontrol.sql   → cria o banco inteiro (modelo físico)
docs/mer.md                        → MER/DER em Mermaid
docs/modelo-logico.md              → tabelas, colunas, tipos, chaves, índices
docs/relatorio/                    → textos para o relatório, conferência de requisitos e lista de prints
StockControl/                      → projeto ASP.NET Core (.NET 8)
  Controllers/                     → MVC (telas)
  Controllers/Api/                 → API REST para o app mobile
  Models/                          → entidades (EntidadeBase, Produto, ...)
  ViewModels/                      → dados de cada tela
  Dtos/                            → dados que entram e saem da API
  Views/                           → telas Razor
  Services/                        → regras de negócio (interface + implementação)
  DAO/                             → acesso a dados (interface + implementação)
  Data/AppDbContext.cs             → mapeamento do banco
  Data/Configurations/             → Fluent API, uma classe por tabela
  wwwroot/                         → CSS e JavaScript
```

Fluxo de uma requisição: **Controller → Service → DAO → AppDbContext → SQL Server**

## Documentação para o relatório

Tudo em [`docs/relatorio/`](docs/relatorio/README.md): arquitetura, banco de dados, back-end, front-end, segurança, API REST, testes realizados, conferência de requisitos e lista de prints.

## Andamento

| Etapa | Situação |
|---|---|
| 1. Diagnóstico do projeto base | ✅ |
| 2. Script SQL + MER + modelo lógico | ✅ |
| 3. AppDbContext + Fluent API + DAOs + Services | ✅ |
| 4. Login (cookie) + CRUDs de Produtos, Categorias e Fornecedores | ✅ |
| 5. API para o mobile + JWT + Swagger | ✅ |
| 6. Movimentações, Dashboard, Usuários | ✅ |
| 7. Revisão final e lista de prints | ✅ |
