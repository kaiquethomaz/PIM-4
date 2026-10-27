namespace StockControl.Models.Consultas;

/// <summary>
/// Números do dashboard. Usado pela tela web (Dashboard) e pela API
/// (GET /api/dashboard/resumo), por isso fica aqui e não em ViewModels.
/// </summary>
public class ResumoDashboard
{
    public int TotalProdutosAtivos { get; set; }
    public int TotalCategorias { get; set; }
    public int TotalFornecedores { get; set; }
    public decimal ValorEmEstoque { get; set; }
    public int EntradasHoje { get; set; }
    public int SaidasHoje { get; set; }
    public List<ProdutoAbaixoMinimo> ProdutosAbaixoDoMinimo { get; set; } = new();
    public List<Movimentacao> UltimasMovimentacoes { get; set; } = new();
}
