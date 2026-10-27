using Microsoft.AspNetCore.Mvc;
using StockControl.Dtos;
using StockControl.Services;

namespace StockControl.Controllers.Api;

/// <summary>Resumo para a tela inicial do app.</summary>
[Route("api/dashboard")]
public class DashboardApiController : ApiControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardApiController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    /// <summary>Totais, produtos abaixo do mínimo e últimas movimentações.</summary>
    [HttpGet("resumo")]
    [ProducesResponseType(typeof(DashboardResumoDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<DashboardResumoDto>> Resumo()
    {
        var resumo = await _dashboardService.ObterResumoAsync();
        return Ok(DashboardResumoDto.DeModelo(resumo));
    }
}
