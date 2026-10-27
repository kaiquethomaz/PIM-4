using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockControl.Services;

namespace StockControl.Controllers;

/// <summary>Dashboard: a primeira tela depois do login.</summary>
[Authorize]   // só usuários logados
public class HomeController : Controller
{
    private readonly IDashboardService _dashboardService;

    public HomeController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    // GET /  (ou /Home/Index)
    public async Task<IActionResult> Index()
    {
        // Totais, produtos abaixo do mínimo (procedure sp_ProdutosAbaixoDoMinimo)
        // e últimas movimentações.
        var resumo = await _dashboardService.ObterResumoAsync();
        return View(resumo);
    }

    [AllowAnonymous]
    public IActionResult Erro()
    {
        return View();
    }

    // GET /Home/Status/404 -> página amigável para erros sem conteúdo
    // (configurado no Program.cs com UseStatusCodePagesWithReExecute).
    [AllowAnonymous]
    public IActionResult Status(int id)
    {
        ViewBag.Codigo = id;
        return View();
    }
}
