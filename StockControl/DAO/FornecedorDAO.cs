using Microsoft.EntityFrameworkCore;
using StockControl.Data;
using StockControl.Models;

namespace StockControl.DAO;

public class FornecedorDAO : DAOBase<Fornecedor>, IFornecedorDAO
{
    public FornecedorDAO(AppDbContext contexto) : base(contexto)
    {
    }

    public override async Task<List<Fornecedor>> ListarTodosAsync()
    {
        return await _tabela.AsNoTracking().OrderBy(f => f.RazaoSocial).ToListAsync();
    }

    public async Task<bool> CnpjExisteAsync(string cnpj, int? ignorarId = null)
    {
        return await _tabela.AnyAsync(f => f.CNPJ == cnpj && f.Id != ignorarId);
    }

    public async Task<bool> PossuiProdutosAsync(int fornecedorId)
    {
        return await _contexto.Produtos.AnyAsync(p => p.FornecedorId == fornecedorId);
    }
}
