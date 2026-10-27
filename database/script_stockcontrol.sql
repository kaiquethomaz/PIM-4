/* =====================================================================
   StockControl - Script completo do banco de dados (SQL Server)
   PIM IV - ADS UNIP

   Este script É o modelo físico do banco. Ele é usado pelo sistema web,
   pela API e (por meio da API) pelo app mobile.
   O Entity Framework NÃO cria o banco (não usamos migrations): ele só
   "enxerga" as tabelas criadas aqui, via Fluent API.

   O sistema atende VÁRIOS estabelecimentos (mercadinhos) no mesmo banco.
   Cada usuário, categoria, fornecedor e produto pertence a um
   estabelecimento, e cada estabelecimento só enxerga os próprios dados.

   Ordem do script:
     1. CREATE DATABASE / USE
     2. CREATE TABLE (PK, FK, UNIQUE, CHECK)
     3. Índices
     4. Procedures
     5. Triggers
     6. Dados de teste (seed)
     7. Comandos de teste (comentados)

   Como rodar: abra no SQL Server Management Studio (SSMS) e aperte F5.
   O script pode ser executado de novo: ele apaga e recria as tabelas
   (CUIDADO: isso apaga todos os dados).
   ===================================================================== */


/* =====================================================================
   1. CREATE DATABASE / USE
   ===================================================================== */
IF DB_ID(N'StockControl') IS NULL
    CREATE DATABASE StockControl;
GO

USE StockControl;
GO

-- Apaga os objetos antigos (ordem inversa das dependências) para o
-- script poder ser executado várias vezes durante o desenvolvimento.
DROP TRIGGER   IF EXISTS trg_AtualizaEstoque;
DROP TRIGGER   IF EXISTS trg_AuditoriaProduto;
DROP PROCEDURE IF EXISTS sp_RegistrarMovimentacao;
DROP PROCEDURE IF EXISTS sp_ProdutosAbaixoDoMinimo;
DROP PROCEDURE IF EXISTS sp_RelatorioMovimentacoes;
DROP TABLE     IF EXISTS LogAuditoria;
DROP TABLE     IF EXISTS Movimentacao;
DROP TABLE     IF EXISTS Produto;
DROP TABLE     IF EXISTS Fornecedor;
DROP TABLE     IF EXISTS Categoria;
DROP TABLE     IF EXISTS Usuario;
DROP TABLE     IF EXISTS Estabelecimento;
GO


/* =====================================================================
   2. CREATE TABLE
   Convenções:
     - PK sempre "Id" INT IDENTITY (número automático).
     - Datas em DATETIME2 com padrão SYSDATETIME() (data/hora do servidor).
     - "AtualizadoEm" existe nas tabelas de cadastro: o app mobile usa
       essa coluna para baixar só o que mudou (sincronização).
     - Textos em NVARCHAR para aceitar acentos.
     - CNPJ guardado só com os 14 números (sem pontos, barra e traço).
   ===================================================================== */

-- Estabelecimentos (mercadinhos) que usam o sistema.
-- É criado pela tela "Cadastrar meu estabelecimento", junto com o
-- primeiro usuário (o dono, com perfil Admin).
CREATE TABLE Estabelecimento (
    Id           INT IDENTITY(1,1) NOT NULL,
    NomeFantasia NVARCHAR(120)     NOT NULL,
    CNPJ         CHAR(14)          NOT NULL,
    Telefone     NVARCHAR(20)      NULL,
    Email        NVARCHAR(150)     NULL,
    Ativo        BIT               NOT NULL CONSTRAINT DF_Estabelecimento_Ativo        DEFAULT 1,
    CriadoEm     DATETIME2         NOT NULL CONSTRAINT DF_Estabelecimento_CriadoEm     DEFAULT SYSDATETIME(),
    AtualizadoEm DATETIME2         NOT NULL CONSTRAINT DF_Estabelecimento_AtualizadoEm DEFAULT SYSDATETIME(),

    CONSTRAINT PK_Estabelecimento      PRIMARY KEY (Id),
    CONSTRAINT CK_Estabelecimento_CNPJ CHECK (LEN(CNPJ) = 14 AND CNPJ NOT LIKE '%[^0-9]%')
);

-- Usuários do sistema (quem faz login na web e no app).
-- O e-mail é o login, por isso é único no sistema inteiro.
CREATE TABLE Usuario (
    Id                INT IDENTITY(1,1) NOT NULL,
    EstabelecimentoId INT               NOT NULL,
    Nome              NVARCHAR(100)     NOT NULL,
    Email             NVARCHAR(150)     NOT NULL,
    SenhaHash         NVARCHAR(100)     NOT NULL,          -- hash BCrypt, nunca a senha pura
    Perfil            NVARCHAR(20)      NOT NULL,
    Ativo             BIT               NOT NULL CONSTRAINT DF_Usuario_Ativo        DEFAULT 1,
    CriadoEm          DATETIME2         NOT NULL CONSTRAINT DF_Usuario_CriadoEm     DEFAULT SYSDATETIME(),
    AtualizadoEm      DATETIME2         NOT NULL CONSTRAINT DF_Usuario_AtualizadoEm DEFAULT SYSDATETIME(),

    CONSTRAINT PK_Usuario                 PRIMARY KEY (Id),
    CONSTRAINT FK_Usuario_Estabelecimento FOREIGN KEY (EstabelecimentoId) REFERENCES Estabelecimento (Id),
    CONSTRAINT CK_Usuario_Perfil          CHECK (Perfil IN (N'Admin', N'Gerente', N'Operador')),
    CONSTRAINT CK_Usuario_Email           CHECK (Email LIKE N'%_@_%._%')
);

