using StockControl.DAO;
using StockControl.Models;
using StockControl.Models.Consultas;

namespace StockControl.Services;

public class DashboardService : IDashboardService
{
    private readonly IProdutoDAO _produtoDAO;
    private readonly ICategoriaDAO _categoriaDAO;
    private readonly IFornecedorDAO _fornecedorDAO;
    private readonly IMovimentacaoDAO _movimentacaoDAO;

    public DashboardService(
        IProdutoDAO produtoDAO,
        ICategoriaDAO categoriaDAO,
        IFornecedorDAO fornecedorDAO,
        IMovimentacaoDAO movimentacaoDAO)
    {
        _produtoDAO = produtoDAO;
        _categoriaDAO = categoriaDAO;
        _fornecedorDAO = fornecedorDAO;
        _movimentacaoDAO = movimentacaoDAO;
    }

    public async Task<ResumoDashboard> ObterResumoAsync()
    {
        var hoje = DateTime.Today;   // hoje à meia-noite

        // As consultas são feitas uma de cada vez: o DbContext não aceita
        // duas consultas ao mesmo tempo.
        return new ResumoDashboard
        {
            TotalProdutosAtivos = await _produtoDAO.ContarAtivosAsync(),
            TotalCategorias = await _categoriaDAO.ContarAsync(),
            TotalFornecedores = await _fornecedorDAO.ContarAsync(),
            ValorEmEstoque = await _produtoDAO.CalcularValorEmEstoqueAsync(),
            EntradasHoje = await _movimentacaoDAO.ContarDesdeAsync(TipoMovimentacao.Entrada, hoje),
            SaidasHoje = await _movimentacaoDAO.ContarDesdeAsync(TipoMovimentacao.Saida, hoje),
            ProdutosAbaixoDoMinimo = await _produtoDAO.ListarAbaixoDoMinimoAsync(),
            UltimasMovimentacoes = await _movimentacaoDAO.ListarUltimasAsync(10)
        };
    }
}
