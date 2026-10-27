using StockControl.Models;

namespace StockControl.DAO;

/// <summary>
/// Interface genérica com as operações básicas (CRUD) que todo DAO de cadastro tem.
/// O "T" é o tipo da entidade (Produto, Categoria...). Assim escrevemos o contrato
/// uma vez só, e cada DAO específico (IProdutoDAO etc.) herda e acrescenta o que precisa.
/// </summary>
public interface IDAOBase<T> where T : EntidadeBase
{
    Task<List<T>> ListarTodosAsync();
    Task<T?> BuscarPorIdAsync(int id);
    Task<int> ContarAsync();
    Task InserirAsync(T entidade);
    Task AtualizarAsync(T entidade);
    Task ExcluirAsync(T entidade);
}
