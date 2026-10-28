using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockControl.Services;

namespace StockControl.Controllers;

/// <summary>
/// Auditoria: mostra o que a trigger trg_AuditoriaProduto gravou
/// (mudanças de preço e exclusões de produtos). Somente o Admin.
/// </summary>
[Authorize(Roles = "Admin")]
public class AuditoriaController : Controller
{
    private readonly IAuditoriaService _auditoriaService;

    public AuditoriaController(IAuditoriaService auditoriaService)
    {
        _auditoriaService = auditoriaService;
    }

    // GET /Auditoria
    public async Task<IActionResult> Index()
    {
        var registros = await _auditoriaService.ListarAsync();
        return View(registros);
    }
}
