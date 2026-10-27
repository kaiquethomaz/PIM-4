# 7. Testes realizados

Testes manuais executados no sistema em funcionamento (SQL Server 2025 Express + aplicação rodando localmente), com o banco criado pelo script e os dados de teste (seed).

## 7.1 Banco de dados

| # | Teste | Resultado esperado | Resultado |
|---|---|---|---|
| B1 | Executar `script_stockcontrol.sql` no SSMS | Banco criado sem erros; 3 usuários, 5 categorias, 3 fornecedores, 15 produtos | ✅ Passou |
| B2 | `EXEC sp_ProdutosAbaixoDoMinimo` | 5 produtos: Detergente, Leite, Água, Café, Feijão | ✅ Passou |
| B3 | Cadastrar produto com estoque inicial 5 | ENTRADA de 5 registrada pela procedure; trigger deixa `EstoqueAtual = 5` | ✅ Passou |
| B4 | Alterar o preço de 10,50 para 11,90 | Trigger grava `UPDATE_PRECO` (10.50 → 11.90) em `LogAuditoria` | ✅ Passou |
| B5 | Atualização de estoque e desativação do produto | **Nenhum** registro extra em `LogAuditoria` (só mudança de preço é auditada) | ✅ Passou |

## 7.2 Autenticação e permissões

| # | Teste | Resultado esperado | Resultado |
|---|---|---|---|
| A1 | Abrir `/Produtos` sem estar logado | Redireciona para `/Conta/Login?ReturnUrl=/Produtos` | ✅ Passou |
| A2 | Enviar o login com campos vazios | Mensagens "Informe o e-mail." e "Informe a senha." | ✅ Passou |
| A3 | Login do Admin com a senha correta | Entra no sistema (hash BCrypt conferido) | ✅ Passou |
| A4 | Botão "Sair" | Volta para a tela de login | ✅ Passou |
| A5 | Login do Operador | Menu mostra só Dashboard e Produtos, sem botões de editar/excluir | ✅ Passou |
| A6 | Operador digita `/Categorias` no navegador | Página "Acesso negado" | ✅ Passou |

## 7.3 Cadastros

| # | Teste | Resultado esperado | Resultado |
|---|---|---|---|
| C1 | Buscar produto por "caf" | Retorna só "Café Torrado e Moído 500g" | ✅ Passou |
| C2 | Cadastrar produto com preço "10,50" (vírgula) | Salvo como R$ 10,50 | ✅ Passou |
| C3 | Editar produto colocando código de barras de outro produto | Erro "Já existe um produto com este código de barras." | ✅ Passou |
| C4 | Excluir produto que tem movimentações | Produto **desativado** (não excluído), com mensagem explicando | ✅ Passou |
| C5 | Excluir categoria que tem produtos | Erro "Não é possível excluir: existem produtos nesta categoria." | ✅ Passou |
| C6 | Cadastrar fornecedor com CNPJ de dígito verificador errado | Erro "CNPJ inválido." | ✅ Passou |
| C7 | Cadastrar fornecedor com CNPJ válido com máscara (`11.444.777/0001-61`) | Salvo sem máscara no banco e exibido formatado | ✅ Passou |
| C8 | Excluir fornecedor sem produtos | Excluído | ✅ Passou |

## 7.4 API REST

Testes feitos chamando a API diretamente (PowerShell `Invoke-RestMethod`) e pelo Swagger.

