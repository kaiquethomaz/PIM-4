# 4. Front-end

## 4.1 Visão geral

A interface do sistema web é gerada no servidor com **ASP.NET Core MVC e Views Razor** (`.cshtml`): o Controller monta os dados e a View transforma esses dados em HTML. Não foram usados frameworks de CSS ou JavaScript. O visual é feito com:
- **HTML5 semântico**;
- **CSS3 próprio**: `wwwroot/css/site.css` para o sistema e `wwwroot/css/conta.css` para login e cadastro;
- **JavaScript puro**, só onde é preciso: `site.js` abre e fecha o menu no celular, e `conta.js` mostra e oculta a senha.

A paleta de cores (tons de azul) foi herdada do projeto do PIM III, mantendo a identidade visual do grupo. O logo é um ícone de caixa desenhado em **SVG** (`Views/Shared/_Logo.cshtml`). Diferente de um emoji, o SVG fica nítido em qualquer tamanho e aparece igual em todos os navegadores e sistemas operacionais.

**Ícones:** todos os ícones do sistema (menu, indicadores, botões) são SVG embutidos, gerados pela classe `Extensions/Icones.cs` (`@Icones.Svg("produtos")`). Eles não dependem de biblioteca externa nem de internet, herdam a cor do texto e são marcados como decorativos (`aria-hidden`), porque o texto ao lado já informa o significado.

## 4.1.1 Design system

O `site.css` funciona como um pequeno **design system**: as cores, raios e sombras ficam em variáveis CSS (`--azul`, `--texto-suave`, `--borda-campo`, `--sombra`...) e todos os componentes usam essas variáveis. Assim as telas ficam consistentes, e uma mudança de cor é feita num lugar só.

| Componente | Uso |
|---|---|
| **Menu lateral** (`.sidebar`) | Navegação agrupada em "Operação", "Cadastros" e "Administração", com ícones; card do usuário (iniciais, nome, perfil) e botão Sair no rodapé. |
| **Barra superior** (`.topbar`) | Nome do estabelecimento, data de hoje e atalho "Nova venda". |
| **Cabeçalho de página** (`.barra-titulo`) | Título, subtítulo explicando a tela e botões de ação. |
| **Cartão** (`.cartao`) | Superfície branca com borda e sombra leves; as tabelas encostam nas bordas do cartão. |
| **Indicador** (`.kpi`) | Número em destaque com ícone colorido e uma linha de detalhe. |
| **Etiqueta** (`.etiqueta`) | Situação com texto e pontinho colorido (OK, Abaixo do mínimo, Inativo, Entrada, Venda). |
| **Botões** | Primário (azul), secundário (contorno), perigo (vermelho) e "texto" (ações discretas nas tabelas, com ícone). |
| **Estado vazio** (`.vazio`) | Ícone e mensagem quando uma lista não tem registros. |
| **Aviso "toast"** (`.toast`) | Mensagens de sucesso/erro no canto superior direito, com ícone e botão de fechar; somem sozinhas (6 s sucesso, 10 s erro). |

## 4.1.2 Modo escuro

O botão com ícone de lua/sol na barra superior alterna entre o tema claro e o escuro.

- **Como funciona:** o tema escuro só troca os **valores** das variáveis de cor (`:root[data-tema="escuro"]`); os componentes são os mesmos. A escolha fica salva no navegador (`localStorage`). Se o usuário nunca escolheu, o sistema segue a preferência do sistema operacional (`prefers-color-scheme`).
- **Sem "piscar":** um pequeno script no `<head>` aplica o tema antes de a página ser desenhada.
- **Controles nativos:** `color-scheme: dark` deixa o calendário, as listas de seleção e as barras de rolagem do navegador também escuros.
- **Acessibilidade:**
  - o botão tem nome fixo ("Modo escuro") e o estado em `aria-pressed`, então o leitor de tela anuncia "Modo escuro, botão, pressionado";
  - o azul usado como **cor de texto** é diferente do azul dos botões (`--azul-texto`), porque o azul escuro dos botões não teria contraste suficiente como letra sobre fundo escuro;
  - **verificação automática de contraste** em todos os textos de 12 telas, nos dois temas: todas passaram (≥ 4,5:1 em texto normal, ≥ 3:1 em texto grande).
- O login e o cadastro continuam no tema claro, que já tem o painel azul escuro como destaque.

## 4.1.3 Confirmação de exclusão em janela

Ao clicar em **Excluir** (produtos, categorias, fornecedores, usuários), abre uma janela de confirmação em vez de outra página. Ela foi feita com o elemento nativo **`<dialog>`** do HTML, que já resolve a maior parte da acessibilidade:

