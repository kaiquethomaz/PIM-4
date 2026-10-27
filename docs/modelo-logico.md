# Modelo Lógico — StockControl

SGBD: **Microsoft SQL Server**. Banco: `StockControl`.
O modelo físico (o código que cria tudo isso) é o script [`database/script_stockcontrol.sql`](../database/script_stockcontrol.sql).

Legenda: **PK** = chave primária · **FK** = chave estrangeira · **UK** = único · **NN** = obrigatório (NOT NULL)

## Usuario

| Coluna | Tipo | Restrições | Descrição |
|---|---|---|---|
| Id | INT IDENTITY | PK | Identificador automático |
| Nome | NVARCHAR(100) | NN | Nome do usuário |
| Email | NVARCHAR(150) | NN, UK, CHECK formato | Login do usuário |
| SenhaHash | NVARCHAR(100) | NN | Senha criptografada com BCrypt |
| Perfil | NVARCHAR(20) | NN, CHECK IN ('Admin','Gerente','Operador') | Nível de acesso |
| Ativo | BIT | NN, padrão 1 | Usuário inativo não faz login |
| CriadoEm | DATETIME2 | NN, padrão SYSDATETIME() | Data de cadastro |
| AtualizadoEm | DATETIME2 | NN, padrão SYSDATETIME() | Última alteração |

## Categoria

| Coluna | Tipo | Restrições | Descrição |
|---|---|---|---|
| Id | INT IDENTITY | PK | Identificador automático |
| Nome | NVARCHAR(60) | NN | Nome da categoria |
| Descricao | NVARCHAR(250) | — | Descrição opcional |
| AtualizadoEm | DATETIME2 | NN, padrão SYSDATETIME() | Última alteração |

## Fornecedor

| Coluna | Tipo | Restrições | Descrição |
|---|---|---|---|
| Id | INT IDENTITY | PK | Identificador automático |
| RazaoSocial | NVARCHAR(150) | NN | Nome jurídico |
| CNPJ | CHAR(14) | NN, UK, CHECK 14 dígitos numéricos | CNPJ sem máscara |
| Telefone | NVARCHAR(20) | — | Telefone de contato |
| Email | NVARCHAR(150) | — | E-mail de contato |
| AtualizadoEm | DATETIME2 | NN, padrão SYSDATETIME() | Última alteração |

## Produto

| Coluna | Tipo | Restrições | Descrição |
|---|---|---|---|
| Id | INT IDENTITY | PK | Identificador automático |
| Nome | NVARCHAR(120) | NN | Nome do produto |
| CodigoBarras | VARCHAR(20) | NN, UK | Código EAN lido pelo app/leitor |
| Preco | DECIMAL(10,2) | NN, CHECK >= 0 | Preço de venda |
| EstoqueAtual | INT | NN, padrão 0, CHECK >= 0 | Quantidade em estoque (atualizada pela trigger) |
| EstoqueMinimo | INT | NN, padrão 0, CHECK >= 0 | Abaixo disso, aparece no alerta do dashboard |
| CategoriaId | INT | NN, FK → Categoria(Id) | Categoria do produto |
| FornecedorId | INT | NN, FK → Fornecedor(Id) | Fornecedor do produto |
| Ativo | BIT | NN, padrão 1 | "Exclusão lógica": produto inativo some das telas |
| AtualizadoEm | DATETIME2 | NN, padrão SYSDATETIME() | Última alteração (usada na sincronização do mobile) |

## Movimentacao

| Coluna | Tipo | Restrições | Descrição |
|---|---|---|---|
| Id | INT IDENTITY | PK | Identificador automático |
| ProdutoId | INT | NN, FK → Produto(Id) | Produto movimentado |
| Tipo | VARCHAR(7) | NN, CHECK IN ('ENTRADA','SAIDA') | Entrada = compra; Saída = venda |
| Quantidade | INT | NN, CHECK > 0 | Quantidade movimentada |
| Data | DATETIME2 | NN, padrão SYSDATETIME() | Data/hora da movimentação |
| UsuarioId | INT | NN, FK → Usuario(Id) | Quem registrou |
| Observacao | NVARCHAR(250) | — | Texto livre |

