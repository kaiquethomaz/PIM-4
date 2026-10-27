using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockControl.Services;
using StockControl.ViewModels;

namespace StockControl.Controllers;

/// <summary>Login e logout do sistema web (autenticação por cookie).</summary>
public class ContaController : Controller
{
    private readonly IUsuarioService _usuarioService;

    public ContaController(IUsuarioService usuarioService)
    {
        _usuarioService = usuarioService;
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

        // "Claims" = informações sobre o usuário que vão dentro do cookie.
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new(ClaimTypes.Name, usuario.Nome),
            new(ClaimTypes.Email, usuario.Email),
            new(ClaimTypes.Role, usuario.Perfil.ToString())   // usado no [Authorize(Roles = ...)]
        };

        var identidade = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identidade),
            new AuthenticationProperties { IsPersistent = modelo.LembrarDeMim });

        // Url.IsLocalUrl evita "open redirect" (mandar o usuário para um site falso depois do login).
        if (!string.IsNullOrEmpty(modelo.ReturnUrl) && Url.IsLocalUrl(modelo.ReturnUrl))
            return Redirect(modelo.ReturnUrl);

        return RedirectToAction("Index", "Home");
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
}