| Recurso | Como funciona |
|---|---|
| **Janela modal** | `showModal()`: o resto da página fica inerte e escurecido; o foco do teclado não sai da janela. |
| **Foco inicial seguro** | O foco começa em **"Cancelar"**, para que um Enter acidental não exclua nada. |
| **Teclado** | Tab alterna entre os botões; **Esc** fecha a janela (cancela). |
| **Foco de volta** | Ao fechar, o foco volta para o link "Excluir" que abriu a janela. |
| **Leitor de tela** | `aria-labelledby` (título "Excluir produto?") e `aria-describedby` (mensagem explicando o que vai acontecer). |
| **Segurança** | A janela envia um POST com o token anti-CSRF para o mesmo endereço da página de confirmação. |
| **Sem JavaScript** | O link "Excluir" continua apontando para a página de confirmação antiga, que funciona normalmente se o script não carregar (*melhoria progressiva*). |

## 4.1.4 Comportamentos dos formulários (`wwwroot/js/formularios.js`)

| Recurso | Como funciona | Por quê |
|---|---|---|
| **Proteção contra duplo clique** | Ao enviar, o botão fica desabilitado, mostra um *spinner* e um texto ("Registrando…", "Salvando…", "Excluindo…") e recebe `aria-busy="true"`. Um segundo clique é ignorado. | Sem isso, dois cliques rápidos em "Registrar" gravariam a **mesma venda duas vezes**. Testado: duplo clique gerou só 1 movimentação. |
| **Máscaras** | CNPJ vira `11.222.333/0001-81` e telefone vira `(11) 98765-4321` enquanto a pessoa digita; o preço aceita só números e uma vírgula, com até 2 casas (ponto vira vírgula). | Evita erro de digitação; o servidor continua validando tudo. |
| **Ícone da aba** | `wwwroot/favicon.svg`: o logo do sistema aparece na aba do navegador e nos favoritos. | Identidade visual. |

## 4.2 Organização das Views

| Pasta / arquivo | Função |
|---|---|
| `Views/Shared/_Layout.cshtml` | Modelo comum das telas internas: menu lateral, barra superior, área de mensagens e conteúdo. |
| `Views/Shared/_LayoutLogin.cshtml` | Modelo da tela de login (sem menu). |
| `Views/Shared/_Mensagens.cshtml` | *Partial view* que exibe as mensagens de sucesso e erro. |
| `Views/Conta/` | Login e acesso negado. |
| `Views/Produtos/`, `Views/Categorias/`, `Views/Fornecedores/` | Listagem (`Index`), formulário de cadastro/edição (`Form`) e confirmação de exclusão (`Excluir`). |
| `Views/Home/` | Página inicial (dashboard). |

**Reaproveitamento:** o mesmo arquivo `Form.cshtml` serve para criar e para editar. A View verifica se o registro é novo (`Model.EhNovo`) e muda o título e o destino do formulário.

**Tag Helpers:** os formulários usam os *Tag Helpers* do ASP.NET Core (`asp-for`, `asp-action`, `asp-validation-for`). Eles geram automaticamente os nomes dos campos, os textos dos `<label>`, as mensagens de validação e o **token anti-CSRF** de cada formulário.

## 4.3 Telas

| Tela | O que faz | Perfil |
|---|---|---|
| **Login** | Tela dividida: à esquerda, um painel com a identidade visual e os principais recursos do sistema; à direita, o formulário com ícones nos campos, botão para mostrar/ocultar a senha e a opção "Manter conectado". No celular, o painel é ocultado e o logo aparece acima do formulário. | Todos |
| **Cadastrar meu estabelecimento** | Mesmo layout do login, com o formulário em dois grupos numerados: "1. Dados do estabelecimento" (nome, CNPJ, telefone) e "2. Seus dados de acesso" (nome, e-mail e senha com confirmação). Depois de cadastrar, o dono já entra no sistema. | Público |
| **Meu estabelecimento** | CNPJ e data de cadastro (só leitura) + edição do nome, telefone e e-mail. O nome alterado aparece na hora no cabeçalho. | Somente Admin |
| **Dashboard** | 4 indicadores (**vendas de hoje em R$** e itens vendidos, vendas dos últimos 7 dias com média diária, produtos abaixo do mínimo, valor em estoque); **gráfico de barras** das vendas dos últimos 7 dias (SVG, com o dia de hoje destacado); lista "Precisam de reposição" com **barra de nível de estoque** e botão **Repor**; últimas 10 movimentações com valor, data e quem registrou; **ranking dos 5 mais vendidos** dos últimos 30 dias, com barras proporcionais; atalhos "Registrar venda" / "Registrar entrada". | Todos (valor em estoque só Admin/Gerente; o Operador vê "Produtos ativos") |
| **Registrar movimentação** | Formulário único para venda ou entrada: tipo (botões de opção), produto (lista com o estoque atual de cada um), quantidade e observação. Depois de salvar, mostra o estoque atualizado e volta pronto para a próxima venda. | Todos |
| **Movimentações** | Indicadores do período (movimentações, unidades de entrada, unidades vendidas) e listagem filtrada por período (padrão: últimos 7 dias). | Todos |
| **Produtos** | Lista com busca por nome ou código de barras, etiqueta de situação (OK / Abaixo do mínimo / Inativo), cadastro, edição e exclusão. | Consulta: todos; edição: Admin e Gerente |
| **Categorias** | Lista, cadastro, edição e exclusão. | Admin e Gerente |
| **Fornecedores** | Lista com CNPJ formatado, cadastro, edição e exclusão. | Admin e Gerente |
| **Acesso negado** | Mostrada quando o usuário tenta abrir uma página sem permissão. | Todos |
| **Página não encontrada** | Mostrada em endereços inexistentes ou ao tentar abrir um registro de outro estabelecimento (erro 404), no visual do sistema em vez da página padrão do navegador. | Todos |
| **Minha conta** | Aberta ao clicar no próprio nome, no rodapé do menu: mostra nome, e-mail, perfil e estabelecimento, e permite **trocar a própria senha** (pede a senha atual). | Todos |
| **Auditoria** | Histórico de mudanças de preço (antes → depois) e exclusões de produtos, gravado pela trigger `trg_AuditoriaProduto`, com um aviso explicando de onde vêm os dados. | Somente Admin |
| **Usuários** | Lista com perfil e situação, cadastro (com confirmação de senha), edição (senha em branco mantém a atual; ativar/desativar) e exclusão. | Somente Admin |

