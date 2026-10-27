# 6. API REST (integração com o app mobile)

## 6.1 Visão geral

A API REST permite que o **aplicativo mobile** do grupo use os mesmos dados e as mesmas regras do sistema web. Ela fica **no mesmo projeto** do sistema web, em `Controllers/Api/`, e reaproveita os mesmos Services e DAOs. A regra "não vender mais do que há em estoque", por exemplo, foi escrita uma única vez e vale para os dois.

| Característica | Como foi feito |
|---|---|
| Formato | JSON (UTF-8), nomes de campos em *camelCase* (`codigoBarras`, `estoqueAtual`) |
| Autenticação | Token **JWT** no cabeçalho `Authorization: Bearer {token}` |
| Autorização | Por perfil (Admin, Gerente, Operador), igual ao sistema web |
| Documentação | **Swagger / OpenAPI** em `/swagger`, gerado a partir do código |
| Erros | Códigos HTTP padrão + corpo `{ "mensagem": "..." }` em português |
| Endereço base | `http://<ip-do-servidor>:5080/api` |

### Princípios REST aplicados
- **Recursos com substantivos no plural** (`/api/produtos`, `/api/categorias`), não verbos.
- **Verbos HTTP** com o significado padrão: `GET` consulta, `POST` cria, `PUT` altera, `DELETE` exclui.
- **Códigos de status** com o significado padrão (tabela 6.5).
- **Sem estado (*stateless*):** o servidor não guarda sessão do app; cada requisição leva o token com a identificação do usuário.
- **`201 Created` com cabeçalho `Location`** apontando para o recurso criado.

## 6.2 Endpoints

### Autenticação

| Método | Rota | Descrição | Acesso |
|---|---|---|---|
| POST | `/api/auth/login` | Recebe e-mail e senha e devolve o token JWT. | Público |
| GET | `/api/auth/eu` | Dados do usuário dono do token (para o app conferir se o token ainda vale). | Logado |

### Produtos

| Método | Rota | Descrição | Acesso |
|---|---|---|---|
| GET | `/api/produtos` | Lista os produtos ativos. Aceita `?busca=` (nome ou código). | Logado |
| GET | `/api/produtos?atualizadoDesde=2026-10-27T10:00:00` | **Sincronização:** só os produtos alterados depois da data (inclui desativados). | Logado |
| GET | `/api/produtos/{id}` | Um produto pelo Id. | Logado |
| GET | `/api/produtos/codigo-barras/{codigo}` | Um produto pelo código de barras (leitura pela câmera). | Logado |
| POST | `/api/produtos` | Cadastra (com estoque inicial opcional). | Admin, Gerente |
| PUT | `/api/produtos/{id}` | Altera (o estoque não é alterado aqui). | Admin, Gerente |
| DELETE | `/api/produtos/{id}` | Exclui; se tiver movimentações, apenas desativa. | Admin, Gerente |

### Categorias e Fornecedores

| Método | Rota | Descrição | Acesso |
|---|---|---|---|
| GET | `/api/categorias` e `/api/categorias/{id}` | Lista / busca. | Logado |
| POST, PUT, DELETE | `/api/categorias[/{id}]` | Cadastra / altera / exclui. | Admin, Gerente |
| GET | `/api/fornecedores` e `/api/fornecedores/{id}` | Lista / busca (com CNPJ formatado). | Logado |
| POST, PUT, DELETE | `/api/fornecedores[/{id}]` | Cadastra / altera / exclui. | Admin, Gerente |

### Movimentações e Dashboard

| Método | Rota | Descrição | Acesso |
|---|---|---|---|
| POST | `/api/movimentacoes` | **Registro rápido** de entrada ou venda, pelo Id ou pelo código de barras. | Logado |
| GET | `/api/movimentacoes?dataInicio=2026-10-01&dataFim=2026-10-27` | Movimentações do período (padrão: últimos 7 dias). | Logado |
| GET | `/api/movimentacoes/ultimas?quantidade=10` | Últimas movimentações. | Logado |
| GET | `/api/dashboard/resumo` | Totais, produtos abaixo do mínimo e últimas movimentações. | Logado |

## 6.3 Exemplos de uso

