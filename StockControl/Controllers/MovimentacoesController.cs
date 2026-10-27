using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using StockControl.Excecoes;
using StockControl.Extensions;
using StockControl.Models;
using StockControl.Services;
using StockControl.ViewModels;

namespace StockControl.Controllers;

/// <summary>
/// Entradas de mercadoria e vendas (saídas). Todos os perfis podem registrar.
/// Uma venda é uma movimentação do tipo Saída.
/// </summary>
[Authorize]
public class MovimentacoesController : Controller
{
    private readonly IMovimentacaoService _movimentacaoService;
    private readonly IProdutoService _produtoService;

    public MovimentacoesController(IMovimentacaoService movimentacaoService, IProdutoService produtoService)
    {
        _movimentacaoService = movimentacaoService;
        _produtoService = produtoService;
    }

    // GET /Movimentacoes?dataInicio=2026-10-01&dataFim=2026-10-27
    public async Task<IActionResult> Index(DateTime? dataInicio, DateTime? dataFim)
    {
        // Sem filtro: últimos 7 dias.
        var modelo = new MovimentacaoListaViewModel
        {
            DataFim = (dataFim ?? DateTime.Today).Date,
            DataInicio = (dataInicio ?? DateTime.Today.AddDays(-7)).Date
        };

        try
        {
            // Usa a procedure sp_RelatorioMovimentacoes (via Service -> DAO).
            modelo.Itens = await _movimentacaoService.ListarPorPeriodoAsync(modelo.DataInicio, modelo.DataFim);
        }
        catch (RegraNegocioException erro)
        {
            modelo.Erro = erro.Message;
        }

        return View(modelo);
    }

    // GET /Movimentacoes/Registrar?tipo=Saida&produtoId=1
    [HttpGet]
    public async Task<IActionResult> Registrar(TipoMovimentacao? tipo, int produtoId = 0)
    {
        var modelo = new MovimentacaoFormViewModel { Tipo = tipo, ProdutoId = produtoId };
        await CarregarProdutosAsync(modelo);
        return View(modelo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Registrar(MovimentacaoFormViewModel modelo)
    {
        if (!ModelState.IsValid)
        {
            await CarregarProdutosAsync(modelo);
            return View(modelo);
        }

        try
        {
            var movimentacao = await _movimentacaoService.RegistrarAsync(
                modelo.ProdutoId,
                modelo.Tipo!.Value,
                modelo.Quantidade!.Value,
                User.ObterUsuarioId(),
                modelo.Observacao);

            // Mostra o estoque já atualizado pela trigger.
            var produto = await _produtoService.ObterAsync(movimentacao.ProdutoId);
            var acao = movimentacao.Tipo == TipoMovimentacao.Entrada ? "Entrada registrada" : "Venda registrada";
            TempData["Sucesso"] = $"{acao}: {movimentacao.Quantidade} x {produto?.Nome}. Estoque atual: {produto?.EstoqueAtual}.";

            // Volta para o formulário com o mesmo tipo, para registrar a próxima rapidamente.
            return RedirectToAction(nameof(Registrar), new { tipo = movimentacao.Tipo });
        }
        catch (RegraNegocioException erro)
        {
            ModelState.AddModelError(string.Empty, erro.Message);
            await CarregarProdutosAsync(modelo);
            return View(modelo);
        }
    }

    /// <summary>Lista os produtos ativos no &lt;select&gt;, mostrando o estoque de cada um.</summary>
    private async Task CarregarProdutosAsync(MovimentacaoFormViewModel modelo)
    {
        var produtos = await _produtoService.ListarAsync();
        modelo.Produtos = produtos
            .Select(p => new SelectListItem(
                $"{p.Nome} — cód. {p.CodigoBarras} (estoque: {p.EstoqueAtual})",
                p.Id.ToString()))
            .ToList();
    }
}
