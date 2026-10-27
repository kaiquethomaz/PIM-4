using StockControl.Models;

namespace StockControl.Services;

/// <summary>Regras de negócio de Categoria.</summary>
public interface ICategoriaService
{
    Task<List<Categoria>> ListarAsync();
    Task<Categoria?> ObterAsync(int id);
    Task<Categoria> CriarAsync(Categoria categoria);
    Task AtualizarAsync(Categoria categoria);
    Task ExcluirAsync(int id);
}
