# 4. Front-end

## 4.1 Visão geral

A interface do sistema web é gerada no servidor com **ASP.NET Core MVC e Views Razor** (`.cshtml`): o Controller monta os dados e a View transforma esses dados em HTML. Não foram usados frameworks de CSS ou JavaScript. O visual é feito com **HTML5 semântico, CSS3 próprio** (`wwwroot/css/site.css`) e um pequeno script em **JavaScript puro** (`wwwroot/js/site.js`), só para abrir e fechar o menu no celular.

A paleta de cores (tons de azul) foi herdada do projeto do PIM III, mantendo a identidade visual do grupo. O logo é um ícone de caixa desenhado em **SVG** (`Views/Shared/_Logo.cshtml`), reutilizado no cabeçalho e no login. Diferente de um emoji, o SVG fica nítido em qualquer tamanho e aparece igual em todos os navegadores e sistemas operacionais. Os demais ícones (e-mail, cadeado, olho, gráfico, celular, escudo) também são SVG embutidos no HTML, sem depender de bibliotecas externas.

## 4.2 Organização das Views

| Pasta / arquivo | Função |
|---|---|
| `Views/Shared/_Layout.cshtml` | Modelo comum a todas as páginas: cabeçalho, menu, área de mensagens e conteúdo. |
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
| **Dashboard** | Cartões com os números do dia (produtos ativos, abaixo do mínimo, vendas e entradas de hoje, valor em estoque), tabela "Precisam de reposição" com botão **Repor** em cada produto, últimas 10 movimentações e atalhos "Registrar venda" / "Registrar entrada". | Todos (valor em estoque só Admin/Gerente) |
| **Registrar movimentação** | Formulário único para venda ou entrada: tipo (botões de opção), produto (lista com o estoque atual de cada um), quantidade e observação. Depois de salvar, mostra o estoque atualizado e volta pronto para a próxima venda. | Todos |
| **Movimentações** | Listagem por período (padrão: últimos 7 dias) com totais de unidades vendidas e de entrada. | Todos |
| **Produtos** | Lista com busca por nome ou código de barras, etiqueta de situação (OK / Abaixo do mínimo / Inativo), cadastro, edição e exclusão. | Consulta: todos; edição: Admin e Gerente |
| **Categorias** | Lista, cadastro, edição e exclusão. | Admin e Gerente |
| **Fornecedores** | Lista com CNPJ formatado, cadastro, edição e exclusão. | Admin e Gerente |
| **Acesso negado** | Mostrada quando o usuário tenta abrir uma página sem permissão. | Todos |
| **Usuários** | Lista com perfil e situação, cadastro (com confirmação de senha), edição (senha em branco mantém a atual; ativar/desativar) e exclusão. | Somente Admin |

**Adaptação ao perfil:** o menu e os botões mudam conforme o usuário logado. O Operador, por exemplo, não vê os itens "Categorias" e "Fornecedores" nem os botões "Editar" e "Excluir" de produtos. Mesmo que digite o endereço da página, ele é redirecionado para "Acesso negado", porque a proteção também existe no servidor.

**Fluxo das telas de cadastro:**
1. A lista mostra os registros e o botão "+ Novo".
2. O formulário valida os dados no servidor. Se houver erro, a tela volta com os dados preenchidos e a mensagem ao lado do campo.
3. Depois de salvar, o usuário volta para a lista com a mensagem "Produto X cadastrado".
4. A exclusão sempre passa por uma **página de confirmação** antes de apagar.

## 4.4 Responsividade

O layout se adapta a celulares, tablets e computadores:
- Página com largura máxima de 1200 px, centralizada; nas telas pequenas ocupa a largura toda.
- **Abaixo de 768 px** (celular), o menu vira um botão **"Menu"** que abre e fecha as opções, e os formulários em duas colunas passam para uma coluna só.
- Tabelas largas ficam dentro de um contêiner com rolagem horizontal, para não quebrar a página.
- No Dashboard, os cartões de números usam CSS Grid com `auto-fit`: são 6 por linha no computador e 2 por linha no celular. Os dois quadros de baixo ficam lado a lado no computador e um embaixo do outro abaixo de 960 px.
- Todos os botões têm no mínimo **44 px de altura**, tamanho recomendado para toque com o dedo.
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
| **Mensagens anunciadas** | Erros com `role="alert"`, sucesso com `role="status"`. | O leitor de tela lê a mensagem sem o usuário procurar. |
| **Botões com contexto** | "Editar" tem `aria-label="Editar Arroz Branco Tipo 1 5kg"`. | O leitor de tela diz *qual* item será editado. |
| **Menu atual** | `aria-current="page"` no item da página aberta. | Orientação dentro do sistema. |
| **Menu do celular** | Botão com `aria-expanded` e `aria-controls`. | O leitor de tela informa se o menu está aberto. |
| **Contraste** | Cores com contraste mínimo de **4,5:1** (ex.: azul `#1e40af` sobre branco = 8,6:1). | Pessoas com baixa visão ou daltonismo. |
| **Informação não depende só da cor** | A situação do produto usa texto + cor ("⚠ Abaixo do mínimo"). | Pessoas daltônicas. |
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
