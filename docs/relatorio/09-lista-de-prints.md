# 9. Lista de prints para o relatório

Checklist dos prints, organizada pela seção do relatório onde cada um entra. **Siga a ordem**: os primeiros só consultam dados, e os do final alteram o banco.

## Antes de começar

1. **Recrie o banco:** abra `database/script_stockcontrol.sql` no SSMS e aperte F5. Os números citados abaixo (15 produtos, 5 abaixo do mínimo...) valem para o banco recém-criado.
2. **Rode o sistema:** `cd StockControl` e `dotnet run`, depois abra https://localhost:7080.
3. **Navegador:** zoom em 100% (Ctrl+0), janela maximizada, sem outras abas abertas no print.
4. **Ferramenta de captura:** `Windows + Shift + S` (recorte) → cole no Word ou salve.
5. **Nome dos arquivos:** use o número da lista (ex.: `07-sp-abaixo-minimo.png`), para facilitar montar o relatório.
6. **Modo celular:** no Chrome/Edge, `F12` → ícone de celular (`Ctrl+Shift+M`) → escolha "iPhone SE" ou "Galaxy S20".

---

## A. Banco de dados — SSMS (Etapa 7)

| ✔ | # | O que capturar | Como fazer | O que deve aparecer | Legenda sugerida |
|---|---|---|---|---|---|
| ☐ | 01 | Execução do script | Logo depois do F5, aba **Mensagens** | "Consulta executada com êxito", sem vermelho | Execução do script de criação do banco |
| ☐ | 02 | Estrutura do banco | Pesquisador de Objetos: expanda **StockControl → Tabelas** e **Programação → Procedimentos Armazenados** | 6 tabelas e 3 procedures | Objetos do banco StockControl |
| ☐ | 03 | Triggers | Expanda **Tabelas → dbo.Movimentacao → Gatilhos** e **dbo.Produto → Gatilhos** | `trg_AtualizaEstoque` e `trg_AuditoriaProduto` | Triggers do banco |
| ☐ | 04 | Diagrama do MER | Cole o código de `docs/mer.md` em https://mermaid.live → *Actions* → PNG | Diagrama com as 6 entidades | Modelo Entidade-Relacionamento |
| ☐ | 05 | Diagrama físico (opcional) | SSMS: **Diagramas de Banco de Dados → Novo Diagrama**, adicione as 6 tabelas | Tabelas com as linhas de FK | Diagrama físico gerado pelo SQL Server |
| ☐ | 06 | Dados de teste | Nova Consulta: `SELECT * FROM Produto` | 15 produtos | Produtos cadastrados pelo seed |
| ☐ | 07 | Procedure do dashboard | `EXEC sp_ProdutosAbaixoDoMinimo @EstabelecimentoId = 1;` e depois `= 2;` (selecione as duas linhas e F5) | Duas grades: 5 produtos no estabelecimento 1 e 2 no estabelecimento 2 | Procedure de produtos abaixo do mínimo, separada por estabelecimento |
| ☐ | 08 | Procedure de relatório | `EXEC sp_RelatorioMovimentacoes @EstabelecimentoId = 1, @DataInicio = '2026-10-01', @DataFim = '2026-11-30';` (ajuste as datas para o mês atual) | Movimentações com produto e usuário | Relatório de movimentações por período |
| ☐ | 09 | Senhas com hash | `SELECT Nome, Email, SenhaHash, Perfil FROM Usuario` | Coluna SenhaHash começando com `$2a$11$` | Senhas armazenadas com hash BCrypt |
| ☐ | 10 | Índices | Expanda **dbo.Produto → Índices** | `UX_Produto_Estab_CodigoBarras`, `IX_Produto_CategoriaId`... | Índices para otimização |
| ☐ | 10b | Mesmo código em 2 estabelecimentos | `SELECT Id, EstabelecimentoId, Nome, CodigoBarras, Preco FROM Produto WHERE CodigoBarras = '7891000000011';` | 2 linhas: Arroz do estabelecimento 1 (R$ 28,90) e do 2 (R$ 27,50) | Código de barras único por estabelecimento |

