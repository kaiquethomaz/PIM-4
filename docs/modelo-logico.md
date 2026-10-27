# Modelo Lógico — StockControl

SGBD: **Microsoft SQL Server**. Banco: `StockControl`.
O modelo físico (o código que cria tudo isso) é o script [`database/script_stockcontrol.sql`](../database/script_stockcontrol.sql).

O banco atende **vários estabelecimentos** (mercadinhos). Usuários, categorias, fornecedores e produtos têm a coluna `EstabelecimentoId`, e cada estabelecimento só enxerga os próprios dados.

Legenda: **PK** = chave primária · **FK** = chave estrangeira · **UK** = único · **NN** = obrigatório (NOT NULL)

## Estabelecimento

| Coluna | Tipo | Restrições | Descrição |
|---|---|---|---|
| Id | INT IDENTITY | PK | Identificador automático |
| NomeFantasia | NVARCHAR(120) | NN | Nome do mercadinho |
| CNPJ | CHAR(14) | NN, UK, CHECK 14 dígitos numéricos | CNPJ sem máscara (único no sistema) |
| Telefone | NVARCHAR(20) | — | Telefone de contato |
| Email | NVARCHAR(150) | — | E-mail de contato |
| Ativo | BIT | NN, padrão 1 | Estabelecimento inativo: ninguém dele consegue entrar |
| CriadoEm | DATETIME2 | NN, padrão SYSDATETIME() | Data do cadastro |
| AtualizadoEm | DATETIME2 | NN, padrão SYSDATETIME() | Última alteração |

## Usuario

| Coluna | Tipo | Restrições | Descrição |
|---|---|---|---|
| Id | INT IDENTITY | PK | Identificador automático |
| EstabelecimentoId | INT | NN, FK → Estabelecimento(Id) | Onde o usuário trabalha |
| Nome | NVARCHAR(100) | NN | Nome do usuário |
| Email | NVARCHAR(150) | NN, UK, CHECK formato | Login do usuário (único no sistema inteiro) |
| SenhaHash | NVARCHAR(100) | NN | Senha criptografada com BCrypt |
| Perfil | NVARCHAR(20) | NN, CHECK IN ('Admin','Gerente','Operador') | Nível de acesso |
| Ativo | BIT | NN, padrão 1 | Usuário inativo não faz login |
| CriadoEm | DATETIME2 | NN, padrão SYSDATETIME() | Data de cadastro |
| AtualizadoEm | DATETIME2 | NN, padrão SYSDATETIME() | Última alteração |

## Categoria

| Coluna | Tipo | Restrições | Descrição |
|---|---|---|---|
| Id | INT IDENTITY | PK | Identificador automático |
| EstabelecimentoId | INT | NN, FK → Estabelecimento(Id) | Dono da categoria |
| Nome | NVARCHAR(60) | NN, UK junto com EstabelecimentoId | Nome da categoria |
| Descricao | NVARCHAR(250) | — | Descrição opcional |
| AtualizadoEm | DATETIME2 | NN, padrão SYSDATETIME() | Última alteração |

## Fornecedor

| Coluna | Tipo | Restrições | Descrição |
|---|---|---|---|
| Id | INT IDENTITY | PK | Identificador automático |
| EstabelecimentoId | INT | NN, FK → Estabelecimento(Id) | Estabelecimento que compra deste fornecedor |
| RazaoSocial | NVARCHAR(150) | NN | Nome jurídico |
| CNPJ | CHAR(14) | NN, UK junto com EstabelecimentoId, CHECK 14 dígitos | CNPJ sem máscara |
| Telefone | NVARCHAR(20) | — | Telefone de contato |
| Email | NVARCHAR(150) | — | E-mail de contato |
| AtualizadoEm | DATETIME2 | NN, padrão SYSDATETIME() | Última alteração |

## Produto

| Coluna | Tipo | Restrições | Descrição |
|---|---|---|---|
| Id | INT IDENTITY | PK | Identificador automático |
| EstabelecimentoId | INT | NN, FK → Estabelecimento(Id) | Estabelecimento que vende o produto |
| Nome | NVARCHAR(120) | NN | Nome do produto |
| CodigoBarras | VARCHAR(20) | NN, UK junto com EstabelecimentoId | Código EAN lido pelo app/leitor |
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
| ProdutoId | INT | NN, FK → Produto(Id) | Produto movimentado (o estabelecimento é o do produto) |
| Tipo | VARCHAR(7) | NN, CHECK IN ('ENTRADA','SAIDA') | Entrada = compra; Saída = venda |
| Quantidade | INT | NN, CHECK > 0 | Quantidade movimentada |
| Data | DATETIME2 | NN, padrão SYSDATETIME() | Data/hora da movimentação |
| UsuarioId | INT | NN, FK → Usuario(Id) | Quem registrou |
| Observacao | NVARCHAR(250) | — | Texto livre |

## LogAuditoria

