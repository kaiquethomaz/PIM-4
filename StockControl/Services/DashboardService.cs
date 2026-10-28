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
        var seteDiasAtras = hoje.AddDays(-6);

        // Vendas por dia: o banco só devolve os dias que tiveram venda;
        // aqui completamos os 7 dias (dia sem venda = zero) para o gráfico.
        var vendasNoBanco = await _movimentacaoDAO.SomarVendasPorDiaAsync(seteDiasAtras);
        var vendas7Dias = Enumerable.Range(0, 7)
            .Select(i => seteDiasAtras.AddDays(i))
            .Select(dia => vendasNoBanco.FirstOrDefault(v => v.Dia == dia) ?? new VendaDia { Dia = dia })
            .ToList();
        var vendasHoje = vendas7Dias.Last();

        // As consultas são feitas uma de cada vez: o DbContext não aceita
        // duas consultas ao mesmo tempo.
        return new ResumoDashboard
        {
            MaisVendidos = await _movimentacaoDAO.ListarMaisVendidosAsync(hoje.AddDays(-29), 5),
            ValorVendidoHoje = vendasHoje.Valor,
            ItensVendidosHoje = vendasHoje.Itens,
            VendasUltimos7Dias = vendas7Dias,
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