## B. Sistema web (Etapa 4)

Entre como **admin@stockcontrol.local**.

| ✔ | # | Tela | Como fazer | O que deve aparecer | Legenda sugerida |
|---|---|---|---|---|---|
| ☐ | 11 | Login | https://localhost:7080 (sem estar logado), janela maximizada | Tela dividida: painel azul com os recursos à esquerda e formulário à direita, com "Cadastrar meu estabelecimento" embaixo | Tela de login |
| ☐ | 11b | Cadastro de estabelecimento | No login, clique em **Cadastrar meu estabelecimento** | Formulário com os grupos "1. Dados do estabelecimento" e "2. Seus dados de acesso" | Cadastro de um novo mercadinho |
| ☐ | 12 | Validação do login | Digite o e-mail do admin e uma senha errada, clique em "Entrar" | Aviso "E-mail ou senha inválidos." acima dos campos | Validação do login |
| ☐ | 13 | Dashboard | Depois do login | Cartões (15 ativos, **5 abaixo do mínimo** em vermelho), tabela de reposição, últimas movimentações | Dashboard do sistema |
| ☐ | 14 | Produtos | Menu **Produtos** | Lista com etiquetas OK / ⚠ Abaixo do mínimo | Listagem de produtos |
| ☐ | 15 | Busca | Digite "café" e clique em Buscar | Só o Café | Busca de produtos |
| ☐ | 16 | Cadastro de produto | **+ Novo produto** | Formulário com categoria, fornecedor e estoque inicial | Cadastro de produto |
| ☐ | 17 | Regra: código repetido | No formulário, use o código `7891000000011` e salve | "Já existe um produto com este código de barras." | Validação de código de barras único |
| ☐ | 18 | Categorias | Menu **Categorias** | Lista das 5 categorias | Cadastro de categorias |
| ☐ | 19 | Fornecedores | Menu **Fornecedores** | 3 fornecedores com CNPJ formatado | Cadastro de fornecedores |
| ☐ | 20 | Regra: CNPJ inválido | **+ Novo fornecedor**, CNPJ `11.222.333/0001-82` | "CNPJ inválido." | Validação de CNPJ |
| ☐ | 21 | Registrar venda | Dashboard → **+ Registrar venda** → escolha o Detergente, quantidade 100 | "Estoque insuficiente. "Detergente..." tem 3 unidade(s)." | Bloqueio de venda sem estoque |
| ☐ | 22 | Venda registrada | Mude a quantidade para 2 e registre | "Venda registrada: 2 x Detergente... Estoque atual: 1." | Registro de venda |
| ☐ | 23 | Movimentações | Menu **Movimentações** | Lista dos últimos 7 dias com totais | Movimentações por período |
| ☐ | 24 | Usuários | Menu **Usuários** | 3 usuários do Mercadinho Bom Preço, o admin marcado como "(você)" | Gerenciamento dos funcionários (estrutura administrativa) |
| ☐ | 24b | Meu estabelecimento | Menu **Estabelecimento** | CNPJ e "Cliente desde" + nome, telefone e e-mail editáveis | Dados do estabelecimento |
| ☐ | 25 | Cadastro de usuário | **+ Novo usuário** | Formulário com perfil e confirmação de senha | Cadastro de usuário |
| ☐ | 26 | Proteção do último Admin | Editar o Administrador → Perfil "Operador" → Salvar | "O sistema precisa ter pelo menos um administrador ativo." | Proteção da estrutura administrativa |
| ☐ | 27 | Menu do Operador | Saia e entre como **operador@stockcontrol.local** | Menu só com Dashboard, Movimentações e Produtos; sem botões Editar/Excluir | Permissões do perfil Operador |
| ☐ | 28 | Acesso negado | Como Operador, digite https://localhost:7080/Usuarios | Página "Acesso negado" | Controle de acesso por perfil |
| ☐ | 28b | Outro estabelecimento | Saia e entre como **maria@donamaria.local** / Maria@123 → Dashboard | Cabeçalho "Mercearia Dona Maria", 5 produtos ativos, 2 abaixo do mínimo | Cada estabelecimento vê só os próprios dados |
| ☐ | 28c | Isolamento | Ainda como Maria, digite https://localhost:7080/Produtos/Editar/1 (produto do outro mercadinho) | Página "Página não encontrada — o registro não pertence ao seu estabelecimento" | Isolamento dos dados entre estabelecimentos |

