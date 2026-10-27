using System.Globalization;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Mvc;
using StockControl.DAO;
using StockControl.Data;
using StockControl.Dtos;
using StockControl.Services;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------
// 1. MVC (Controllers + Views em Razor) e controllers da API
// ---------------------------------------------------------------------
builder.Services.AddControllersWithViews(opcoes =>
{
    // Mensagens de erro de conversão em português
    // (ex.: alguém digita "abc" no campo de preço).
    var mensagens = opcoes.ModelBindingMessageProvider;
    mensagens.SetAttemptedValueIsInvalidAccessor((valor, campo) => $"O valor \"{valor}\" não é válido para {campo}.");
    mensagens.SetValueMustNotBeNullAccessor(campo => $"Informe {campo}.");
    mensagens.SetValueIsInvalidAccessor(valor => $"O valor \"{valor}\" é inválido.");
    mensagens.SetValueMustBeANumberAccessor(campo => $"{campo} deve ser um número.");
    mensagens.SetMissingBindRequiredValueAccessor(campo => $"Informe {campo}.");
    mensagens.SetUnknownValueIsInvalidAccessor(campo => $"Valor inválido para {campo}.");
    mensagens.SetNonPropertyAttemptedValueIsInvalidAccessor(valor => $"O valor \"{valor}\" é inválido.");
})
.ConfigureApiBehaviorOptions(opcoes =>
{
    // API: quando o JSON recebido é inválido, responde em português e no mesmo
    // formato das outras mensagens: { "mensagem": "...", "erros": { "Campo": [...] } }
    opcoes.InvalidModelStateResponseFactory = contexto =>
    {
        var erros = contexto.ModelState
            .Where(campo => campo.Value!.Errors.Count > 0)
            .ToDictionary(
                campo => string.IsNullOrEmpty(campo.Key) ? "corpo" : campo.Key.TrimStart('$', '.'),
                campo => campo.Value!.Errors
                    .Select(e => e.ErrorMessage.StartsWith("A non-empty request body")
                        ? "Envie os dados em JSON no corpo da requisição."
                        : string.IsNullOrEmpty(e.ErrorMessage) || e.ErrorMessage.StartsWith("The ")
                            ? "Valor inválido ou em formato incorreto."
                            : e.ErrorMessage)
                    .ToArray());

        return new BadRequestObjectResult(new ErroValidacaoResposta { Erros = erros });
    };
});

// ---------------------------------------------------------------------
// 2. Banco de dados: SQL Server.
//    A connection string fica no appsettings.json ("StockControl").
//    O banco é criado pelo script database/script_stockcontrol.sql.
// ---------------------------------------------------------------------
builder.Services.AddDbContext<AppDbContext>(opcoes =>
    opcoes.UseSqlServer(builder.Configuration.GetConnectionString("StockControl")));

// ---------------------------------------------------------------------
// 3. Autenticação: dois "esquemas", um para cada tipo de cliente
//
//    a) COOKIE (padrão) -> sistema web no navegador.
//       Depois do login, o navegador guarda um cookie criptografado com os
//       dados do usuário (Id, nome, perfil) e envia em toda requisição.
//
//    b) JWT -> API do app mobile.
//       O app faz login em POST /api/auth/login, recebe um token e envia
//       esse token no cabeçalho "Authorization: Bearer {token}".
// ---------------------------------------------------------------------
builder.Services.Configure<ConfiguracaoJwt>(builder.Configuration.GetSection("Jwt"));
var configuracaoJwt = builder.Configuration.GetSection("Jwt").Get<ConfiguracaoJwt>()
    ?? throw new InvalidOperationException("Seção \"Jwt\" não encontrada no appsettings.json.");

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(opcoes =>
    {
        opcoes.LoginPath = "/Conta/Login";               // sem login -> vai para cá
        opcoes.LogoutPath = "/Conta/Sair";
        opcoes.AccessDeniedPath = "/Conta/AcessoNegado"; // logado, mas sem permissão
        opcoes.ExpireTimeSpan = TimeSpan.FromHours(8);   // um turno de trabalho
        opcoes.SlidingExpiration = true;                 // renova enquanto o usuário usa
        opcoes.Cookie.Name = "StockControl.Auth";
        opcoes.Cookie.HttpOnly = true;                   // JavaScript não consegue ler o cookie
        opcoes.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        opcoes.Cookie.SameSite = SameSiteMode.Lax;       // ajuda contra CSRF
    })
    .AddJwtBearer(opcoes =>
    {
        // Mantém os nomes curtos dos claims ("sub", "name", "role") como estão no token.
        opcoes.MapInboundClaims = false;

        // O que a API confere em cada token recebido:
        opcoes.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,                 // foi gerado por nós?
            ValidIssuer = configuracaoJwt.Emissor,
            ValidateAudience = true,               // é para o app mobile?
            ValidAudience = configuracaoJwt.Publico,
            ValidateIssuerSigningKey = true,       // a assinatura bate com a nossa chave?
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuracaoJwt.Chave)),
            ValidateLifetime = true,               // ainda não expirou?
            ClockSkew = TimeSpan.FromMinutes(1),   // tolerância de relógio entre servidor e celular
            NameClaimType = "name",
            RoleClaimType = "role"                 // usado pelo [Authorize(Roles = ...)]
        };
    });