### Login
```http
POST /api/auth/login
Content-Type: application/json

{ "email": "operador@stockcontrol.local", "senha": "Operador@123" }
```
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiraEm": "2026-10-27T21:35:37Z",
  "usuario": {
    "id": 3, "nome": "Otávio Operador", "email": "operador@stockcontrol.local", "perfil": "Operador",
    "estabelecimentoId": 1, "estabelecimento": "Mercadinho Bom Preço"
  }
}
```

**Vários estabelecimentos:** o token leva o estabelecimento do usuário, e **todas as respostas da API trazem só os dados daquele estabelecimento**. O app não precisa enviar o estabelecimento em nenhuma rota. Por exemplo, `GET /api/produtos/codigo-barras/7891000000011` devolve o Arroz **do mercadinho de quem está logado**, mesmo que outro mercadinho tenha um produto com o mesmo código. O app pode mostrar o campo `estabelecimento` no topo da tela.

### Venda pelo código de barras
```http
POST /api/movimentacoes
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
Content-Type: application/json

{ "codigoBarras": "7891000000011", "tipo": "SAIDA", "quantidade": 2, "observacao": "Venda pelo app" }
```
```json
{
  "movimentacao": { "id": 40, "tipo": "SAIDA", "produtoId": 1, "produto": "Arroz Branco Tipo 1 5kg", "quantidade": 2, "...": "..." },
  "produto": { "id": 1, "nome": "Arroz Branco Tipo 1 5kg", "estoqueAtual": 27, "abaixoDoMinimo": false, "...": "..." }
}
```
A resposta já traz o produto com o **estoque atualizado**, para o app mostrar na hora.

### Venda sem estoque suficiente
```json
HTTP 400
{ "mensagem": "Estoque insuficiente. \"Detergente Líquido 500ml\" tem 3 unidade(s)." }
```

### Dados inválidos
```json
HTTP 400
{
  "mensagem": "Dados inválidos.",
  "erros": {
    "Nome": ["Informe o nome."],
    "CodigoBarras": ["O código de barras deve ter só números (até 20)."]
  }
}
```

## 6.4 Autenticação com JWT

```mermaid
sequenceDiagram
    participant App as App mobile
    participant API as API StockControl
    participant BD as SQL Server

    App->>API: POST /api/auth/login {email, senha}
    API->>BD: busca usuário pelo e-mail
    API->>API: BCrypt.Verify(senha, hash)
    API->>API: gera token JWT assinado (HMAC-SHA256)
    API-->>App: { token, expiraEm, usuario }
    Note over App: guarda o token
    App->>API: GET /api/produtos<br/>Authorization: Bearer {token}
    API->>API: valida assinatura, emissor, público e validade
    API-->>App: 200 [ produtos ]
