using StockControl.Models;

namespace StockControl.DAO;

/// <summary>Acesso a dados da tabela Categoria.</summary>
public interface ICategoriaDAO : IDAOBase<Categoria>
{
    Task<bool> NomeExisteAsync(string nome, int? ignorarId = null);
    Task<bool> PossuiProdutosAsync(int categoriaId);
}
