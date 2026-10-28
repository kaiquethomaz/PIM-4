using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockControl.Excecoes;
using StockControl.Extensions;
using StockControl.Models;
using StockControl.Services;
using StockControl.ViewModels;

namespace StockControl.Controllers;

/// <summary>
/// Login, logout e cadastro de um novo estabelecimento (autenticação por cookie).
/// </summary>
public class ContaController : Controller
{
    private readonly IUsuarioService _usuarioService;
    private readonly IEstabelecimentoService _estabelecimentoService;

    public ContaController(IUsuarioService usuarioService, IEstabelecimentoService estabelecimentoService)
    {
        _usuarioService = usuarioService;
        _estabelecimentoService = estabelecimentoService;
    }

    [HttpGet]
    [AllowAnonymous]   // a tela de login precisa abrir sem estar logado
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]   // proteção CSRF: só aceita o POST vindo do nosso formulário
    public async Task<IActionResult> Login(LoginViewModel modelo)
    {
        if (!ModelState.IsValid)
            return View(modelo);

        var usuario = await _usuarioService.AutenticarAsync(modelo.Email, modelo.Senha);
        if (usuario is null)
        {
            // Mensagem genérica de propósito: não contamos se o erro foi no
            // e-mail ou na senha (dificulta descobrir e-mails cadastrados).
            ModelState.AddModelError(string.Empty, "E-mail ou senha inválidos.");
            return View(modelo);
        }

        await EntrarAsync(usuario, modelo.LembrarDeMim);

        // Url.IsLocalUrl evita "open redirect" (mandar o usuário para um site falso depois do login).
        if (!string.IsNullOrEmpty(modelo.ReturnUrl) && Url.IsLocalUrl(modelo.ReturnUrl))
            return Redirect(modelo.ReturnUrl);

        return RedirectToAction("Index", "Home");
    }

    // GET /Conta/Cadastrar -> "Cadastrar meu estabelecimento"
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Cadastrar()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");

        return View(new CadastroEstabelecimentoViewModel());
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cadastrar(CadastroEstabelecimentoViewModel modelo)
    {
        if (!ModelState.IsValid)
            return View(modelo);

        try
        {
            // Cria o estabelecimento + o dono (Admin) e já entra no sistema.
            var dono = await _estabelecimentoService.CadastrarAsync(
                modelo.ParaEstabelecimento(), modelo.ParaDono(), modelo.Senha);

            await EntrarAsync(dono, lembrarDeMim: false);

            TempData["Sucesso"] = $"Bem-vindo(a)! O estabelecimento \"{modelo.NomeFantasia}\" foi cadastrado. " +
                                  "Comece cadastrando seus fornecedores e produtos, e crie em Usuários o acesso dos seus funcionários.";
            return RedirectToAction("Index", "Home");
        }
        catch (RegraNegocioException erro)
        {
            ModelState.AddModelError(string.Empty, erro.Message);
            return View(modelo);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Sair()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public IActionResult AcessoNegado()
    {
        return View();
    }

    // GET /Conta/MinhaConta -> dados do usuário logado e troca da própria senha
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> MinhaConta()
    {
        var modelo = await MontarMinhaContaAsync();
        return modelo is null ? NotFound() : View(modelo);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MinhaConta(MinhaContaViewModel modelo)
    {
        if (ModelState.IsValid)
        {
            try
            {
                await _usuarioService.AlterarMinhaSenhaAsync(User.ObterUsuarioId(), modelo.SenhaAtual, modelo.NovaSenha);
                TempData["Sucesso"] = "Senha alterada com sucesso.";
                return RedirectToAction(nameof(MinhaConta));
            }
            catch (RegraNegocioException erro)
            {
                ModelState.AddModelError(string.Empty, erro.Message);
            }
        }

        // Volta para a tela com os dados de exibição preenchidos de novo.
        var dados = await MontarMinhaContaAsync();
        if (dados is null)
            return NotFound();
        modelo.Nome = dados.Nome;
        modelo.Email = dados.Email;
        modelo.Perfil = dados.Perfil;
        modelo.Estabelecimento = dados.Estabelecimento;
        return View(modelo);
    }

    private async Task<MinhaContaViewModel?> MontarMinhaContaAsync()
    {
        var usuario = await _usuarioService.ObterAsync(User.ObterUsuarioId());
        if (usuario is null)
            return null;

        return new MinhaContaViewModel
        {
            Nome = usuario.Nome,
            Email = usuario.Email,
            Perfil = usuario.Perfil.ToString(),
            Estabelecimento = usuario.Estabelecimento?.NomeFantasia ?? string.Empty
        };
    }

    /// <summary>
    /// Cria o cookie de autenticação com os dados do usuário ("claims"):
    /// Id, nome, e-mail, perfil e o estabelecimento em que ele trabalha.
    /// </summary>
    private async Task EntrarAsync(Usuario usuario, bool lembrarDeMim)
    {
        var claims = ClaimsStockControl.CriarClaims(usuario, usuario.Estabelecimento?.NomeFantasia ?? string.Empty);
        var identidade = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identidade),
            new AuthenticationProperties { IsPersistent = lembrarDeMim });
    }
}
