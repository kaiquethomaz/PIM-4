using StockControl.Models;

namespace StockControl.ViewModels;

/// <summary>Tela de Vendas: vendas (saídas) do período e os totais.</summary>
public class VendasViewModel
{
    public DateTime DataInicio { get; set; }
    public DateTime DataFim { get; set; }
    public List<Movimentacao> Vendas { get; set; } = new();

    /// <summary>Mensagem de erro do filtro (ex.: data inicial maior que a final).</summary>
    public string? Erro { get; set; }

    // Valor de cada venda = quantidade x preço atual do produto.
    public static decimal ValorDe(Movimentacao venda) => venda.Quantidade * (venda.Produto?.Preco ?? 0);

    public decimal TotalVendido => Vendas.Sum(ValorDe);
    public int TotalItens => Vendas.Sum(v => v.Quantidade);
    public decimal ValorMedioPorVenda => Vendas.Count == 0 ? 0 : TotalVendido / Vendas.Count;
}