-- Categorias de produtos (Mercearia, Bebidas, Limpeza...)
CREATE TABLE Categoria (
    Id                INT IDENTITY(1,1) NOT NULL,
    EstabelecimentoId INT               NOT NULL,
    Nome              NVARCHAR(60)      NOT NULL,
    Descricao         NVARCHAR(250)     NULL,
    AtualizadoEm      DATETIME2         NOT NULL CONSTRAINT DF_Categoria_AtualizadoEm DEFAULT SYSDATETIME(),

    CONSTRAINT PK_Categoria                 PRIMARY KEY (Id),
    CONSTRAINT FK_Categoria_Estabelecimento FOREIGN KEY (EstabelecimentoId) REFERENCES Estabelecimento (Id)
);

-- Fornecedores de cada estabelecimento
CREATE TABLE Fornecedor (
    Id                INT IDENTITY(1,1) NOT NULL,
    EstabelecimentoId INT               NOT NULL,
    RazaoSocial       NVARCHAR(150)     NOT NULL,
    CNPJ              CHAR(14)          NOT NULL,
    Telefone          NVARCHAR(20)      NULL,
    Email             NVARCHAR(150)     NULL,
    AtualizadoEm      DATETIME2         NOT NULL CONSTRAINT DF_Fornecedor_AtualizadoEm DEFAULT SYSDATETIME(),

    CONSTRAINT PK_Fornecedor                 PRIMARY KEY (Id),
    CONSTRAINT FK_Fornecedor_Estabelecimento FOREIGN KEY (EstabelecimentoId) REFERENCES Estabelecimento (Id),
    CONSTRAINT CK_Fornecedor_CNPJ            CHECK (LEN(CNPJ) = 14 AND CNPJ NOT LIKE '%[^0-9]%')
);

-- Produtos vendidos em cada estabelecimento
CREATE TABLE Produto (
    Id                INT IDENTITY(1,1) NOT NULL,
    EstabelecimentoId INT               NOT NULL,
    Nome              NVARCHAR(120)     NOT NULL,
    CodigoBarras      VARCHAR(20)       NOT NULL,
    Preco             DECIMAL(10,2)     NOT NULL,
    EstoqueAtual      INT               NOT NULL CONSTRAINT DF_Produto_EstoqueAtual DEFAULT 0,
    EstoqueMinimo     INT               NOT NULL CONSTRAINT DF_Produto_EstoqueMinimo DEFAULT 0,
    CategoriaId       INT               NOT NULL,
    FornecedorId      INT               NOT NULL,
    Ativo             BIT               NOT NULL CONSTRAINT DF_Produto_Ativo        DEFAULT 1,
    AtualizadoEm      DATETIME2         NOT NULL CONSTRAINT DF_Produto_AtualizadoEm DEFAULT SYSDATETIME(),

    CONSTRAINT PK_Produto                 PRIMARY KEY (Id),
    CONSTRAINT FK_Produto_Estabelecimento FOREIGN KEY (EstabelecimentoId) REFERENCES Estabelecimento (Id),
    CONSTRAINT FK_Produto_Categoria       FOREIGN KEY (CategoriaId)  REFERENCES Categoria (Id),
    CONSTRAINT FK_Produto_Fornecedor      FOREIGN KEY (FornecedorId) REFERENCES Fornecedor (Id),
    CONSTRAINT CK_Produto_Preco           CHECK (Preco >= 0),
    -- Segurança extra: mesmo que alguém insira direto no banco, o estoque
    -- nunca fica negativo (a trigger falha e a operação é desfeita).
    CONSTRAINT CK_Produto_EstoqueAtual    CHECK (EstoqueAtual >= 0),
    CONSTRAINT CK_Produto_EstoqueMinimo   CHECK (EstoqueMinimo >= 0)
);

-- Movimentações de estoque. Uma "venda" é uma movimentação do tipo SAIDA.
-- Não tem EstabelecimentoId: o estabelecimento é o do produto
-- (evita guardar a mesma informação em dois lugares - 3ª forma normal).
CREATE TABLE Movimentacao (
    Id         INT IDENTITY(1,1) NOT NULL,
    ProdutoId  INT               NOT NULL,
    Tipo       VARCHAR(7)        NOT NULL,
    Quantidade INT               NOT NULL,
    Data       DATETIME2         NOT NULL CONSTRAINT DF_Movimentacao_Data DEFAULT SYSDATETIME(),
    UsuarioId  INT               NOT NULL,
    Observacao NVARCHAR(250)     NULL,

    CONSTRAINT PK_Movimentacao         PRIMARY KEY (Id),
    CONSTRAINT FK_Movimentacao_Produto FOREIGN KEY (ProdutoId) REFERENCES Produto (Id),
    CONSTRAINT FK_Movimentacao_Usuario FOREIGN KEY (UsuarioId) REFERENCES Usuario (Id),
    CONSTRAINT CK_Movimentacao_Tipo       CHECK (Tipo IN ('ENTRADA', 'SAIDA')),
    CONSTRAINT CK_Movimentacao_Quantidade CHECK (Quantidade > 0)
);

