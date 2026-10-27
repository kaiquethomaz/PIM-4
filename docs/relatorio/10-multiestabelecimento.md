# 10. Vários estabelecimentos no mesmo sistema (multi-estabelecimento)

## 10.1 O que é

O StockControl foi construído para atender **vários mercadinhos ao mesmo tempo**, usando o mesmo sistema e o mesmo banco de dados. Esse modelo é chamado de **multi-tenant** (multi-inquilino): cada estabelecimento é um "inquilino" que enxerga e altera **somente os próprios dados**, como se o sistema fosse só dele.

| Quem | O que faz |
|---|---|
| **Dono do mercadinho** | Cadastra o estabelecimento na tela "Cadastrar meu estabelecimento" e vira o **Admin** dele. |
| **Admin (dono)** | Cria o acesso dos funcionários (Gerente ou Operador), edita os dados do estabelecimento e usa todo o sistema. |
| **Funcionários** | Entram com o login criado pelo dono e trabalham só com os dados daquele estabelecimento. |

**Por que fazer assim?** Um único sistema atende quantos mercadinhos quiserem, sem instalar nada novo para cada cliente. É o mesmo modelo dos sistemas vendidos por assinatura (SaaS).

## 10.2 Fluxo de cadastro

```mermaid
sequenceDiagram
    actor D as Dono do mercadinho
    participant C as ContaController
    participant S as EstabelecimentoService
    participant DAO as EstabelecimentoDAO
    participant B as SQL Server

    D->>C: POST /Conta/Cadastrar (dados do mercadinho + nome, e-mail e senha)
    C->>C: Valida o formulário (DataAnnotations) e o token anti-CSRF
    C->>S: CadastrarAsync(estabelecimento, dono, senha)
    S->>S: CNPJ válido? CNPJ e e-mail ainda não usados? Senha ≥ 6?
    S->>S: Perfil = Admin; senha → hash BCrypt
    S->>DAO: CadastrarComAdministradorAsync(...)
    DAO->>B: BEGIN TRANSACTION
    DAO->>B: INSERT Estabelecimento
    DAO->>B: INSERT Usuario (dono) + 6 categorias iniciais
    DAO->>B: COMMIT
    C->>C: Cria o cookie de login (com o estabelecimento)
    C-->>D: Dashboard do novo mercadinho + mensagem de boas-vindas
```

- Tudo é gravado em **uma transação**: se qualquer passo falhar, nada fica gravado (não existe estabelecimento "sem dono").
- O mercadinho novo já começa com **6 categorias** (Mercearia, Bebidas, Laticínios e Frios, Limpeza, Higiene Pessoal, Hortifrúti), para não começar com o sistema vazio.
- O dono entra no sistema automaticamente depois do cadastro.

## 10.3 Como o isolamento dos dados funciona (3 camadas)

### Camada 1 — Banco de dados
- `Usuario`, `Categoria`, `Fornecedor` e `Produto` têm a coluna `EstabelecimentoId` (FK para `Estabelecimento`).
- `Movimentacao` **não** tem: o estabelecimento é o do produto movimentado (3ª Forma Normal).
- As **procedures recebem `@EstabelecimentoId`** e só trabalham com os dados dele. A `sp_RegistrarMovimentacao` recusa um produto ou usuário de outro estabelecimento (erro 50001/50002).
- Os índices únicos são **compostos**: `(EstabelecimentoId, CodigoBarras)`, `(EstabelecimentoId, CNPJ)` do fornecedor e `(EstabelecimentoId, Nome)` da categoria. Assim, dois mercadinhos podem vender o mesmo arroz, com o mesmo código de barras, e comprar do mesmo distribuidor, mas nenhum deles cadastra o mesmo produto duas vezes.
- O **e-mail** (login) e o **CNPJ do estabelecimento** continuam únicos no sistema todo.

### Camada 2 — Entity Framework: filtro automático (*Global Query Filter*)
O estabelecimento do usuário logado vai gravado no cookie (site) e no token JWT (API). O `AppDbContext` aplica um filtro automático em **todas** as consultas:

```csharp
modelBuilder.Entity<Produto>().HasQueryFilter(p => p.EstabelecimentoId == EstabelecimentoAtualId);
modelBuilder.Entity<Movimentacao>().HasQueryFilter(m => m.Produto!.EstabelecimentoId == EstabelecimentoAtualId);
// ... o mesmo para Estabelecimento, Usuario, Categoria, Fornecedor e LogAuditoria
```

Com isso, um simples `_contexto.Produtos.ToListAsync()` vira, no SQL:

```sql
SELECT ... FROM Produto WHERE EstabelecimentoId = @estabelecimentoDoUsuarioLogado
```

**Vantagem:** ninguém precisa lembrar de filtrar em cada tela, e é impossível "esquecer" o filtro em algum lugar. Quando ninguém está logado, as consultas não trazem nada.

Só dois pontos ignoram o filtro, de propósito, com `IgnoreQueryFilters()`:
- **o login**, que precisa achar o usuário pelo e-mail antes de saber de qual estabelecimento ele é;
- **a verificação de e-mail e CNPJ únicos** no sistema todo.

**Gravação automática:** ao salvar um registro novo (produto, categoria, fornecedor ou usuário), o `AppDbContext` preenche o `EstabelecimentoId` com o estabelecimento do usuário logado, usando a interface `IPertenceAEstabelecimento`:

```csharp
public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
{
    foreach (var entrada in ChangeTracker.Entries<IPertenceAEstabelecimento>()
                 .Where(e => e.State == EntityState.Added && e.Entity.EstabelecimentoId == 0))
        entrada.Entity.EstabelecimentoId = EstabelecimentoAtualId ?? throw new InvalidOperationException(...);

    return base.SaveChangesAsync(cancellationToken);
}
```

### Camada 3 — Regras de negócio
Como os DAOs só enxergam o próprio estabelecimento, as regras dos Services passam a valer **por estabelecimento** sem nenhum código a mais:
- "Categoria inválida": a categoria de outro mercadinho não é encontrada.
- "Já existe um produto com este código de barras": a busca é só no próprio estabelecimento.
- "Precisa ter pelo menos um administrador ativo": conta os administradores do próprio estabelecimento.

## 10.4 Classes envolvidas

| Classe | Papel |
|---|---|
| `Models/Estabelecimento` | Entidade (herda de `EntidadeBase`). |
| `Models/IPertenceAEstabelecimento` | Interface das entidades que pertencem a um estabelecimento. |
| `Models/Cnpj` | Classe estática com limpeza, formatação e validação de CNPJ (usada por Estabelecimento e Fornecedor). |
| `Services/IUsuarioAtual` / `UsuarioAtual` | Lê o estabelecimento do usuário logado (cookie ou JWT). |
| `Data/AppDbContext` | Filtros automáticos e preenchimento automático do `EstabelecimentoId`. |
| `DAO/IEstabelecimentoDAO` / `EstabelecimentoDAO` | CNPJ único e cadastro em transação. |
| `Services/IEstabelecimentoService` / `EstabelecimentoService` | Regras do cadastro e da edição do estabelecimento. |
| `Controllers/ContaController` | Telas de login e de cadastro de estabelecimento. |
| `Controllers/EstabelecimentoController` | Tela "Meu estabelecimento" (somente Admin). |
| `Extensions/ClaimsStockControl` | Nome dos *claims* `EstabelecimentoId` e `Estabelecimento`. |

## 10.5 Dados de teste

| Estabelecimento | Usuários | Dados |
|---|---|---|
| **Mercadinho Bom Preço** | admin@stockcontrol.local (Admin), gerente@stockcontrol.local (Gerente), operador@stockcontrol.local (Operador) | 5 categorias, 3 fornecedores, 15 produtos, 38 movimentações; 5 abaixo do mínimo |
| **Mercearia Dona Maria** | maria@donamaria.local (Admin) | 2 categorias, 1 fornecedor, 5 produtos, 10 movimentações; 2 abaixo do mínimo |

O seed foi montado para **demonstrar o isolamento**:
- o **Arroz** tem o **mesmo código de barras** nos dois estabelecimentos;
- a categoria **"Mercearia"** existe nos dois;
- a **Distribuidora Alfa** (mesmo CNPJ) é fornecedora dos dois.

Tudo isso é permitido, porque a unicidade vale dentro de cada estabelecimento.