## C. Responsividade e acessibilidade

| ✔ | # | O que capturar | Como fazer | O que deve aparecer | Legenda sugerida |
|---|---|---|---|---|---|
| ☐ | 29 | Dashboard no celular | F12 → modo celular → Dashboard | Botão "Menu", cartões 2 por linha | Layout responsivo (celular) |
| ☐ | 30 | Menu aberto no celular | Clique em "Menu" | Itens do menu em lista | Menu no celular |
| ☐ | 31 | Formulário no celular | Registrar venda no modo celular | Campos ocupando a largura toda | Formulário responsivo |
| ☐ | 32 | Navegação por teclado | Em qualquer tela, recarregue e aperte **Tab** uma vez | Link "Pular para o conteúdo" visível no topo | Atalho de acessibilidade |
| ☐ | 33 | Foco visível | Continue apertando Tab até um botão | Contorno amarelo no item focado | Indicador de foco para navegação por teclado |
| ☐ | 34 | Lighthouse (opcional) | Chrome: F12 → aba **Lighthouse** → marque só "Acessibilidade" → *Analyze* | Nota de acessibilidade | Avaliação automática de acessibilidade |

## D. API REST e Swagger

Abra https://localhost:7080/swagger.

| ✔ | # | O que capturar | Como fazer | O que deve aparecer | Legenda sugerida |
|---|---|---|---|---|---|
| ☐ | 35 | Swagger | Página inicial do Swagger | Endpoints agrupados (Auth, Categorias, Dashboard, Fornecedores, Movimentacoes, Produtos) | Documentação da API com Swagger |
| ☐ | 36 | Login na API | **POST /api/auth/login** → *Try it out* → e-mail/senha do operador → *Execute* | Resposta 200 com `token` e `usuario` | Autenticação da API com JWT |
| ☐ | 37 | Sem token | Antes de autorizar, execute **GET /api/produtos** | Resposta **401** | Endpoint protegido sem token |
| ☐ | 38 | Authorize | Botão **Authorize** → cole o token | Janela com o token preenchido | Envio do token JWT |
| ☐ | 39 | Busca por código de barras | **GET /api/produtos/codigo-barras/{codigo}** com `7891000000011` | JSON do Arroz | Consulta por código de barras (app mobile) |
| ☐ | 40 | Venda pela API | **POST /api/movimentacoes** com `{ "codigoBarras": "7891000000011", "tipo": "SAIDA", "quantidade": 2 }` | 201 com o `estoqueAtual` já reduzido | Registro de venda pelo app |
| ☐ | 41 | Erro de regra na API | Mesmo endpoint com `"produtoId": 12, "quantidade": 100` | 400 `"Estoque insuficiente..."` | Regra de negócio aplicada também na API |
| ☐ | 42 | Permissão na API | Logado como operador, **POST /api/produtos** | **403** | Autorização por perfil na API |
| ☐ | 43 | Dashboard da API | **GET /api/dashboard/resumo** | JSON com totais e produtos abaixo do mínimo | Resumo para o app |

## E. Código-fonte (arquitetura e POO)

Abra a pasta `StockControl` no Visual Studio ou VS Code.

