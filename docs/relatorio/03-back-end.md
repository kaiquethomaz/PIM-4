# 3. Back-end

## 3.1 Visão geral

O back-end foi desenvolvido em **C# com ASP.NET Core 8**, em um único projeto que atende tanto o sistema web (padrão **MVC**) quanto o aplicativo mobile (**Web API**). O código é organizado em camadas, e cada camada só conversa com a camada imediatamente abaixo:

**Controller → Service → DAO → AppDbContext → SQL Server**

## 3.2 Camadas

### Controllers (apresentação)
Recebem a requisição HTTP, validam os dados de entrada (`ModelState`), chamam o Service e devolvem a resposta: uma tela HTML (MVC) ou JSON (API). Os Controllers **não** contêm regras de negócio nem acessam o banco.

| Controller | Função | Quem acessa |
|---|---|---|
| `ContaController` | Login, logout e acesso negado | Todos (login sem autenticação) |
| `HomeController` | Página inicial / dashboard | Usuários logados |
| `ProdutosController` | Listagem com busca e CRUD de produtos | Consulta: todos; cadastro/edição/exclusão: Admin e Gerente |
| `CategoriasController` | CRUD de categorias | Admin e Gerente |
| `FornecedoresController` | CRUD de fornecedores | Admin e Gerente |

Os controllers da API REST (`Controllers/Api/`) herdam de `ApiControllerBase`, usam autenticação JWT e devolvem JSON. Eles estão descritos em [06-api-rest.md](06-api-rest.md).

### DTOs (API)
Os *Data Transfer Objects* (`Dtos/`) são os objetos que a API recebe e devolve em JSON (ex.: `ProdutoDto`, `ProdutoRequisicao`, `LoginResposta`). Eles têm o mesmo papel dos ViewModels nas telas: validar a entrada e controlar o que sai, sem expor dados sensíveis como o hash da senha.

### ViewModels
Classes com os dados de cada tela (ex.: `ProdutoFormViewModel`, `LoginViewModel`). Elas contêm as **regras de validação de entrada** com *DataAnnotations* (`[Required]`, `[StringLength]`, `[Range]`, `[EmailAddress]`, `[RegularExpression]`) e controlam exatamente quais campos o usuário pode enviar. Isso evita, por exemplo, que alguém altere o estoque de um produto enviando um campo extra no formulário (*overposting*).

### Services (regras de negócio)
Cada Service tem uma **interface** (`IProdutoService`) e uma **implementação** (`ProdutoService`). Os Services concentram as regras do mercadinho:

| Service | Principais regras |
|---|---|
| `ProdutoService` | Código de barras não pode se repetir; categoria e fornecedor precisam existir; preço e estoque mínimo não podem ser negativos; o estoque inicial vira uma movimentação de ENTRADA; produto com histórico é **desativado** em vez de excluído. |
| `MovimentacaoService` | Quantidade maior que zero; produto ativo; **saída não pode ser maior que o estoque**; período do relatório válido. |
| `CategoriaService` | Nome obrigatório e único; não exclui categoria que tenha produtos. |
| `FornecedorService` | Remove a máscara do CNPJ e **valida os dígitos verificadores**; CNPJ único; não exclui fornecedor que tenha produtos. |
| `UsuarioService` | Senha com no mínimo 6 caracteres, guardada com hash BCrypt; e-mail único e sempre minúsculo; o sistema não pode ficar sem administrador ativo; o usuário não pode desativar a si mesmo. |
| `DashboardService` | Monta os totais do painel, os produtos abaixo do mínimo e as últimas movimentações. |

Quando uma regra é violada, o Service lança uma `RegraNegocioException` com uma mensagem amigável. O Controller captura essa exceção e mostra a mensagem na tela (ou devolve no JSON).

### DAO — Data Access Object (acesso a dados)
O padrão **DAO** isola todo o acesso ao banco em classes próprias. Os Services não sabem *como* os dados são gravados: só chamam métodos como `BuscarPorCodigoBarrasAsync`.

```mermaid
classDiagram
    class IDAOBase~T~ {
        <<interface>>
        +ListarTodosAsync()
        +BuscarPorIdAsync(id)
        +ContarAsync()
        +InserirAsync(entidade)
        +AtualizarAsync(entidade)
        +ExcluirAsync(entidade)
    }
    class DAOBase~T~ {
        <<abstract>>
        #_contexto : AppDbContext
        #_tabela : DbSet~T~
    }
    class IProdutoDAO {
        <<interface>>
        +BuscarAsync(termo)
        +BuscarPorCodigoBarrasAsync(codigo)
        +ListarAbaixoDoMinimoAsync()
    }
    class ProdutoDAO
    class IMovimentacaoDAO {
        <<interface>>
        +RegistrarAsync(movimentacao)
        +ListarPorPeriodoAsync(inicio, fim)
    }
    class MovimentacaoDAO

    IDAOBase~T~ <|.. DAOBase~T~
    IDAOBase~T~ <|-- IProdutoDAO
    DAOBase~T~ <|-- ProdutoDAO
    IProdutoDAO <|.. ProdutoDAO
    IMovimentacaoDAO <|.. MovimentacaoDAO
```

- `IDAOBase<T>` / `DAOBase<T>`: CRUD genérico, escrito uma única vez e herdado por `ProdutoDAO`, `CategoriaDAO`, `FornecedorDAO` e `UsuarioDAO`.
- Cada DAO específico acrescenta suas consultas (ex.: `ProdutoDAO.ListarAbaixoDoMinimoAsync()` chama a procedure `sp_ProdutosAbaixoDoMinimo`).
- `MovimentacaoDAO` **não** herda o CRUD genérico: uma movimentação é histórica e não pode ser editada nem excluída. Ela é registrada pela procedure `sp_RegistrarMovimentacao`.
- Os erros lançados pela procedure (`THROW 50005 'Estoque insuficiente'`) são convertidos em `RegraNegocioException`.

