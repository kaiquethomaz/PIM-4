using StockControl.Models;

namespace StockControl.Services;

/// <summary>Regras de negócio de Fornecedor.</summary>
public interface IFornecedorService
{
    Task<List<Fornecedor>> ListarAsync();
    Task<Fornecedor?> ObterAsync(int id);
    Task<Fornecedor> CriarAsync(Fornecedor fornecedor);
    Task AtualizarAsync(Fornecedor fornecedor);
    Task ExcluirAsync(int id);
}