builder.Services.AddAuthorization();

// ---------------------------------------------------------------------
// 4. CORS: libera chamadas do app mobile quando ele roda em outro endereço
//    (ex.: app em React Native/Expo rodando no navegador durante o desenvolvimento).
//    Apps nativos no celular não precisam de CORS. As origens ficam no appsettings.json.
// ---------------------------------------------------------------------
var origensPermitidas = builder.Configuration.GetSection("Cors:OrigensPermitidas").Get<string[]>()
                        ?? Array.Empty<string>();
builder.Services.AddCors(opcoes =>
{
    opcoes.AddPolicy("Mobile", politica => politica
        .WithOrigins(origensPermitidas)
        .AllowAnyHeader()
        .AllowAnyMethod());
});

// ---------------------------------------------------------------------
// 5. Swagger: documentação interativa da API em /swagger
// ---------------------------------------------------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opcoes =>
{
    opcoes.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "StockControl API",
        Version = "v1",
        Description = "API REST do StockControl para o aplicativo mobile. " +
                      "Faça login em POST /api/auth/login, copie o token e clique em \"Authorize\"."
    });

    // Usa os comentários /// dos controllers e DTOs como documentação.
    var arquivoXml = Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");
    opcoes.IncludeXmlComments(arquivoXml);

    // Agrupa os endpoints pelo nome do controller sem o sufixo "Api" (ProdutosApi -> Produtos).
    opcoes.TagActionsBy(api => new[] { api.ActionDescriptor.RouteValues["controller"]!.Replace("Api", "") });

    // Botão "Authorize" para colar o token JWT e testar os endpoints protegidos.
    opcoes.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Cole aqui apenas o token recebido no login (sem a palavra Bearer)."
    });
    opcoes.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// ---------------------------------------------------------------------
// 6. Injeção de dependência (DI)
//    "Quando alguém pedir IProdutoDAO, entregue um ProdutoDAO."
//    AddScoped = um objeto novo por requisição HTTP (mesmo tempo de vida do DbContext).
//    Fluxo: Controller -> Service -> DAO -> AppDbContext -> Banco
// ---------------------------------------------------------------------

// DAOs (acesso a dados)
builder.Services.AddScoped<IUsuarioDAO, UsuarioDAO>();
builder.Services.AddScoped<ICategoriaDAO, CategoriaDAO>();
builder.Services.AddScoped<IFornecedorDAO, FornecedorDAO>();
builder.Services.AddScoped<IProdutoDAO, ProdutoDAO>();
builder.Services.AddScoped<IMovimentacaoDAO, MovimentacaoDAO>();

// Services (regras de negócio)
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<ICategoriaService, CategoriaService>();
builder.Services.AddScoped<IFornecedorService, FornecedorService>();
builder.Services.AddScoped<IProdutoService, ProdutoService>();
builder.Services.AddScoped<IMovimentacaoService, MovimentacaoService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<ITokenService, TokenService>();

var app = builder.Build();

// ---------------------------------------------------------------------
// Pipeline: o caminho que cada requisição percorre
// ---------------------------------------------------------------------
if (app.Environment.IsDevelopment())
{
    // Swagger só em desenvolvimento: em produção não expomos o mapa da API.
    app.UseSwagger();
    app.UseSwaggerUI(opcoes => opcoes.DocumentTitle = "StockControl API");
}
else
{
    app.UseExceptionHandler("/Home/Erro");
    app.UseHsts();   // obriga o navegador a usar HTTPS
}

// http -> https. Exceção: em desenvolvimento, a API aceita HTTP para o celular
// conseguir acessar o PC pela rede local (o certificado de desenvolvimento só
// vale para "localhost"). Em produção, tudo passa por HTTPS.
app.UseWhen(
    contexto => !(app.Environment.IsDevelopment() && contexto.Request.Path.StartsWithSegments("/api")),
    ramo => ramo.UseHttpsRedirection());

app.UseStaticFiles();        // arquivos da pasta wwwroot (CSS, JS)

// Cultura brasileira: datas dd/MM/yyyy, dinheiro R$ e vírgula decimal ("28,90").
var culturaBrasil = new CultureInfo("pt-BR");
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(culturaBrasil),
    SupportedCultures = new[] { culturaBrasil },
    SupportedUICultures = new[] { culturaBrasil }
});

app.UseRouting();
app.UseCors();               // aplica a política "Mobile" nos controllers da API
app.UseAuthentication();     // 1º descobre QUEM é o usuário (cookie ou token)
app.UseAuthorization();      // 2º verifica se ele PODE acessar

// Rota padrão das telas: /Controller/Acao/Id  (ex.: /Produtos/Editar/5)
// Os controllers da API usam rotas próprias ([Route("api/...")]).
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
