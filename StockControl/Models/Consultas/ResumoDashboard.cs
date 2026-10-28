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

    // Vendas em R$ (quantidade x preço atual do produto)
    public decimal ValorVendidoHoje { get; set; }
    public int ItensVendidosHoje { get; set; }

    /// <summary>Um item por dia, dos últimos 7 dias (dias sem venda vêm com zero).</summary>
    public List<VendaDia> VendasUltimos7Dias { get; set; } = new();

    public decimal ValorVendido7Dias => VendasUltimos7Dias.Sum(v => v.Valor);

    /// <summary>Os 5 produtos mais vendidos (em unidades) nos últimos 30 dias.</summary>
    public List<ProdutoMaisVendido> MaisVendidos { get; set; } = new();
}

/// <summary>Uma linha do ranking de mais vendidos.</summary>
public class ProdutoMaisVendido
{
    public int ProdutoId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public decimal Valor { get; set; }
}

/// <summary>Total vendido em um dia (usado no gráfico do dashboard).</summary>
public class VendaDia
{
    public DateTime Dia { get; set; }
    public decimal Valor { get; set; }
    public int Itens { get; set; }
}