**Estrutura das telas internas:** menu lateral fixo à esquerda e, no topo, uma barra com o **nome do estabelecimento**, a data e o atalho "Nova venda", para o funcionário sempre saber em qual mercadinho está. O usuário logado (iniciais, nome e perfil) e o botão Sair ficam no rodapé do menu.

**Adaptação ao perfil:** o menu e os botões mudam conforme o usuário logado. O Operador, por exemplo, não vê os itens "Categorias" e "Fornecedores" nem os botões "Editar" e "Excluir" de produtos. Mesmo que digite o endereço da página, ele é redirecionado para "Acesso negado", porque a proteção também existe no servidor.

**Fluxo das telas de cadastro:**
1. A lista mostra os registros e o botão "+ Novo".
2. O formulário valida os dados no servidor. Se houver erro, a tela volta com os dados preenchidos e a mensagem ao lado do campo.
3. Depois de salvar, o usuário volta para a lista com a mensagem "Produto X cadastrado".
4. A exclusão sempre passa por uma **página de confirmação** antes de apagar.

## 4.4 Responsividade

O layout se adapta a celulares, tablets e computadores:
- **Computador:** menu lateral fixo à esquerda e conteúdo com largura máxima de 1280 px.
- **Tablet e celular (abaixo de 900 px):** o menu lateral vira uma **gaveta**. Ela abre pelo botão ☰ da barra superior, por cima de um fundo escurecido, e fecha pelo botão ×, pelo clique no fundo ou pela tecla Esc.
- **Celular (abaixo de 600 px):**
  - formulários em duas colunas passam para uma coluna;
  - os botões do cabeçalho ocupam a largura toda;
  - abaixo de 420 px, o atalho "Nova venda" vira só o ícone +.
- **Dashboard:**
  - os 4 indicadores ficam lado a lado no computador e 2 por linha em telas menores;
  - o gráfico e a lista de reposição ficam lado a lado e passam para um embaixo do outro abaixo de 1180 px.
- **Tabelas largas** rolam na horizontal dentro do cartão, sem quebrar a página. Isso foi conferido: em 375 px, a página tem exatamente 375 px de largura.
- **Toque:** os botões têm no mínimo **42 a 44 px de altura** (40 px nos botões pequenos do celular), tamanho confortável para o dedo.
- Os campos usam o teclado adequado no celular: `inputmode="decimal"` no preço, `inputmode="numeric"` no código de barras e no CNPJ, `type="email"` e `type="tel"`.
- A tag `<meta name="viewport">` faz o navegador do celular usar a largura real da tela.

## 4.5 Acessibilidade

O sistema segue recomendações da **WCAG 2.1** (Diretrizes de Acessibilidade para Conteúdo Web) para atender pessoas com deficiência visual, motora ou que usam leitores de tela:

