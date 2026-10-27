using Microsoft.EntityFrameworkCore;
using StockControl.Data;
using StockControl.Models;

namespace StockControl.DAO;

public class CategoriaDAO : DAOBase<Categoria>, ICategoriaDAO
{
    public CategoriaDAO(AppDbContext contexto) : base(contexto)
    {
    }

    // Sobrescreve a listagem da classe base para ordenar por nome.
    public override async Task<List<Categoria>> ListarTodosAsync()
    {
        return await _tabela.AsNoTracking().OrderBy(c => c.Nome).ToListAsync();
    }

    public async Task<bool> NomeExisteAsync(string nome, int? ignorarId = null)
    {
        // ignorarId: na edição, a própria categoria não conta como "repetida".
        return await _tabela.AnyAsync(c => c.Nome == nome && c.Id != ignorarId);
    }

    public async Task<bool> PossuiProdutosAsync(int categoriaId)
    {
        return await _contexto.Produtos.AnyAsync(p => p.CategoriaId == categoriaId);
    }
}