-- Histórico de alterações importantes (preenchido pela trigger de auditoria).
-- Não tem FK para Produto de propósito: o log precisa continuar existindo
-- mesmo depois que o produto é excluído.
CREATE TABLE LogAuditoria (
    Id                INT IDENTITY(1,1) NOT NULL,
    EstabelecimentoId INT               NULL,
    Tabela            NVARCHAR(50)      NOT NULL,
    Acao              NVARCHAR(20)      NOT NULL,
    RegistroId        INT               NOT NULL,
    ValorAntigo       NVARCHAR(500)     NULL,
    ValorNovo         NVARCHAR(500)     NULL,
    Data              DATETIME2         NOT NULL CONSTRAINT DF_LogAuditoria_Data DEFAULT SYSDATETIME(),

    CONSTRAINT PK_LogAuditoria PRIMARY KEY (Id)
);
GO


/* =====================================================================
   3. ÍNDICES (otimização)
   - Índices ÚNICOS: além de deixar a busca rápida, impedem duplicados.
     Código de barras, CNPJ de fornecedor e nome de categoria são únicos
     POR ESTABELECIMENTO (dois mercadinhos podem vender o mesmo arroz,
     com o mesmo código de barras, e comprar do mesmo fornecedor).
     O e-mail do usuário e o CNPJ do estabelecimento são únicos no sistema todo.
   - Índices nas FKs: deixam os JOINs mais rápidos.
   - Índice em Movimentacao.Data: relatórios por período.
   - Índice em Produto (EstabelecimentoId, AtualizadoEm): sincronização do app.
   ===================================================================== */
CREATE UNIQUE INDEX UX_Estabelecimento_CNPJ      ON Estabelecimento (CNPJ);
CREATE UNIQUE INDEX UX_Usuario_Email             ON Usuario (Email);
CREATE UNIQUE INDEX UX_Categoria_Estab_Nome      ON Categoria (EstabelecimentoId, Nome);
CREATE UNIQUE INDEX UX_Fornecedor_Estab_CNPJ     ON Fornecedor (EstabelecimentoId, CNPJ);
CREATE UNIQUE INDEX UX_Produto_Estab_CodigoBarras ON Produto (EstabelecimentoId, CodigoBarras);

CREATE INDEX IX_Usuario_EstabelecimentoId    ON Usuario (EstabelecimentoId);
CREATE INDEX IX_Produto_CategoriaId          ON Produto (CategoriaId);
CREATE INDEX IX_Produto_FornecedorId         ON Produto (FornecedorId);
CREATE INDEX IX_Produto_Estab_AtualizadoEm   ON Produto (EstabelecimentoId, AtualizadoEm);
CREATE INDEX IX_Movimentacao_ProdutoId       ON Movimentacao (ProdutoId);
CREATE INDEX IX_Movimentacao_UsuarioId       ON Movimentacao (UsuarioId);
CREATE INDEX IX_Movimentacao_Data            ON Movimentacao (Data);
GO


/* =====================================================================
   4. PROCEDURES
   Todas recebem @EstabelecimentoId: cada mercadinho só mexe e só
   consulta os próprios dados.
   ===================================================================== */

/* ---------------------------------------------------------------------
   sp_RegistrarMovimentacao
   Registra uma entrada ou saída de estoque.
   - Valida os dados e, na SAIDA, verifica se há estoque suficiente.
   - Confere se o produto e o usuário são do estabelecimento informado.
   - Insere a movimentação dentro de uma transação.
   - NÃO atualiza o estoque: quem faz isso é a trigger trg_AtualizaEstoque
     (se a procedure também atualizasse, o estoque seria contado 2 vezes).
   - Devolve o Id da movimentação criada.
   Erros (THROW): 50001 produto não existe/inativo, 50002 usuário inválido,
   50003 tipo inválido, 50004 quantidade inválida, 50005 estoque insuficiente.
   --------------------------------------------------------------------- */
CREATE OR ALTER PROCEDURE sp_RegistrarMovimentacao
    @EstabelecimentoId INT,
    @ProdutoId         INT,
    @Tipo              VARCHAR(7),
    @Quantidade        INT,
    @UsuarioId         INT,
    @Observacao        NVARCHAR(250) = NULL,
    @Data              DATETIME2     = NULL      -- opcional; se vier NULL usa a data/hora atual
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;   -- qualquer erro desfaz a transação inteira

    SET @Tipo = UPPER(LTRIM(RTRIM(@Tipo)));

    IF @Tipo IS NULL OR @Tipo NOT IN ('ENTRADA', 'SAIDA')
        THROW 50003, N'Tipo de movimentação inválido. Use ENTRADA ou SAIDA.', 1;

    IF @Quantidade IS NULL OR @Quantidade <= 0
        THROW 50004, N'A quantidade deve ser maior que zero.', 1;

    IF NOT EXISTS (SELECT 1 FROM Usuario
                   WHERE Id = @UsuarioId AND Ativo = 1 AND EstabelecimentoId = @EstabelecimentoId)
        THROW 50002, N'Usuário inexistente ou inativo.', 1;

    BEGIN TRY
        BEGIN TRANSACTION;

        -- Lê o estoque "travando" a linha do produto (UPDLOCK, HOLDLOCK).
        -- Assim, se duas vendas do mesmo produto chegarem ao mesmo tempo
        -- (web e celular), uma espera a outra e não vendemos o que não existe.
        DECLARE @EstoqueAtual INT;

        SELECT @EstoqueAtual = EstoqueAtual
        FROM Produto WITH (UPDLOCK, HOLDLOCK)
        WHERE Id = @ProdutoId
          AND Ativo = 1
          AND EstabelecimentoId = @EstabelecimentoId;   -- produto de outro mercadinho não é encontrado

        IF @EstoqueAtual IS NULL
            THROW 50001, N'Produto inexistente ou inativo.', 1;

        IF @Tipo = 'SAIDA' AND @Quantidade > @EstoqueAtual
            THROW 50005, N'Estoque insuficiente para esta saída.', 1;

        INSERT INTO Movimentacao (ProdutoId, Tipo, Quantidade, Data, UsuarioId, Observacao)
        VALUES (@ProdutoId, @Tipo, @Quantidade, COALESCE(@Data, SYSDATETIME()), @UsuarioId, @Observacao);
        -- ^ este INSERT dispara a trigger trg_AtualizaEstoque

        -- SCOPE_IDENTITY pega o Id gerado AQUI (e não um Id gerado dentro da trigger)
        DECLARE @NovoId INT = CAST(SCOPE_IDENTITY() AS INT);

        COMMIT TRANSACTION;

        SELECT @NovoId AS Id;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        THROW;   -- repassa o erro original para quem chamou (API/web)
    END CATCH
