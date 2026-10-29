# 7. Testes realizados

Testes manuais executados no sistema em funcionamento (SQL Server 2025 Express + aplicação rodando localmente), com o banco criado pelo script e os dados de teste (seed).

## 7.1 Banco de dados

| # | Teste | Resultado esperado | Resultado |
|---|---|---|---|
| B1 | Executar `script_stockcontrol.sql` no SSMS | Banco criado sem erros; 2 estabelecimentos; o 1º com 3 usuários, 5 categorias, 3 fornecedores e 15 produtos | ✅ Passou |
| B2 | `EXEC sp_ProdutosAbaixoDoMinimo @EstabelecimentoId = 1` | 5 produtos: Detergente, Leite, Água, Café, Feijão | ✅ Passou |
| B2b | `EXEC sp_ProdutosAbaixoDoMinimo @EstabelecimentoId = 2` | 2 produtos: Feijão, Banana (só os do estabelecimento 2) | ✅ Passou |
| B2c | `sp_RegistrarMovimentacao` com o estabelecimento 2 vendendo o produto 1 (do estabelecimento 1) | Erro 50001 "Produto inexistente ou inativo." | ✅ Passou |
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
| R9 | Venda de 3 unidades (estoque 27) | 201; resposta com `estoqueAtual` = 24 | ✅ Passou (após correção — ver 7.13) |
| R10 | Entrada de 10 unidades pelo código de barras | 201; `estoqueAtual` = 34 | ✅ Passou |
| R11 | Venda de 100 detergentes (estoque 3) | 400 "Estoque insuficiente..." | ✅ Passou |
| R12 | Tipo "TROCA" | 400 "O tipo deve ser ENTRADA ou SAIDA." | ✅ Passou |
| R13 | Operador tentando cadastrar produto | 403 | ✅ Passou |
| R14 | `GET /api/produtos?atualizadoDesde=` (últimos 5 min) | Só o produto que acabou de ter venda | ✅ Passou |
| R15 | `GET /api/dashboard/resumo` | 15 ativos, 5 abaixo do mínimo, 10 últimas movimentações | ✅ Passou |
| R16 | `GET /api/movimentacoes` com período invertido | 400 "A data inicial não pode ser maior..." | ✅ Passou |
| R17 | Admin cadastra categoria "Pães e Bolos" (com acento) | 201 | ✅ Passou |
| R18 | Categoria com nome repetido | 400 "Já existe uma categoria com este nome." | ✅ Passou |
| R19 | Cadastrar produto com estoque inicial 7 | 201; resposta com `estoqueAtual` = 7 | ✅ Passou (após correção — ver 7.13) |
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
| U11 | Dashboard em 375 px (celular) | Menu recolhido, cartões 2 por linha, sem rolagem lateral | ✅ Passou (após correção — ver 7.13) |

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

## 7.8 Vários estabelecimentos (isolamento e cadastro)

Feitos com os dois estabelecimentos do seed (Mercadinho Bom Preço e Mercearia Dona Maria) e com um terceiro cadastrado pela tela.

| # | Teste | Resultado esperado | Resultado |
|---|---|---|---|
| E1 | Dashboard do admin do Bom Preço × da Maria | Cabeçalho com o nome de cada estabelecimento; 15 × 5 produtos ativos; 5 × 2 abaixo do mínimo | ✅ Passou |
| E2 | Listas de cada um (produtos, categorias, fornecedores, usuários, movimentações) | Bom Preço: 15 / 5 / 3 / 3 / 12; Maria: 5 / 2 / 1 / 1 / 4 | ✅ Passou |
| E3 | Maria abre `/Produtos/Editar/1`, `/Categorias/Editar/1` e `/Usuarios/Editar/1` (registros do Bom Preço) | 404 nos três | ✅ Passou |
| E4 | Admin do Bom Preço abre `/Produtos/Editar/16` (produto da Maria) | 404 | ✅ Passou |
| E5 | Login da Maria pela API | Token com `estabelecimentoId = 2` e o nome "Mercearia Dona Maria" | ✅ Passou |
| E6 | Maria: `GET /api/produtos/codigo-barras/7891000000011` (código que existe nos dois) | Devolve o Arroz **dela** (Id 16, R$ 27,50) | ✅ Passou |
| E7 | Maria: `GET /api/produtos/1` | 404 | ✅ Passou |
| E8 | Maria: vender pela API o produto 1 (do Bom Preço) | 400 "Produto não encontrado ou inativo." | ✅ Passou |
| E9 | Cadastrar estabelecimento com CNPJ já cadastrado | "Este CNPJ já está cadastrado. Peça ao administrador..." | ✅ Passou |
| E10 | Cadastrar com e-mail já usado | "Este e-mail já está em uso..." | ✅ Passou |
| E11 | Cadastrar com CNPJ de dígito verificador errado | "CNPJ do estabelecimento inválido." | ✅ Passou |
| E12 | Cadastro válido ("Mercado Teste") | Entra direto no Dashboard com mensagem de boas-vindas; 0 produtos, 6 categorias iniciais, 1 usuário; menu com "Estabelecimento" | ✅ Passou |
| E13 | Dono do Mercado Teste cria a funcionária Carla (Operador) | Criada; Carla entra pela API vendo o "Mercado Teste" (0 produtos, 6 categorias) | ✅ Passou |
| E14 | Admin do Bom Preço cria a categoria "Hortifrúti" (nome que existe só na Maria) | Criada | ✅ Passou |
| E15 | Admin do Bom Preço cria "Mercearia" (já existe no Bom Preço) | "Já existe uma categoria com este nome." | ✅ Passou |
| E16 | Dono renomeia o estabelecimento em "Meu estabelecimento" | Cabeçalho mostra o nome novo na hora | ✅ Passou |
| E17 | Gerente abre `/Estabelecimento` | "Acesso negado" | ✅ Passou |