| ✔ | # | O que capturar | Arquivo | Legenda sugerida |
|---|---|---|---|---|
| ☐ | 44 | Estrutura de pastas | Explorador de arquivos do editor, com as pastas expandidas | Organização em camadas |
| ☐ | 45 | Classe base | `Models/EntidadeBase.cs` | Herança e encapsulamento: classe base abstrata |
| ☐ | 46 | Encapsulamento do estoque | `Models/Produto.cs` (`EstoqueAtual { get; private set; }`) | Encapsulamento do estoque |
| ☐ | 46b | Filtro automático por estabelecimento | `Data/AppDbContext.cs` (os `HasQueryFilter` e o `SaveChangesAsync`) | Isolamento dos dados com Global Query Filter |
| ☐ | 47 | Fluent API + trigger | `Data/Configurations/ProdutoConfiguration.cs` | Mapeamento com Fluent API e `HasTrigger` |
| ☐ | 48 | Padrão DAO | `DAO/IProdutoDAO.cs` + `DAO/ProdutoDAO.cs` | Padrão DAO: interface e implementação |
| ☐ | 49 | Chamada da procedure | `DAO/MovimentacaoDAO.cs` (`EXEC sp_RegistrarMovimentacao`) | Chamada da procedure pelo Entity Framework |
| ☐ | 50 | Regra de negócio | `Services/MovimentacaoService.cs` (verificação de estoque) | Regra de negócio na camada de serviço |
| ☐ | 51 | Injeção de dependência | `Program.cs`, seção "6. Injeção de dependência" | Registro de DAOs e Services |
| ☐ | 52 | Autenticação | `Program.cs`, seção "3. Autenticação" (cookie + JWT) | Configuração de cookie e JWT |

## F. Triggers em ação (por último — alteram dados)

Faça no SSMS, nesta ordem. Os comandos estão prontos no fim do script (seção 7, comentados): selecione o bloco e aperte F5.

| ✔ | # | O que capturar | Comando | O que deve aparecer | Legenda sugerida |
|---|---|---|---|---|---|
| ☐ | 53 | Trigger de estoque | `SELECT EstoqueAtual FROM Produto WHERE Id = 3;` → `INSERT INTO Movimentacao (...) VALUES (3, 'ENTRADA', 5, 1, ...)` → o SELECT de novo | Estoque aumenta em 5 | Trigger `trg_AtualizaEstoque` atualizando o estoque |
| ☐ | 54 | Proteção do estoque | `INSERT INTO Movimentacao (ProdutoId, Tipo, Quantidade, UsuarioId) VALUES (4, 'SAIDA', 999, 1);` | Erro de CHECK em vermelho; nada é gravado | Estoque nunca fica negativo |
| ☐ | 55 | Trigger de auditoria | `UPDATE Produto SET Preco = 30.90 ... WHERE Id = 1;` → `SELECT * FROM LogAuditoria` | Linha `UPDATE_PRECO` com 28.90 → 30.90 | Trigger de auditoria de preço |
| ☐ | 56 | Procedure com erro | `EXEC sp_RegistrarMovimentacao @EstabelecimentoId = 1, @ProdutoId = 12, @Tipo = 'SAIDA', @Quantidade = 100, @UsuarioId = 3;` | Mensagem 50005 "Estoque insuficiente para esta saída." | Validação dentro da procedure |
| ☐ | 56b | Isolamento na procedure | `EXEC sp_RegistrarMovimentacao @EstabelecimentoId = 2, @ProdutoId = 1, @Tipo = 'SAIDA', @Quantidade = 1, @UsuarioId = 4;` | Mensagem 50001 "Produto inexistente ou inativo." | Um estabelecimento não movimenta produto de outro |
| ☐ | 57 | Restrição única | `INSERT INTO Usuario ... 'admin@stockcontrol.local' ...` (bloco "Constraints" do script) | Erro de chave duplicada `UX_Usuario_Email` | Restrição de e-mail único |

## Depois dos prints

Rode o script de novo no SSMS (F5) para voltar o banco ao estado inicial.