END
GO

/* ---------------------------------------------------------------------
   sp_ProdutosAbaixoDoMinimo
   Lista os produtos ativos do estabelecimento com EstoqueAtual < EstoqueMinimo.
   Usada no Dashboard (alerta de reposição).
   --------------------------------------------------------------------- */
CREATE OR ALTER PROCEDURE sp_ProdutosAbaixoDoMinimo
    @EstabelecimentoId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        p.Id,
        p.Nome,
        p.CodigoBarras,
        c.Nome                            AS Categoria,
        p.EstoqueAtual,
        p.EstoqueMinimo,
        p.EstoqueMinimo - p.EstoqueAtual  AS QuantidadeFaltante
    FROM Produto p
    INNER JOIN Categoria c ON c.Id = p.CategoriaId
    WHERE p.EstabelecimentoId = @EstabelecimentoId
      AND p.Ativo = 1
      AND p.EstoqueAtual < p.EstoqueMinimo
    ORDER BY QuantidadeFaltante DESC, p.Nome;
END
GO

/* ---------------------------------------------------------------------
   sp_RelatorioMovimentacoes
   Lista as movimentações do estabelecimento entre duas datas (os dois dias inclusos).
   Ex.: @DataInicio = '2026-11-01', @DataFim = '2026-11-01' traz o dia 1 inteiro.
   --------------------------------------------------------------------- */
CREATE OR ALTER PROCEDURE sp_RelatorioMovimentacoes
    @EstabelecimentoId INT,
    @DataInicio        DATE,
    @DataFim           DATE
AS
BEGIN
    SET NOCOUNT ON;

    IF @DataInicio > @DataFim
        THROW 50010, N'A data inicial não pode ser maior que a data final.', 1;

    SELECT
        m.Id,
        m.Data,
        m.Tipo,
        m.ProdutoId,
        p.Nome        AS Produto,
        m.Quantidade,
        m.UsuarioId,
        u.Nome        AS Usuario,
        m.Observacao
    FROM Movimentacao m
    INNER JOIN Produto p ON p.Id = m.ProdutoId
    INNER JOIN Usuario u ON u.Id = m.UsuarioId
    WHERE p.EstabelecimentoId = @EstabelecimentoId
      AND m.Data >= @DataInicio
      AND m.Data <  DATEADD(DAY, 1, @DataFim)   -- "< dia seguinte" inclui o dia final inteiro
    ORDER BY m.Data DESC, m.Id DESC;
END
GO


/* =====================================================================
   5. TRIGGERS
   Obs.: dentro de trigger, as tabelas especiais "inserted" e "deleted"
   têm as linhas novas e as antigas. Um único comando pode afetar várias
   linhas, por isso as triggers trabalham com conjuntos (JOIN/GROUP BY),
   e não com uma linha só.
   ===================================================================== */

/* ---------------------------------------------------------------------
   trg_AtualizaEstoque
   Depois de inserir uma movimentação, soma (ENTRADA) ou subtrai (SAIDA)
   a quantidade do EstoqueAtual do produto e atualiza AtualizadoEm.
   Se o estoque ficar negativo, o CHECK CK_Produto_EstoqueAtual falha e
   tudo é desfeito (inclusive o INSERT da movimentação).
   --------------------------------------------------------------------- */
CREATE OR ALTER TRIGGER trg_AtualizaEstoque
ON Movimentacao
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE p
    SET p.EstoqueAtual = p.EstoqueAtual + t.Variacao,
        p.AtualizadoEm = SYSDATETIME()
    FROM Produto p
    INNER JOIN (
        SELECT ProdutoId,
               SUM(CASE WHEN Tipo = 'ENTRADA' THEN Quantidade ELSE -Quantidade END) AS Variacao
        FROM inserted
        GROUP BY ProdutoId
    ) t ON t.ProdutoId = p.Id;
END
GO

/* ---------------------------------------------------------------------
   trg_AuditoriaProduto
   Grava em LogAuditoria (com o estabelecimento do produto):
     - UPDATE_PRECO: quando o preço de um produto muda;
     - DELETE:       quando um produto é excluído.
   Obs.: a trigger de estoque também faz UPDATE em Produto; como ali o
   preço não muda, o filtro "Preco <> Preco" evita log desnecessário.
   --------------------------------------------------------------------- */
