using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockControl.Excecoes;
using StockControl.Extensions;
using StockControl.Services;
using StockControl.ViewModels;

namespace StockControl.Controllers;

/// <summary>"Meu estabelecimento": dados do mercadinho. Somente o Admin (dono).</summary>
[Authorize(Roles = "Admin")]
public class EstabelecimentoController : Controller
{
    private readonly IEstabelecimentoService _estabelecimentoService;

    public EstabelecimentoController(IEstabelecimentoService estabelecimentoService)
    {
        _estabelecimentoService = estabelecimentoService;
    }

    // GET /Estabelecimento
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var estabelecimento = await _estabelecimentoService.ObterAtualAsync();
        if (estabelecimento is null)
            return NotFound();

        return View(EstabelecimentoFormViewModel.DeModelo(estabelecimento));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(EstabelecimentoFormViewModel modelo)
    {
        var atual = await _estabelecimentoService.ObterAtualAsync();
        if (atual is null)
            return NotFound();

        // Campos só de exibição voltam do banco (não vêm do formulário).
        modelo.CnpjFormatado = atual.CnpjFormatado();
        modelo.CriadoEm = atual.CriadoEm;

        if (!ModelState.IsValid)
            return View(modelo);

        try
        {
            await _estabelecimentoService.AtualizarAsync(modelo.ParaModelo());
            await AtualizarNomeNoCookieAsync(modelo.NomeFantasia.Trim());
            TempData["Sucesso"] = "Dados do estabelecimento atualizados.";
            return RedirectToAction(nameof(Index));
        }
        catch (RegraNegocioException erro)
        {
            ModelState.AddModelError(string.Empty, erro.Message);
            return View(modelo);
        }
    }

    /// <summary>
    /// O nome do estabelecimento fica gravado no cookie (aparece no cabeçalho).
    /// Depois de alterar, gravamos o cookie de novo com o nome novo.
    /// </summary>
    private async Task AtualizarNomeNoCookieAsync(string novoNome)
    {
        var autenticacao = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        if (autenticacao.Principal is null)
            return;

        var claims = autenticacao.Principal.Claims
            .Where(c => c.Type != ClaimsStockControl.Estabelecimento)
            .Append(new Claim(ClaimsStockControl.Estabelecimento, novoNome));

        var identidade = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identidade),
            autenticacao.Properties);   // mantém o "Manter conectado"
    }
}