| Recurso | Como foi feito | Quem é beneficiado |
|---|---|---|
| **Idioma da página** | `<html lang="pt-BR">` | Leitores de tela pronunciam corretamente em português. |
| **Link "Pular para o conteúdo"** | Primeiro item da página; aparece ao apertar Tab. | Quem navega pelo teclado ou leitor de tela não precisa passar por todo o menu. |
| **Navegação por teclado** | Todos os controles são links, botões ou campos nativos; contorno de foco amarelo de 3 px bem visível (`:focus-visible`). | Pessoas que não usam mouse. |
| **Rótulos nos campos** | Todo campo tem `<label>` ligado a ele; dicas ligadas por `aria-describedby`. | Leitores de tela anunciam o nome e a dica do campo. |
| **Campos obrigatórios** | Asterisco visual + `required` / `aria-required="true"`. | Todos. |
| **Mostrar/ocultar senha** | Botão com `aria-label` ("Mostrar senha" / "Ocultar senha") e `aria-pressed`; os ícones são decorativos (`aria-hidden`). | Quem tem dificuldade para digitar confere a senha antes de enviar; o leitor de tela informa se a senha está visível. |
| **Ícones decorativos** | Todos os ícones SVG têm `aria-hidden="true"`; o significado está sempre no texto ao lado. | O leitor de tela não lê "imagem" sem sentido. |
| **Grupos de opções** | Entrada/Venda em `<fieldset>` com `<legend>`, e cada opção com o rótulo clicável inteiro. | O leitor de tela anuncia a pergunta antes das opções; a área de clique é maior. |
| **Regiões do Dashboard** | `<section aria-labelledby>` em cada bloco. | O leitor de tela permite pular direto para "Precisam de reposição". |
| **Mensagens anunciadas** | Erros com `role="alert"`, sucesso com `role="status"` (também nos avisos "toast"). | O leitor de tela lê a mensagem sem o usuário procurar. |
| **Avisos sem pressa** | O "toast" **pausa** enquanto o mouse está em cima ou o foco do teclado está nele, e tem botão "Fechar aviso". | Quem lê mais devagar não perde a mensagem. |
| **Ação em andamento** | Botão enviando recebe `aria-busy="true"` e texto "Salvando…". | O leitor de tela informa que a ação está sendo processada. |
| **Modo escuro** | Botão com `aria-pressed`; contraste verificado em todas as telas também no tema escuro; segue a preferência do sistema operacional. | Pessoas com sensibilidade à luz, enxaqueca ou baixa visão que leem melhor em fundo escuro. |
| **Janela de confirmação** | `<dialog>` modal: foco preso na janela, foco inicial em "Cancelar", Esc fecha, foco volta ao botão de origem. | Quem usa teclado ou leitor de tela não se perde e não exclui nada por acidente. |
| **Botões com contexto** | "Editar" tem `aria-label="Editar Arroz Branco Tipo 1 5kg"`. | O leitor de tela diz *qual* item será editado. |
| **Menu atual** | `aria-current="page"` no item da página aberta. | Orientação dentro do sistema. |
| **Menu do celular (gaveta)** | Botão com `aria-expanded` e `aria-controls`; ao abrir, o foco vai para o botão "Fechar menu"; Esc fecha e o foco volta para o botão do menu. | Quem usa teclado ou leitor de tela não fica "perdido" com o menu aberto. |
| **Grupos do menu** | Cada grupo ("Operação", "Cadastros", "Administração") tem título ligado à lista por `aria-labelledby`. | O leitor de tela anuncia a seção do menu. |
| **Gráfico acessível** | O gráfico SVG tem `role="img"` com título e descrição, e uma **tabela escondida** (só para leitores de tela) com o valor de cada dia. | Pessoas cegas recebem os mesmos dados do gráfico. |
| **Contraste** | Texto com contraste mínimo de **4,5:1** (ex.: azul `#1d4ed8` sobre branco = 6,7:1) e bordas de campos com **3:1** (`#8492a6`). | Pessoas com baixa visão ou daltonismo. |
| **Informação não depende só da cor** | A situação do produto usa texto + cor ("Abaixo do mínimo"); a barra de nível de estoque tem o mesmo dado em texto ao lado ("3 de 12 · faltam 9"). | Pessoas daltônicas. |
| **Tabelas semânticas** | `<caption>`, `<th scope="col">`. | Leitores de tela leem os cabeçalhos junto com cada célula. |
| **Tamanho de fonte** | Fonte em `rem`, respeitando o tamanho escolhido no navegador. | Pessoas com baixa visão que aumentam a fonte. |
| **Menos animação** | `prefers-reduced-motion` desliga animações. | Pessoas sensíveis a movimento. |
| **HTML semântico** | `<header>`, `<nav aria-label>`, `<main>`, `role="search"`. | Leitores de tela permitem pular entre regiões. |

## 4.6 Formatação brasileira

A aplicação usa a cultura **pt-BR** (`UseRequestLocalization`):
- Valores em reais: **R$ 28,90**.
- Datas: **27/10/2026 10:17**.
- O usuário digita o preço com vírgula ("28,90"), como está acostumado.
- As mensagens de validação estão todas em português, inclusive as de conversão automática (ex.: texto digitado no campo de preço).