CREATE OR ALTER TRIGGER trg_AuditoriaProduto
ON Produto
AFTER UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    -- Alteração de preço (existe em "inserted" e em "deleted")
    INSERT INTO LogAuditoria (EstabelecimentoId, Tabela, Acao, RegistroId, ValorAntigo, ValorNovo)
    SELECT i.EstabelecimentoId,
           N'Produto',
           N'UPDATE_PRECO',
           i.Id,
           CONVERT(NVARCHAR(30), d.Preco),
           CONVERT(NVARCHAR(30), i.Preco)
    FROM inserted i
    INNER JOIN deleted d ON d.Id = i.Id
    WHERE i.Preco <> d.Preco;

    -- Exclusão (existe só em "deleted")
    INSERT INTO LogAuditoria (EstabelecimentoId, Tabela, Acao, RegistroId, ValorAntigo, ValorNovo)
    SELECT d.EstabelecimentoId,
           N'Produto',
           N'DELETE',
           d.Id,
           CONCAT(N'Nome=', d.Nome, N'; CodigoBarras=', d.CodigoBarras,
                  N'; Preco=', d.Preco, N'; Estoque=', d.EstoqueAtual),
           NULL
    FROM deleted d
    WHERE NOT EXISTS (SELECT 1 FROM inserted i WHERE i.Id = d.Id);
END
GO


/* =====================================================================
   6. DADOS DE TESTE (seed)
   Dois estabelecimentos, para demonstrar que um não enxerga o outro:

   Estabelecimento 1 - Mercadinho Bom Preço (dados completos)
     admin@stockcontrol.local    / Admin@123     (Admin)
     gerente@stockcontrol.local  / Gerente@123   (Gerente)
     operador@stockcontrol.local / Operador@123  (Operador)

   Estabelecimento 2 - Mercearia Dona Maria (dados menores)
     maria@donamaria.local       / Maria@123     (Admin)

   No banco fica só o hash BCrypt das senhas.
   CNPJs e códigos de barras são fictícios (os CNPJs têm dígitos
   verificadores válidos, porque o sistema valida isso).
   ===================================================================== */

INSERT INTO Estabelecimento (NomeFantasia, CNPJ, Telefone, Email) VALUES
(N'Mercadinho Bom Preço', '12345678000195', N'(11) 4000-1000', N'contato@bompreco.exemplo'),   -- Id 1
(N'Mercearia Dona Maria', '98765432000198', N'(11) 4000-2000', N'contato@donamaria.exemplo');  -- Id 2

INSERT INTO Usuario (EstabelecimentoId, Nome, Email, SenhaHash, Perfil) VALUES
(1, N'Administrador',    N'admin@stockcontrol.local',    N'$2a$11$iGuz4A17/wpP64qoR5apnuc2XzDwJKWwHkXAJVFrqv9JvzsnqjOW.', N'Admin'),     -- Id 1
(1, N'Gabriela Gerente', N'gerente@stockcontrol.local',  N'$2a$11$6jx84l99EUD8ww.kfclsLOuhKm2TKQAF2i0mY8seE5MbHlK.u5Gwi', N'Gerente'),   -- Id 2
(1, N'Otávio Operador',  N'operador@stockcontrol.local', N'$2a$11$pLLKZXqdrMpnQZAVQGo7S.WcKuxKhd3Rb0kvy5b/mTcRN9DaXoiZi', N'Operador'),  -- Id 3
(2, N'Maria Aparecida',  N'maria@donamaria.local',       N'$2a$11$8fiYvbranwQZAhNeczqnWeoAeXkFYextWAxKuF5DkRlZfx0R2vG72', N'Admin');     -- Id 4

-- Estabelecimento 1
INSERT INTO Categoria (EstabelecimentoId, Nome, Descricao) VALUES
(1, N'Mercearia',          N'Grãos, massas, óleos e itens básicos'),       -- Id 1
(1, N'Bebidas',            N'Refrigerantes, águas e sucos'),               -- Id 2
(1, N'Laticínios e Frios', N'Leite, queijos e derivados'),                 -- Id 3
(1, N'Limpeza',            N'Produtos de limpeza da casa'),                -- Id 4
(1, N'Higiene Pessoal',    N'Sabonetes, papel higiênico e similares');     -- Id 5

-- Estabelecimento 2 ("Mercearia" de novo: o nome é único só dentro de cada estabelecimento)
INSERT INTO Categoria (EstabelecimentoId, Nome, Descricao) VALUES
(2, N'Mercearia',  N'Itens básicos'),                                      -- Id 6
(2, N'Hortifrúti', N'Frutas, legumes e verduras');                         -- Id 7

INSERT INTO Fornecedor (EstabelecimentoId, RazaoSocial, CNPJ, Telefone, Email) VALUES
(1, N'Distribuidora Alfa Alimentos Ltda', '11222333000181', N'(11) 3333-1001', N'vendas@alfaalimentos.exemplo'),  -- Id 1
(1, N'Beta Bebidas Distribuidora Ltda',   '22333444000181', N'(11) 3333-2002', N'pedidos@betabebidas.exemplo'),   -- Id 2
(1, N'Gama Limpeza e Higiene Ltda',       '33444555000181', N'(11) 3333-3003', N'contato@gamalimpeza.exemplo'),   -- Id 3
-- O mesmo fornecedor (mesmo CNPJ) também atende o estabelecimento 2:
(2, N'Distribuidora Alfa Alimentos Ltda', '11222333000181', N'(11) 3333-1001', N'vendas@alfaalimentos.exemplo');  -- Id 4