## 7.9 Novo visual (menu lateral, dashboard e telas internas)

| # | Teste | Resultado esperado | Resultado |
|---|---|---|---|
| V1 | Dashboard em 1440 px | Menu lateral agrupado, barra superior com o estabelecimento e a data, 4 indicadores, gráfico de 7 dias com hoje destacado, reposição com barras de nível | ✅ Passou |
| V2 | Vendas de hoje no Dashboard (banco recém-criado) | R$ 368,25 e 31 itens (Detergente 15 × 2,99 + Café 9 × 18,90 + Papel 7 × 21,90) | ✅ Passou |
| V3 | Produtos, Movimentações e Registrar em 1440 px | Cabeçalho com subtítulo, tabelas com categoria embaixo do nome, ações com ícone, opções Venda/Entrada em cartões | ✅ Passou |
| V4 | Dashboard em 375 px | Menu escondido, botão ☰ visível, indicadores 2 por linha, página com exatamente 375 px (sem rolagem lateral) | ✅ Passou |
| V5 | Abrir o menu no celular | Gaveta aparece sobre fundo escuro; `aria-expanded="true"`; foco no botão "Fechar menu" | ✅ Passou |
| V6 | Tecla Esc com o menu aberto | Menu fecha; `aria-expanded="false"`; foco volta para "Abrir menu" | ✅ Passou |
| V7 | 18 páginas × 3 perfis depois da reforma | 54/54 com o comportamento esperado; menu mostra só o que cada perfil acessa (Admin 7 itens, Gerente 5, Operador 3) | ✅ Passou |
| V8 | `GET /api/dashboard/resumo` | Traz `valorVendidoHoje`, `itensVendidosHoje` e 7 dias em `vendasUltimos7Dias` | ✅ Passou |

## 7.10 Melhorias de acabamento (Minha conta, Auditoria, ranking, avisos, máscaras)

| # | Teste | Resultado esperado | Resultado |
|---|---|---|---|
| P1 | Operador abre "Minha conta" pelo próprio nome no menu | Tela com dados e "Trocar senha" | ✅ Passou |
| P2 | Trocar senha com a senha atual errada | "A senha atual está incorreta." | ✅ Passou |
| P3 | Nova senha e confirmação diferentes | "As senhas não conferem."; senha não muda | ✅ Passou |
| P4 | Nova senha igual à atual | "A nova senha deve ser diferente da atual." | ✅ Passou |
| P5 | Troca válida | Aviso "Senha alterada com sucesso."; login com a senha antiga recusado, com a nova aceito | ✅ Passou |
| P6 | **Duplo clique** em "Registrar" (venda de 1 Açúcar) | Só **1** movimentação gravada (estoque 22 → 21) | ✅ Passou |
| P7 | Aviso de sucesso depois de registrar | Aparece no canto e some sozinho em ~6 s | ✅ Passou |
| P8 | Digitar `11444777000161` no CNPJ e `11987654321` no telefone | `11.444.777/0001-61` e `(11) 98765-4321` | ✅ Passou |
| P9 | Digitar `28.9x05` no preço | `28,90` | ✅ Passou |
| P10 | Ranking de mais vendidos no Dashboard e na API | Água (78), Leite (45), Feijão (38), Refrigerante (36), Detergente (33) | ✅ Passou |
| P11 | Admin muda o preço do Arroz de 28,90 para 31,50 e abre Auditoria | Linha "Alteração de preço · Arroz · R$ 28,90 → R$ 31,50" | ✅ Passou |
| P12 | Gerente abre `/Auditoria` | "Acesso negado" | ✅ Passou |
| P13 | Maria (outro estabelecimento) abre Auditoria | Não vê o log do Bom Preço | ✅ Passou |
| P14 | `favicon.svg` | 200, `image/svg+xml`, referenciado nas telas | ✅ Passou |
| P15 | 19 páginas × 3 perfis (com Auditoria e Minha conta) | 57/57 com o comportamento esperado | ✅ Passou |

## 7.11 Modo escuro e janela de confirmação (acessibilidade)