| # | Teste | Resultado esperado | Resultado |
|---|---|---|---|
| R1 | `GET /api/produtos` sem token | 401 | ✅ Passou |
| R2 | Login com senha errada | 401 `{"mensagem":"E-mail ou senha inválidos."}` | ✅ Passou |
| R3 | Login do Operador | 200 com token JWT e perfil "Operador" | ✅ Passou |
| R4 | `GET /api/auth/eu` com token | Nome do usuário | ✅ Passou |
| R5 | Token com a assinatura adulterada | 401 | ✅ Passou |
| R6 | `GET /api/produtos` | 15 produtos ativos | ✅ Passou |
| R7 | `GET /api/produtos/codigo-barras/7891000000011` | Arroz, com categoria e estoque | ✅ Passou |
| R8 | Código de barras inexistente | 404 com mensagem | ✅ Passou |
| R9 | Venda de 3 unidades (estoque 27) | 201; resposta com `estoqueAtual` = 24 | ✅ Passou (após correção — ver 7.8) |
| R10 | Entrada de 10 unidades pelo código de barras | 201; `estoqueAtual` = 34 | ✅ Passou |
| R11 | Venda de 100 detergentes (estoque 3) | 400 "Estoque insuficiente..." | ✅ Passou |
| R12 | Tipo "TROCA" | 400 "O tipo deve ser ENTRADA ou SAIDA." | ✅ Passou |
| R13 | Operador tentando cadastrar produto | 403 | ✅ Passou |
| R14 | `GET /api/produtos?atualizadoDesde=` (últimos 5 min) | Só o produto que acabou de ter venda | ✅ Passou |
| R15 | `GET /api/dashboard/resumo` | 15 ativos, 5 abaixo do mínimo, 10 últimas movimentações | ✅ Passou |
| R16 | `GET /api/movimentacoes` com período invertido | 400 "A data inicial não pode ser maior..." | ✅ Passou |
| R17 | Admin cadastra categoria "Pães e Bolos" (com acento) | 201 | ✅ Passou |
| R18 | Categoria com nome repetido | 400 "Já existe uma categoria com este nome." | ✅ Passou |
| R19 | Cadastrar produto com estoque inicial 7 | 201; resposta com `estoqueAtual` = 7 | ✅ Passou (após correção — ver 7.8) |
| R20 | Alterar o preço do produto (PUT) | 200; preço novo e estoque mantido | ✅ Passou |
| R21 | Excluir categoria que tem produto | 400 com mensagem | ✅ Passou |
| R22 | Excluir produto com movimentação | 200; produto desativado | ✅ Passou |
| R23 | Produto com vários campos inválidos | 400 com a lista de erros por campo, em português | ✅ Passou |
| R24 | Fornecedor com CNPJ inválido | 400 "CNPJ inválido." | ✅ Passou |
| R25 | Swagger em `/swagger` | 12 rotas documentadas, agrupadas, com botão Authorize | ✅ Passou |

## 7.5 Movimentações, Dashboard e Usuários

| # | Teste | Resultado esperado | Resultado |
|---|---|---|---|
| M1 | Dashboard do Admin | 15 ativos, 5 abaixo do mínimo (cartão em vermelho), vendas de hoje, valor em estoque | ✅ Passou |
| M2 | Abrir "Registrar" pelo link do Detergente com tipo Venda | Formulário já vem com "Venda" marcado e o produto selecionado, mostrando "estoque: 3" | ✅ Passou |
| M3 | Venda de 100 detergentes (estoque 3) | Erro "Estoque insuficiente..." e o formulário mantém os dados | ✅ Passou |
| M4 | Venda de 2 detergentes | "Venda registrada: 2 x Detergente... Estoque atual: 1." e o formulário volta com "Venda" marcado | ✅ Passou |
| M5 | Listagem de movimentações sem filtro | Últimos 7 dias, com totais de unidades | ✅ Passou |
| M6 | Período com data inicial maior que a final | Erro "A data inicial não pode ser maior que a data final." | ✅ Passou |
| U1 | Cadastrar usuário com senha e confirmação diferentes | Erro "As senhas não conferem."; os campos de senha voltam vazios | ✅ Passou |
| U2 | Cadastrar usuário com e-mail em maiúsculas (`Paula@StockControl.local`) | Salvo em minúsculas | ✅ Passou |
| U3 | Lista de usuários | O próprio Admin aparece como "(você)", sem o botão Excluir | ✅ Passou |
| U4 | Mudar o perfil do único Admin para Operador | Erro "O sistema precisa ter pelo menos um administrador ativo." | ✅ Passou |
| U5 | Excluir usuário sem movimentações | Excluído | ✅ Passou |
| U6 | Excluir usuário com movimentações (Operador) | Desativado, com mensagem explicando | ✅ Passou |
| U7 | Login do usuário desativado | Recusado (401 na API) | ✅ Passou |
| U8 | Reativar o Operador pela edição, com a senha em branco | Reativado; o login com a senha antiga continua funcionando | ✅ Passou |
| U9 | Gerente abrindo `/Usuarios` | "Acesso negado"; o menu do Gerente não tem "Usuários" | ✅ Passou |
| U10 | POST de venda sem o token anti-CSRF | Recusado com HTTP 400 | ✅ Passou |
| U11 | Dashboard em 375 px (celular) | Menu recolhido, cartões 2 por linha, sem rolagem lateral | ✅ Passou (após correção — ver 7.8) |