-- Os produtos começam com EstoqueAtual = 0. O estoque é formado pelas
-- movimentações logo abaixo (passando pela procedure e pela trigger),
-- exatamente como acontece no sistema de verdade.
INSERT INTO Produto (EstabelecimentoId, Nome, CodigoBarras, Preco, EstoqueMinimo, CategoriaId, FornecedorId) VALUES
(1, N'Arroz Branco Tipo 1 5kg',      '7891000000011', 28.90, 10, 1, 1),  -- Id 1
(1, N'Feijão Carioca 1kg',           '7891000000028',  9.49, 15, 1, 1),  -- Id 2
(1, N'Açúcar Refinado 1kg',          '7891000000035',  4.79, 10, 1, 1),  -- Id 3
(1, N'Café Torrado e Moído 500g',    '7891000000042', 18.90,  8, 1, 1),  -- Id 4
(1, N'Óleo de Soja 900ml',           '7891000000059',  7.99, 12, 1, 1),  -- Id 5
(1, N'Macarrão Espaguete 500g',      '7891000000066',  4.29, 10, 1, 1),  -- Id 6
(1, N'Refrigerante Cola 2L',         '7891000000073',  9.99, 12, 2, 2),  -- Id 7
(1, N'Água Mineral 500ml',           '7891000000080',  2.50, 24, 2, 2),  -- Id 8
(1, N'Suco de Laranja 1L',           '7891000000097',  8.49,  6, 2, 2),  -- Id 9
(1, N'Leite Integral 1L',            '7891000000103',  5.49, 24, 3, 1),  -- Id 10
(1, N'Queijo Mussarela 500g',        '7891000000110', 24.90,  5, 3, 1),  -- Id 11
(1, N'Detergente Líquido 500ml',     '7891000000127',  2.99, 12, 4, 3),  -- Id 12
(1, N'Sabão em Pó 1kg',              '7891000000134', 12.90,  6, 4, 3),  -- Id 13
(1, N'Papel Higiênico 12 rolos',     '7891000000141', 21.90,  6, 5, 3),  -- Id 14
(1, N'Sabonete 90g',                 '7891000000158',  2.19, 20, 5, 3);  -- Id 15

-- Estabelecimento 2 (o Arroz tem o MESMO código de barras do estabelecimento 1:
-- permitido, porque o código é único só dentro de cada estabelecimento)
INSERT INTO Produto (EstabelecimentoId, Nome, CodigoBarras, Preco, EstoqueMinimo, CategoriaId, FornecedorId) VALUES
(2, N'Arroz Branco Tipo 1 5kg',      '7891000000011', 27.50,  8, 6, 4),  -- Id 16
(2, N'Feijão Carioca 1kg',           '7891000000028',  9.90,  6, 6, 4),  -- Id 17
(2, N'Ovos Brancos 12 unidades',     '7893000000031', 13.90,  5, 6, 4),  -- Id 18
(2, N'Banana Prata (kg)',            '7893000000017',  6.99, 10, 7, 4),  -- Id 19
(2, N'Tomate (kg)',                  '7893000000024',  8.49,  8, 7, 4);  -- Id 20

-- Movimentações dos últimos 30 dias, registradas pela procedure.
-- Como as tabelas acabaram de ser criadas, os Ids são 1, 2, 3... na
-- ordem dos INSERTs acima.
DECLARE @d30  DATETIME2 = DATEADD(DAY, -30, SYSDATETIME());
DECLARE @d20  DATETIME2 = DATEADD(DAY, -20, SYSDATETIME());
DECLARE @d10  DATETIME2 = DATEADD(DAY, -10, SYSDATETIME());
DECLARE @d3   DATETIME2 = DATEADD(DAY,  -3, SYSDATETIME());
DECLARE @d1   DATETIME2 = DATEADD(DAY,  -1, SYSDATETIME());
DECLARE @hoje DATETIME2 = SYSDATETIME();

-- ---------- Estabelecimento 1: Admin (usuário 1) registra as entradas; Operador (3) as vendas ----------
-- Parâmetros: @EstabelecimentoId, @ProdutoId, @Tipo, @Quantidade, @UsuarioId, @Observacao, @Data
EXEC sp_RegistrarMovimentacao 1,  1, 'ENTRADA', 40, 1, N'Estoque inicial', @d30;
EXEC sp_RegistrarMovimentacao 1,  2, 'ENTRADA', 50, 1, N'Estoque inicial', @d30;
EXEC sp_RegistrarMovimentacao 1,  3, 'ENTRADA', 30, 1, N'Estoque inicial', @d30;
EXEC sp_RegistrarMovimentacao 1,  4, 'ENTRADA', 24, 1, N'Estoque inicial', @d30;
EXEC sp_RegistrarMovimentacao 1,  5, 'ENTRADA', 36, 1, N'Estoque inicial', @d30;
EXEC sp_RegistrarMovimentacao 1,  6, 'ENTRADA', 40, 1, N'Estoque inicial', @d30;
EXEC sp_RegistrarMovimentacao 1,  7, 'ENTRADA', 48, 1, N'Estoque inicial', @d30;
EXEC sp_RegistrarMovimentacao 1,  8, 'ENTRADA', 96, 1, N'Estoque inicial', @d30;
EXEC sp_RegistrarMovimentacao 1,  9, 'ENTRADA', 20, 1, N'Estoque inicial', @d30;
EXEC sp_RegistrarMovimentacao 1, 10, 'ENTRADA', 60, 1, N'Estoque inicial', @d30;
EXEC sp_RegistrarMovimentacao 1, 11, 'ENTRADA', 15, 1, N'Estoque inicial', @d30;
EXEC sp_RegistrarMovimentacao 1, 12, 'ENTRADA', 36, 1, N'Estoque inicial', @d30;
EXEC sp_RegistrarMovimentacao 1, 13, 'ENTRADA', 18, 1, N'Estoque inicial', @d30;
EXEC sp_RegistrarMovimentacao 1, 14, 'ENTRADA', 20, 1, N'Estoque inicial', @d30;
EXEC sp_RegistrarMovimentacao 1, 15, 'ENTRADA', 60, 1, N'Estoque inicial', @d30;

