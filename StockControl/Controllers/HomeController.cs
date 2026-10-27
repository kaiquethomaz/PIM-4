using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockControl.Services;

namespace StockControl.Controllers;

/// <summary>
/// Página inicial TEMPORÁRIA (etapa 3): só serve para provar que a cadeia
/// Controller -> Service -> DAO -> AppDbContext -> Banco está funcionando.
/// Na etapa 6 ela vira o Dashboard de verdade.
/// </summary>
[Authorize]   // só usuários logados
public class HomeController : Controller
{
    private readonly IDashboardService _dashboardService;

    public HomeController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    public async Task<IActionResult> Index()
    {
        var resumo = await _dashboardService.ObterResumoAsync();
        return View(resumo);
    }

    [AllowAnonymous]
    public IActionResult Erro()
    {
        return View();
    }
}