## LogAuditoria

| Coluna | Tipo | Restrições | Descrição |
|---|---|---|---|
| Id | INT IDENTITY | PK | Identificador automático |
| Tabela | NVARCHAR(50) | NN | Tabela alterada (ex.: Produto) |
| Acao | NVARCHAR(20) | NN | `UPDATE_PRECO` ou `DELETE` |
| RegistroId | INT | NN | Id do registro alterado (sem FK, de propósito) |
| ValorAntigo | NVARCHAR(500) | — | Valor antes da mudança |
| ValorNovo | NVARCHAR(500) | — | Valor depois da mudança (NULL na exclusão) |
| Data | DATETIME2 | NN, padrão SYSDATETIME() | Quando aconteceu |

## Índices

| Índice | Tabela (coluna) | Tipo | Por quê |
|---|---|---|---|
| UX_Produto_CodigoBarras | Produto (CodigoBarras) | Único | Busca pelo leitor de código de barras; impede duplicados |
| UX_Usuario_Email | Usuario (Email) | Único | Login; impede e-mail repetido |
| UX_Fornecedor_CNPJ | Fornecedor (CNPJ) | Único | Impede fornecedor duplicado |
| IX_Produto_CategoriaId | Produto (CategoriaId) | Comum | JOIN com Categoria |
| IX_Produto_FornecedorId | Produto (FornecedorId) | Comum | JOIN com Fornecedor |
| IX_Produto_AtualizadoEm | Produto (AtualizadoEm) | Comum | Sincronização do app (`?atualizadoDesde=`) |
| IX_Movimentacao_ProdutoId | Movimentacao (ProdutoId) | Comum | Histórico do produto |
| IX_Movimentacao_UsuarioId | Movimentacao (UsuarioId) | Comum | JOIN com Usuario |
| IX_Movimentacao_Data | Movimentacao (Data) | Comum | Relatório por período |

## Procedures

| Procedure | Parâmetros | O que faz |
|---|---|---|
| `sp_RegistrarMovimentacao` | @ProdutoId, @Tipo, @Quantidade, @UsuarioId, @Observacao (opc.), @Data (opc.) | Valida os dados e o estoque (na saída) e insere a movimentação em uma transação. Devolve o Id criado. **Não** mexe no estoque: quem faz isso é a trigger. |
| `sp_ProdutosAbaixoDoMinimo` | — | Lista produtos ativos com `EstoqueAtual < EstoqueMinimo` (dashboard). |
| `sp_RelatorioMovimentacoes` | @DataInicio, @DataFim | Lista as movimentações do período, com os dois dias inclusos. |

## Triggers

| Trigger | Tabela / evento | O que faz |
|---|---|---|
| `trg_AtualizaEstoque` | Movimentacao / AFTER INSERT | Soma (ENTRADA) ou subtrai (SAIDA) do `EstoqueAtual` e atualiza `AtualizadoEm`. |
| `trg_AuditoriaProduto` | Produto / AFTER UPDATE, DELETE | Grava em `LogAuditoria` a mudança de preço (`UPDATE_PRECO`) e a exclusão (`DELETE`). |

## Normalização

O modelo está na **3ª Forma Normal (3FN)**:
- **1FN:** todas as colunas guardam um único valor (nada de listas dentro de uma coluna).
- **2FN:** as chaves primárias são simples (`Id`), então nenhuma coluna depende só de parte da chave.
- **3FN:** nenhuma coluna depende de outra coluna que não seja a chave. Por exemplo, o nome da categoria fica em `Categoria`, e `Produto` guarda só o `CategoriaId`.

Exceção consciente: `Produto.EstoqueAtual` poderia ser calculado somando as movimentações. Ele é guardado (atualizado pela trigger) para o dashboard e o app não precisarem somar todo o histórico a cada consulta. É uma **desnormalização controlada**, feita para desempenho.