| Coluna | Tipo | Restrições | Descrição |
|---|---|---|---|
| Id | INT IDENTITY | PK | Identificador automático |
| EstabelecimentoId | INT | — (sem FK) | Estabelecimento do produto alterado |
| Tabela | NVARCHAR(50) | NN | Tabela alterada (ex.: Produto) |
| Acao | NVARCHAR(20) | NN | `UPDATE_PRECO` ou `DELETE` |
| RegistroId | INT | NN | Id do registro alterado (sem FK, de propósito) |
| ValorAntigo | NVARCHAR(500) | — | Valor antes da mudança |
| ValorNovo | NVARCHAR(500) | — | Valor depois da mudança (NULL na exclusão) |
| Data | DATETIME2 | NN, padrão SYSDATETIME() | Quando aconteceu |

## Índices

| Índice | Tabela (colunas) | Tipo | Por quê |
|---|---|---|---|
| UX_Estabelecimento_CNPJ | Estabelecimento (CNPJ) | Único | Um CNPJ = um estabelecimento |
| UX_Usuario_Email | Usuario (Email) | Único | Login; e-mail não se repete no sistema |
| UX_Categoria_Estab_Nome | Categoria (EstabelecimentoId, Nome) | Único composto | Nome não se repete dentro do estabelecimento |
| UX_Fornecedor_Estab_CNPJ | Fornecedor (EstabelecimentoId, CNPJ) | Único composto | Fornecedor não é cadastrado duas vezes no mesmo estabelecimento |
| UX_Produto_Estab_CodigoBarras | Produto (EstabelecimentoId, CodigoBarras) | Único composto | Busca pelo leitor; código não se repete no estabelecimento |
| IX_Usuario_EstabelecimentoId | Usuario (EstabelecimentoId) | Comum | Listar a equipe do estabelecimento |
| IX_Produto_CategoriaId | Produto (CategoriaId) | Comum | JOIN com Categoria |
| IX_Produto_FornecedorId | Produto (FornecedorId) | Comum | JOIN com Fornecedor |
| IX_Produto_Estab_AtualizadoEm | Produto (EstabelecimentoId, AtualizadoEm) | Comum composto | Sincronização do app (`?atualizadoDesde=`) |
| IX_Movimentacao_ProdutoId | Movimentacao (ProdutoId) | Comum | Histórico do produto |
| IX_Movimentacao_UsuarioId | Movimentacao (UsuarioId) | Comum | JOIN com Usuario |
| IX_Movimentacao_Data | Movimentacao (Data) | Comum | Relatório por período |

Os índices únicos compostos começam por `EstabelecimentoId`, então também aceleram as consultas filtradas por estabelecimento (que são todas).

## Procedures

| Procedure | Parâmetros | O que faz |
|---|---|---|
| `sp_RegistrarMovimentacao` | @EstabelecimentoId, @ProdutoId, @Tipo, @Quantidade, @UsuarioId, @Observacao (opc.), @Data (opc.) | Valida os dados, confere se produto e usuário são do estabelecimento, valida o estoque (na saída) e insere a movimentação em uma transação. Devolve o Id criado. **Não** mexe no estoque: quem faz isso é a trigger. |
| `sp_ProdutosAbaixoDoMinimo` | @EstabelecimentoId | Lista os produtos ativos do estabelecimento com `EstoqueAtual < EstoqueMinimo` (dashboard). |
| `sp_RelatorioMovimentacoes` | @EstabelecimentoId, @DataInicio, @DataFim | Lista as movimentações do estabelecimento no período, com os dois dias inclusos. |

## Triggers

| Trigger | Tabela / evento | O que faz |
|---|---|---|
| `trg_AtualizaEstoque` | Movimentacao / AFTER INSERT | Soma (ENTRADA) ou subtrai (SAIDA) do `EstoqueAtual` e atualiza `AtualizadoEm`. |
| `trg_AuditoriaProduto` | Produto / AFTER UPDATE, DELETE | Grava em `LogAuditoria` a mudança de preço (`UPDATE_PRECO`) e a exclusão (`DELETE`), com o estabelecimento do produto. |

## Normalização

O modelo está na **3ª Forma Normal (3FN)**:
- **1FN:** todas as colunas guardam um único valor (nada de listas dentro de uma coluna).
- **2FN:** as chaves primárias são simples (`Id`), então nenhuma coluna depende só de parte da chave.
- **3FN:** nenhuma coluna depende de outra coluna que não seja a chave. Por exemplo, o nome da categoria fica em `Categoria`, e `Produto` guarda só o `CategoriaId`. Pelo mesmo motivo, `Movimentacao` **não** guarda `EstabelecimentoId`: ele é obtido pelo produto.

Exceção consciente: `Produto.EstoqueAtual` poderia ser calculado somando as movimentações. Ele é guardado (atualizado pela trigger) para o dashboard e o app não precisarem somar todo o histórico a cada consulta. É uma **desnormalização controlada**, feita para desempenho.