```

O **JWT** (*JSON Web Token*) tem três partes separadas por ponto: `cabeçalho.dados.assinatura`.
- **Dados (*claims*):** `sub` (Id do usuário), `name`, `email`, `role` (perfil), `EstabelecimentoId` e `Estabelecimento` (mercadinho do usuário), `jti` (identificador único do token) e `exp` (validade).
- **Assinatura:** calculada com uma **chave secreta** que só o servidor conhece (algoritmo HMAC-SHA256). Se alguém alterar os dados do token, por exemplo trocando o perfil para "Admin", a assinatura deixa de bater e a API responde **401**.
- **Validade:** 8 horas. Depois disso o app precisa fazer login de novo.

Em cada requisição, a API confere: assinatura, emissor (`StockControl.Api`), público (`StockControl.Mobile`) e validade.

**Cookie × JWT no mesmo projeto:** o sistema web usa cookie (esquema padrão) e a API usa JWT. Todos os controllers da API herdam de `ApiControllerBase`, que tem `[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]`. Assim, a API **só aceita token**, e o cookie do site não dá acesso a ela.

## 6.5 Códigos de resposta

| Código | Quando acontece |
|---|---|
| **200 OK** | Consulta ou alteração feita. |
| **201 Created** | Cadastro feito (produto, categoria, fornecedor, movimentação). |
| **204 No Content** | Exclusão de categoria/fornecedor feita. |
| **400 Bad Request** | Dados inválidos ou regra de negócio violada (estoque insuficiente, código repetido...). |
| **401 Unauthorized** | Sem token, token inválido/expirado/adulterado, ou login com senha errada. |
| **403 Forbidden** | Token válido, mas o perfil não tem permissão (ex.: Operador tentando cadastrar produto). |
| **404 Not Found** | Produto/categoria/fornecedor não encontrado. |

## 6.6 Organização do código da API

| Classe | Função |
|---|---|
| `ApiControllerBase` | Classe base (herança) com tudo que vale para a API inteira: `[ApiController]`, autenticação JWT, CORS e filtro de erros. |
| `TratarRegraNegocioAttribute` | Filtro que converte `RegraNegocioException` em HTTP 400 `{ "mensagem": ... }`, sem precisar de `try/catch` em cada endpoint. |
| `AuthController`, `ProdutosApiController`, `CategoriasApiController`, `FornecedoresApiController`, `MovimentacoesApiController`, `DashboardApiController` | Um controller por recurso. |
| `TokenService` | Gera o token JWT. |
| `Dtos/` | *Data Transfer Objects*: objetos que entram e saem da API. |

**Por que DTOs em vez das entidades?**
- **Segurança:** a entidade `Usuario` tem `SenhaHash`, que nunca pode sair do servidor.
- **Controle do que entra:** o `ProdutoRequisicao` não tem `EstoqueAtual`, então o app não consegue alterar o estoque por fora das movimentações.
- **Formato adequado ao app:** o `ProdutoDto` já traz o nome da categoria e do fornecedor e o campo `abaixoDoMinimo` calculado.
- **Evitar referências circulares** no JSON (Produto → Categoria → Produtos → ...).

## 6.7 Sincronização do app

Para o app funcionar bem com internet ruim, ele pode guardar os produtos no celular e baixar **só o que mudou**:

1. Na primeira vez: `GET /api/produtos` (todos os ativos).
2. O app guarda o maior `atualizadoEm` recebido.
3. Nas próximas vezes: `GET /api/produtos?atualizadoDesde={maior atualizadoEm}`.
4. Os produtos recebidos com `"ativo": false` devem ser removidos da lista do app.

Toda alteração em um produto, inclusive a mudança de estoque feita pela trigger, atualiza `AtualizadoEm`, e existe um índice (`IX_Produto_Estab_AtualizadoEm`, por estabelecimento e data) para essa consulta ser rápida.

## 6.8 Segurança da API

- **JWT** assinado, com validade de 8 horas; a chave fica na configuração e, em produção, deve vir de variável de ambiente (`Jwt__Chave`).
- **Autorização por perfil** com `[Authorize(Roles = "Admin,Gerente")]` nos endpoints de cadastro.
- **Validação** de todo JSON recebido (*DataAnnotations* + `[ApiController]`), com resposta 400 em português.
- **CORS** liberado apenas para as origens configuradas em `appsettings.json` (`Cors:OrigensPermitidas`). Isso é necessário quando o app roda no navegador durante o desenvolvimento (ex.: Expo Web); apps nativos não precisam.
- **HTTPS** obrigatório em produção. Em desenvolvimento, as rotas `/api` também aceitam HTTP, para o celular conseguir acessar o computador pela rede local.
- **Swagger** disponível apenas em ambiente de desenvolvimento, para não expor o mapa da API em produção.

## 6.9 Como o app mobile se conecta (guia para o colega do grupo)

1. No computador que roda o sistema, descubra o IP na rede: `ipconfig` → "Endereço IPv4" (ex.: `192.168.0.15`).
2. Rode o sistema aceitando conexões da rede:
   ```bash
   cd /d/PIM-4/StockControl && /d/dotnet/dotnet.exe run --urls "http://0.0.0.0:5080"
   ```
3. Na primeira vez, o Windows pergunta sobre o **Firewall**: permitir em "Redes privadas".
4. No app, use a URL base `http://192.168.0.15:5080/api` (celular e computador no mesmo Wi-Fi).
5. Documentação interativa: `http://192.168.0.15:5080/swagger`.
6. Se o app rodar no navegador (Expo Web etc.), adicione o endereço dele em `Cors:OrigensPermitidas` no `appsettings.json`.

## 6.10 Testando pelo Swagger

1. Abra `http://localhost:5080/swagger`.
2. Em **Auth → POST /api/auth/login**, clique em *Try it out*, informe e-mail e senha e clique em *Execute*.
3. Copie o valor de `token` da resposta.
4. Clique no botão **Authorize** (cadeado), cole o token e confirme.
5. Agora todos os endpoints podem ser testados com *Try it out*.
