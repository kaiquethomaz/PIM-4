using StockControl.Models;
using StockControl.Models.Consultas;

namespace StockControl.DAO;

/// <summary>Acesso a dados da tabela Produto.</summary>
public interface IProdutoDAO : IDAOBase<Produto>
{
    /// <summary>Busca por parte do nome ou pelo código de barras exato.</summary>
    Task<List<Produto>> BuscarAsync(string? termo, bool somenteAtivos = true);

    Task<Produto?> BuscarPorCodigoBarrasAsync(string codigoBarras);

    /// <summary>Produtos alterados depois da data informada (sincronização do app).</summary>
    Task<List<Produto>> ListarAtualizadosDesdeAsync(DateTime desde);

    Task<bool> CodigoBarrasExisteAsync(string codigoBarras, int? ignorarId = null);
    Task<bool> PossuiMovimentacoesAsync(int produtoId);

    /// <summary>Executa a procedure sp_ProdutosAbaixoDoMinimo.</summary>
    Task<List<ProdutoAbaixoMinimo>> ListarAbaixoDoMinimoAsync();

    Task<int> ContarAtivosAsync();

    /// <summary>Soma de (preço x estoque) dos produtos ativos.</summary>
    Task<decimal> CalcularValorEmEstoqueAsync();
}