| # | Teste | Resultado esperado | Resultado |
|---|---|---|---|
| D1 | Clicar no botão "Modo escuro" | Tema muda; `aria-pressed="true"`; escolha salva no navegador | ✅ Passou |
| D2 | Navegar para outra tela depois de ativar | Continua no tema escuro, sem "piscar" claro | ✅ Passou |
| D3 | Verificação automática de contraste em **12 telas × 2 temas** (todos os textos visíveis) | Nenhum texto abaixo de 4,5:1 (3:1 para texto grande) | ✅ Passou (após correção — ver 7.13) |
| D4 | Clicar em "Excluir" do Sabonete | Janela modal abre com "Excluir produto?", mensagem explicando a desativação; foco em **Cancelar** | ✅ Passou |
| D5 | Tab dentro da janela | Foco vai para "Sim, excluir" | ✅ Passou |
| D6 | Esc | Janela fecha, nada é excluído, foco volta para "Excluir Sabonete 90g" | ✅ Passou |
| D7 | Enter no link focado reabre; confirmar | POST com token anti-CSRF; produto desativado; aviso "...foi desativado (não excluído)." | ✅ Passou |
| D8 | Página de confirmação antiga (`/Produtos/Excluir/1`) | Continua existindo (usada se o JavaScript não carregar) | ✅ Passou |

## 7.12 Tela de Vendas

| # | Teste | Resultado esperado | Resultado |
|---|---|---|---|
| S1 | Operador abre **Vendas** (últimos 7 dias) | R$ 1.417,17 · 155 itens · 12 vendas, **iguais à consulta direta no banco** | ✅ Passou |
| S2 | Maria (outro estabelecimento) abre Vendas | Só as vendas da Mercearia Dona Maria (R$ 413,08 · 48 itens · 4 vendas) | ✅ Passou |
| S3 | Filtro "só hoje" | Somente as vendas de hoje | ✅ Passou |
| S4 | Período com data inicial maior que a final | "A data inicial não pode ser maior que a data final." | ✅ Passou |
| S5 | Linha de total da tabela | "Total do período · 155 · R$ 1.417,17" | ✅ Passou |
| S6 | Contraste (claro e escuro) e celular (375 px) | Sem textos abaixo do mínimo; sem rolagem lateral | ✅ Passou |
| S7 | Item "Vendas" no menu | Aparece para todos os perfis, logo abaixo do Dashboard | ✅ Passou |

## 7.13 Defeitos encontrados e corrigidos

| Defeito | Causa | Correção |
|---|---|---|
| Depois de uma venda pela API, a resposta mostrava o estoque **antigo** (29 em vez de 27), embora o banco estivesse correto. | O Entity Framework guarda em memória os objetos já lidos na requisição. A trigger `trg_AtualizaEstoque` altera o estoque direto no banco, sem o EF saber, então a segunda leitura devolvia a cópia antiga. | Depois de executar a procedure, o `MovimentacaoDAO` recarrega do banco o produto que estava em memória (`Entry(produto).ReloadAsync()`). Testes R9 e R19 repetidos com sucesso. |
| Mensagens de validação da API em inglês ("One or more validation errors occurred"). | Formato padrão do ASP.NET Core. | Resposta personalizada (`InvalidModelStateResponseFactory`) no formato `{ "mensagem": "Dados inválidos.", "erros": {...} }`. |
| Iniciais brancas no círculo azul (lista de Usuários) com contraste de 3,7:1 no início do degradê. | O degradê começava num azul claro demais (`#3b82f6`). | Degradê mais escuro (`#2563eb` → `#1e40af`): contraste de 5,2:1 a 8,7:1. |
| As barras do ranking "Mais vendidos" apareciam vermelhas (cor da reposição) em vez de azuis. | As duas barras usam a mesma classe base (`.barra-nivel`), e a regra vermelha vinha depois no CSS. | Regra mais específica para o ranking (`.ranking .ranking-barra span`). |
| Na tabela de últimas movimentações do Dashboard, os nomes quebravam em 3 linhas depois que ela passou a dividir a linha com o ranking. | 6 colunas numa coluna estreita. | Tabela com 4 colunas: data e quem registrou foram para a linha de baixo do nome do produto. |
| No computador, apareciam os botões "×" e "☰", que deveriam existir só no celular. | A regra genérica `.botao-icone { display: inline-flex }` vinha depois no CSS e "ganhava" da regra que escondia esses botões. | Regra com dois nomes de classe (`.botao-icone.botao-menu`), que é mais específica e vale em qualquer ordem. |
| Campo **Telefone** (cadastro de estabelecimento e de fornecedor) menor e com borda diferente dos outros campos. | A regra de estilo dos campos de formulário não incluía `input[type="tel"]`. | `type="tel"` incluído na regra; conferido que Telefone e CNPJ ficaram idênticos (48 px de altura, mesma borda e fonte). |
| No celular (375 px), o Dashboard ficava com 449 px de largura: o navegador reduzia o zoom e os quadros encostavam na borda. | Em um CSS Grid, cada coluna tem largura mínima igual ao seu conteúdo; a tabela de reposição "empurrava" a coluna para além da tela. | `min-width: 0` nas colunas do grid, para a tabela rolar por dentro do quadro. A página passou a ter exatamente 375 px. |