EXEC sp_RegistrarMovimentacao 1,  1, 'SAIDA', 12, 3, N'Venda balcão', @d20;
EXEC sp_RegistrarMovimentacao 1,  2, 'SAIDA', 20, 3, N'Venda balcão', @d20;
EXEC sp_RegistrarMovimentacao 1,  7, 'SAIDA', 20, 3, N'Venda balcão', @d20;
EXEC sp_RegistrarMovimentacao 1,  8, 'SAIDA', 40, 3, N'Venda balcão', @d20;
EXEC sp_RegistrarMovimentacao 1, 10, 'SAIDA', 25, 3, N'Venda balcão', @d20;
EXEC sp_RegistrarMovimentacao 1, 12, 'SAIDA', 18, 3, N'Venda balcão', @d20;
EXEC sp_RegistrarMovimentacao 1,  4, 'SAIDA', 10, 3, N'Venda balcão', @d10;
EXEC sp_RegistrarMovimentacao 1,  5, 'SAIDA', 14, 3, N'Venda balcão', @d10;
EXEC sp_RegistrarMovimentacao 1,  6, 'SAIDA', 15, 3, N'Venda balcão', @d10;
EXEC sp_RegistrarMovimentacao 1, 15, 'SAIDA', 22, 3, N'Venda balcão', @d10;

EXEC sp_RegistrarMovimentacao 1,  1, 'ENTRADA', 10, 1, N'Reposição fornecedor', @d10;

EXEC sp_RegistrarMovimentacao 1,  1, 'SAIDA',  9, 3, N'Venda balcão', @d3;
EXEC sp_RegistrarMovimentacao 1,  2, 'SAIDA', 18, 3, N'Venda balcão', @d3;
EXEC sp_RegistrarMovimentacao 1,  3, 'SAIDA',  8, 3, N'Venda balcão', @d3;
EXEC sp_RegistrarMovimentacao 1,  9, 'SAIDA',  6, 3, N'Venda balcão', @d3;
EXEC sp_RegistrarMovimentacao 1, 11, 'SAIDA',  4, 3, N'Venda balcão', @d3;
EXEC sp_RegistrarMovimentacao 1,  7, 'SAIDA', 16, 3, N'Venda balcão', @d1;
EXEC sp_RegistrarMovimentacao 1,  8, 'SAIDA', 38, 3, N'Venda balcão', @d1;
EXEC sp_RegistrarMovimentacao 1, 10, 'SAIDA', 20, 3, N'Venda balcão', @d1;
EXEC sp_RegistrarMovimentacao 1, 13, 'SAIDA',  5, 3, N'Venda balcão', @d1;
EXEC sp_RegistrarMovimentacao 1,  4, 'SAIDA',  9, 3, N'Venda balcão', @hoje;
EXEC sp_RegistrarMovimentacao 1, 12, 'SAIDA', 15, 3, N'Venda balcão', @hoje;
EXEC sp_RegistrarMovimentacao 1, 14, 'SAIDA',  7, 3, N'Venda balcão', @hoje;

-- ---------- Estabelecimento 2: Maria (usuário 4) registra tudo ----------
EXEC sp_RegistrarMovimentacao 2, 16, 'ENTRADA', 20, 4, N'Estoque inicial', @d20;
EXEC sp_RegistrarMovimentacao 2, 17, 'ENTRADA', 15, 4, N'Estoque inicial', @d20;
EXEC sp_RegistrarMovimentacao 2, 18, 'ENTRADA', 12, 4, N'Estoque inicial', @d20;
EXEC sp_RegistrarMovimentacao 2, 19, 'ENTRADA', 30, 4, N'Estoque inicial', @d20;
EXEC sp_RegistrarMovimentacao 2, 20, 'ENTRADA', 25, 4, N'Estoque inicial', @d20;

EXEC sp_RegistrarMovimentacao 2, 16, 'SAIDA',  6, 4, N'Venda balcão', @d10;
EXEC sp_RegistrarMovimentacao 2, 17, 'SAIDA', 12, 4, N'Venda balcão', @d3;
EXEC sp_RegistrarMovimentacao 2, 18, 'SAIDA',  4, 4, N'Venda balcão', @d3;
EXEC sp_RegistrarMovimentacao 2, 19, 'SAIDA', 22, 4, N'Venda balcão', @d1;
EXEC sp_RegistrarMovimentacao 2, 20, 'SAIDA', 10, 4, N'Venda balcão', @hoje;
GO

/* Estoque esperado depois do seed (confira com: SELECT Id, EstabelecimentoId, Nome, EstoqueAtual, EstoqueMinimo FROM Produto)

   Estabelecimento 1 - Mercadinho Bom Preço
     Id  Produto                     Atual  Mínimo
      1  Arroz                          29      10
      2  Feijão                         12      15   <- abaixo do mínimo
      3  Açúcar                         22      10
      4  Café                            5       8   <- abaixo do mínimo
      5  Óleo                           22      12
      6  Macarrão                       25      10
      7  Refrigerante                   12      12   (igual ao mínimo: NÃO aparece)
      8  Água                           18      24   <- abaixo do mínimo
      9  Suco                           14       6
     10  Leite                          15      24   <- abaixo do mínimo
     11  Queijo                         11       5
     12  Detergente                      3      12   <- abaixo do mínimo
     13  Sabão em pó                    13       6
     14  Papel higiênico                13       6
     15  Sabonete                       38      20

   Estabelecimento 2 - Mercearia Dona Maria
     16  Arroz                          14       8
     17  Feijão                          3       6   <- abaixo do mínimo
     18  Ovos                            8       5
     19  Banana                          8      10   <- abaixo do mínimo
     20  Tomate                         15       8
*/