### Data (mapeamento)
`AppDbContext` e as classes em `Data/Configurations` fazem o mapeamento das classes C# para as tabelas do banco por **Fluent API**. Detalhes em [02-banco-de-dados.md](02-banco-de-dados.md#29-integração-com-a-aplicação-entity-framework-core).

## 3.3 Programação Orientada a Objetos

| Conceito | Onde aparece no código |
|---|---|
| **Abstração** | `EntidadeBase` é `abstract`: representa o que toda entidade de cadastro tem em comum, mas não pode ser instanciada sozinha. `DAOBase<T>` também é abstrata. |
| **Herança** | `Usuario`, `Categoria`, `Fornecedor` e `Produto` herdam `Id` e `AtualizadoEm` de `EntidadeBase`. `ProdutoDAO`, `CategoriaDAO`, `FornecedorDAO` e `UsuarioDAO` herdam o CRUD de `DAOBase<T>`. |
| **Encapsulamento** | `Produto.EstoqueAtual` tem `private set`: nenhum código C# consegue alterar o estoque diretamente; ele só muda por movimentação. `EntidadeBase.AtualizadoEm` tem `protected set` e só é alterada pelo método `MarcarComoAtualizado()`. Os DAOs guardam o `DbContext` em campos `protected readonly`. |
| **Polimorfismo** | Os métodos de `DAOBase<T>` são `virtual` e são sobrescritos (`override`) pelos DAOs filhos. Por exemplo, `ProdutoDAO.ListarTodosAsync()` também carrega a categoria e o fornecedor. Os Services trabalham com interfaces (`IProdutoDAO`), sem depender da classe concreta. |
| **Interfaces** | Todo Service e todo DAO tem uma interface (`IProdutoService`, `IProdutoDAO`...). |
| **Métodos com comportamento** | As entidades não são só "sacos de dados": `Produto.EstaAbaixoDoMinimo()`, `Produto.TemEstoquePara(qtd)`, `Fornecedor.CnpjFormatado()`, `Usuario.EhAdmin()`. |
| **Generics** | `IDAOBase<T>` e `DAOBase<T> where T : EntidadeBase` reaproveitam o mesmo código para qualquer entidade de cadastro. |

```mermaid
classDiagram
    class EntidadeBase {
        <<abstract>>
        +int Id
        +DateTime AtualizadoEm
        +MarcarComoAtualizado()
    }
    class Produto {
        +string Nome
        +string CodigoBarras
        +decimal Preco
        +int EstoqueAtual «private set»
        +int EstoqueMinimo
        +bool Ativo
        +EstaAbaixoDoMinimo() bool
        +TemEstoquePara(qtd) bool
    }
    class Categoria {
        +string Nome
        +string Descricao
    }
    class Fornecedor {
        +string RazaoSocial
        +string CNPJ
        +CnpjFormatado() string
    }
    class Usuario {
        +string Nome
        +string Email
        +string SenhaHash
        +PerfilUsuario Perfil
        +EhAdmin() bool
    }
    class Movimentacao {
        +int Id
        +TipoMovimentacao Tipo
        +int Quantidade
        +DateTime Data
    }
    EntidadeBase <|-- Produto
    EntidadeBase <|-- Categoria
    EntidadeBase <|-- Fornecedor
    EntidadeBase <|-- Usuario
    Categoria "1" --> "*" Produto
    Fornecedor "1" --> "*" Produto
    Produto "1" --> "*" Movimentacao
    Usuario "1" --> "*" Movimentacao
```

## 3.4 Injeção de dependência

Os Services e DAOs são registrados no `Program.cs` e o ASP.NET Core entrega as instâncias prontas aos construtores:

```csharp
builder.Services.AddScoped<IProdutoDAO, ProdutoDAO>();
builder.Services.AddScoped<IProdutoService, ProdutoService>();
```

```csharp
public ProdutoService(IProdutoDAO produtoDAO, ICategoriaDAO categoriaDAO, ...)
```

`AddScoped` cria uma instância por requisição HTTP, o mesmo tempo de vida do `AppDbContext`. Com isso, as classes dependem de **interfaces** e não de implementações concretas (baixo acoplamento).

## 3.5 Tratamento de erros

| Tipo de erro | Como é tratado |
|---|---|
| Dado inválido no formulário (campo vazio, texto no preço) | *DataAnnotations* + `ModelState`: a tela volta com a mensagem abaixo do campo. |
| Regra de negócio violada | `RegraNegocioException` lançada pelo Service; o Controller mostra a mensagem. |
| Erro de regra dentro do banco (procedure) | `SqlException` com número ≥ 50000 é convertida em `RegraNegocioException` pelo DAO. |
| Erro inesperado | Em produção, página genérica `/Home/Erro`, sem expor detalhes técnicos. |

## 3.6 Estrutura administrativa (perfis de acesso)

| Perfil | Pode fazer |
|---|---|
| **Admin** | Tudo, inclusive gerenciar usuários. |
| **Gerente** | Cadastros (produtos, categorias, fornecedores), movimentações e relatórios. |
| **Operador** | Consultar produtos e registrar entradas e vendas. |

O controle é feito com `[Authorize(Roles = "...")]` nos Controllers (detalhes em [05-seguranca.md](05-seguranca.md)).

> ⏳ **Em desenvolvimento (etapa 6):** tela de gerenciamento de usuários (somente Admin), registro de movimentações e dashboard definitivo.
