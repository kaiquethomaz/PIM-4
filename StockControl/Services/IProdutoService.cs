using StockControl.Models;

namespace StockControl.Services;

/// <summary>Regras de negócio de Produto.</summary>
public interface IProdutoService
{
    Task<List<Produto>> ListarAsync(string? busca = null, bool somenteAtivos = true);
    Task<Produto?> ObterAsync(int id);
    Task<Produto?> ObterPorCodigoBarrasAsync(string codigoBarras);
    Task<List<Produto>> ListarAtualizadosDesdeAsync(DateTime desde);

    /// <summary>
    /// Cadastra o produto. Se estoqueInicial &gt; 0, registra uma ENTRADA
    /// (o estoque nunca é gravado direto: sempre passa por uma movimentação).
    /// </summary>
    Task<Produto> CriarAsync(Produto produto, int estoqueInicial, int usuarioId);

    Task AtualizarAsync(Produto produto);

    /// <summary>
    /// Exclui o produto. Se ele já teve movimentações, apenas desativa
    /// (o histórico não pode ser perdido). Retorna uma mensagem do que foi feito.
    /// </summary>
    Task<string> ExcluirAsync(int id);
}
