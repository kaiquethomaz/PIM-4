using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockControl.Excecoes;
using StockControl.Services;
using StockControl.ViewModels;

namespace StockControl.Controllers;

/// <summary>
/// Vendas do estabelecimento. Uma venda é uma movimentação do tipo Saída,
/// então esta tela mostra só as saídas, com o valor em R$ e os totais do período.
/// O registro de uma venda nova continua em Movimentacoes/Registrar.
/// </summary>
[Authorize]
public class VendasController : Controller
{
    private readonly IMovimentacaoService _movimentacaoService;

    public VendasController(IMovimentacaoService movimentacaoService)
    {
        _movimentacaoService = movimentacaoService;
    }

    // GET /Vendas?dataInicio=2026-10-01&dataFim=2026-10-29
    public async Task<IActionResult> Index(DateTime? dataInicio, DateTime? dataFim)
    {
        // Sem filtro: últimos 7 dias (hoje incluso).
        var modelo = new VendasViewModel
        {
            DataFim = (dataFim ?? DateTime.Today).Date,
            DataInicio = (dataInicio ?? DateTime.Today.AddDays(-6)).Date
        };

        try
        {
            modelo.Vendas = await _movimentacaoService.ListarVendasAsync(modelo.DataInicio, modelo.DataFim);
        }
        catch (RegraNegocioException erro)
        {
            modelo.Erro = erro.Message;
        }

        return View(modelo);
    }
}