## 7.6 Interface

| # | Teste | Resultado esperado | Resultado |
|---|---|---|---|
| I1 | Tela de login em 375 px (celular) | Cartão ocupa a largura, campos legíveis, sem rolagem lateral | ✅ Passou |
| I2 | Tela de login em 1024 px | Cartão centralizado | ✅ Passou |

## 7.7 Revisão final (etapa 7)

Sistema rodando do zero com `dotnet run` (HTTPS na porta 7080) e o banco recriado pelo script.

| # | Teste | Resultado esperado | Resultado |
|---|---|---|---|
| F1 | Abrir as 17 páginas do sistema logado como **Admin** | Todas abrem | ✅ 17/17 |
| F2 | Abrir as 17 páginas como **Gerente** | Abrem, exceto as 3 de Usuários ("Acesso negado") | ✅ 17/17 |
| F3 | Abrir as 17 páginas como **Operador** | Abrem só Dashboard, Movimentações e Produtos (consulta); as outras 12 dão "Acesso negado" | ✅ 17/17 |
| F4 | `/Produtos/Editar/999` (id inexistente) | 404 | ✅ Passou |
| F5 | `/Movimentacoes` sem login | Redireciona para o login com `ReturnUrl` | ✅ Passou |
| F6 | `http://localhost:5080/Produtos` | Redireciona (307) para `https://localhost:7080/Produtos` | ✅ Passou |
| F7 | `/swagger` | Abre a documentação | ✅ Passou |

## 7.8 Defeitos encontrados e corrigidos

| Defeito | Causa | Correção |
|---|---|---|
| Depois de uma venda pela API, a resposta mostrava o estoque **antigo** (29 em vez de 27), embora o banco estivesse correto. | O Entity Framework guarda em memória os objetos já lidos na requisição. A trigger `trg_AtualizaEstoque` altera o estoque direto no banco, sem o EF saber, então a segunda leitura devolvia a cópia antiga. | Depois de executar a procedure, o `MovimentacaoDAO` recarrega do banco o produto que estava em memória (`Entry(produto).ReloadAsync()`). Testes R9 e R19 repetidos com sucesso. |
| Mensagens de validação da API em inglês ("One or more validation errors occurred"). | Formato padrão do ASP.NET Core. | Resposta personalizada (`InvalidModelStateResponseFactory`) no formato `{ "mensagem": "Dados inválidos.", "erros": {...} }`. |
| No celular (375 px), o Dashboard ficava com 449 px de largura: o navegador reduzia o zoom e os quadros encostavam na borda. | Em um CSS Grid, cada coluna tem largura mínima igual ao seu conteúdo; a tabela de reposição "empurrava" a coluna para além da tela. | `min-width: 0` nas colunas do grid, para a tabela rolar por dentro do quadro. A página passou a ter exatamente 375 px. |