/* =====================================================================
   7. COMANDOS DE TESTE
   Selecione um bloco e execute (F5) para testar. Bons para os prints
   do relatório.
   ===================================================================== */

/* --- Procedures -----------------------------------------------------

-- Produtos abaixo do mínimo do estabelecimento 1 (deve trazer 5: Detergente, Leite, Água, Café, Feijão)
EXEC sp_ProdutosAbaixoDoMinimo @EstabelecimentoId = 1;

-- ... e do estabelecimento 2 (deve trazer 2: Feijão e Banana) - um não vê o do outro
EXEC sp_ProdutosAbaixoDoMinimo @EstabelecimentoId = 2;

-- Movimentações dos últimos 7 dias do estabelecimento 1
DECLARE @ini DATE = DATEADD(DAY, -7, CAST(SYSDATETIME() AS DATE));
DECLARE @fim DATE = CAST(SYSDATETIME() AS DATE);
EXEC sp_RelatorioMovimentacoes @EstabelecimentoId = 1, @DataInicio = @ini, @DataFim = @fim;

-- Venda que dá certo: vende 2 Arroz (estoque 29 -> 27)
EXEC sp_RegistrarMovimentacao @EstabelecimentoId = 1, @ProdutoId = 1, @Tipo = 'SAIDA', @Quantidade = 2, @UsuarioId = 3, @Observacao = N'Teste de venda';
SELECT Id, Nome, EstoqueAtual, AtualizadoEm FROM Produto WHERE Id = 1;

-- Venda que deve FALHAR: Detergente só tem 3 (erro 50005 "Estoque insuficiente")
EXEC sp_RegistrarMovimentacao @EstabelecimentoId = 1, @ProdutoId = 12, @Tipo = 'SAIDA', @Quantidade = 100, @UsuarioId = 3;

-- Isolamento: o estabelecimento 2 tentando vender um produto do estabelecimento 1 (erro 50001)
EXEC sp_RegistrarMovimentacao @EstabelecimentoId = 2, @ProdutoId = 1, @Tipo = 'SAIDA', @Quantidade = 1, @UsuarioId = 4;

-- Tipo inválido (erro 50003)
EXEC sp_RegistrarMovimentacao @EstabelecimentoId = 1, @ProdutoId = 1, @Tipo = 'TROCA', @Quantidade = 1, @UsuarioId = 1;

------------------------------------------------------------------- */

/* --- Trigger trg_AtualizaEstoque ------------------------------------

-- INSERT direto (sem a procedure) também atualiza o estoque, pela trigger:
SELECT EstoqueAtual FROM Produto WHERE Id = 3;          -- antes
INSERT INTO Movimentacao (ProdutoId, Tipo, Quantidade, UsuarioId, Observacao)
VALUES (3, 'ENTRADA', 5, 1, N'Teste trigger');
SELECT EstoqueAtual FROM Produto WHERE Id = 3;          -- depois (+5)

-- INSERT direto que deixaria o estoque negativo: o CHECK barra e nada é gravado
INSERT INTO Movimentacao (ProdutoId, Tipo, Quantidade, UsuarioId)
VALUES (4, 'SAIDA', 999, 1);

------------------------------------------------------------------- */

/* --- Trigger trg_AuditoriaProduto -----------------------------------

-- Alteração de preço gera log UPDATE_PRECO
UPDATE Produto SET Preco = 30.90, AtualizadoEm = SYSDATETIME() WHERE Id = 1;
SELECT * FROM LogAuditoria ORDER BY Id DESC;

-- Exclusão gera log DELETE (usa um produto novo, sem movimentações,
-- porque produto com movimentação não pode ser apagado por causa da FK)
INSERT INTO Produto (EstabelecimentoId, Nome, CodigoBarras, Preco, EstoqueMinimo, CategoriaId, FornecedorId)
VALUES (1, N'Produto Teste', '7899999999999', 1.00, 0, 1, 1);
DELETE FROM Produto WHERE CodigoBarras = '7899999999999';
SELECT * FROM LogAuditoria ORDER BY Id DESC;

------------------------------------------------------------------- */

/* --- Constraints (devem dar erro) -----------------------------------

INSERT INTO Usuario (EstabelecimentoId, Nome, Email, SenhaHash, Perfil)
VALUES (2, N'Repetido', N'admin@stockcontrol.local', N'x', N'Admin');  -- e-mail duplicado, mesmo em outro estabelecimento (UX_Usuario_Email)

INSERT INTO Usuario (EstabelecimentoId, Nome, Email, SenhaHash, Perfil)
VALUES (1, N'Perfil errado', N'x@x.com', N'x', N'Chefe');              -- perfil inválido (CK_Usuario_Perfil)

INSERT INTO Produto (EstabelecimentoId, Nome, CodigoBarras, Preco, CategoriaId, FornecedorId)
VALUES (1, N'Arroz repetido', '7891000000011', 1, 1, 1);               -- código repetido no MESMO estabelecimento (UX_Produto_Estab_CodigoBarras)

INSERT INTO Fornecedor (EstabelecimentoId, RazaoSocial, CNPJ) VALUES (1, N'Teste', '12.345.678/0001-90');  -- CNPJ com máscara (CK_Fornecedor_CNPJ)

------------------------------------------------------------------- */
