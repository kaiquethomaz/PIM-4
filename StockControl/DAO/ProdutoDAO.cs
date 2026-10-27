using Microsoft.EntityFrameworkCore;
using StockControl.Data;
using StockControl.Models;
using StockControl.Models.Consultas;

namespace StockControl.DAO;

public class ProdutoDAO : DAOBase<Produto>, IProdutoDAO
{
    public ProdutoDAO(AppDbContext contexto) : base(contexto)
    {
    }

    // Include = traz junto a categoria e o fornecedor (JOIN), para mostrar os nomes na tela.
    public override async Task<List<Produto>> ListarTodosAsync()
    {
        return await _tabela.AsNoTracking()
            .Include(p => p.Categoria)
            .Include(p => p.Fornecedor)
            .OrderBy(p => p.Nome)
            .ToListAsync();
    }

    public override async Task<Produto?> BuscarPorIdAsync(int id)
    {
        return await _tabela
            .Include(p => p.Categoria)
            .Include(p => p.Fornecedor)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<List<Produto>> BuscarAsync(string? termo, bool somenteAtivos = true)
    {
        // IQueryable: a consulta vai sendo montada aos poucos e só é
        // executada no banco no ToListAsync().
        var consulta = _tabela.AsNoTracking()
            .Include(p => p.Categoria)
            .Include(p => p.Fornecedor)
            .AsQueryable();

        if (somenteAtivos)
            consulta = consulta.Where(p => p.Ativo);

        if (!string.IsNullOrWhiteSpace(termo))
        {
            termo = termo.Trim();
            consulta = consulta.Where(p => p.Nome.Contains(termo) || p.CodigoBarras == termo);
        }

        return await consulta.OrderBy(p => p.Nome).ToListAsync();
    }

    public async Task<Produto?> BuscarPorCodigoBarrasAsync(string codigoBarras)
    {
        return await _tabela.AsNoTracking()
            .Include(p => p.Categoria)
            .Include(p => p.Fornecedor)
            .FirstOrDefaultAsync(p => p.CodigoBarras == codigoBarras);
    }

    public async Task<List<Produto>> ListarAtualizadosDesdeAsync(DateTime desde)
    {
        // Inclui os inativos: o app precisa saber que um produto foi desativado.
        return await _tabela.AsNoTracking()
            .Include(p => p.Categoria)
            .Include(p => p.Fornecedor)
            .Where(p => p.AtualizadoEm > desde)
            .OrderBy(p => p.AtualizadoEm)
            .ToListAsync();
    }

    public async Task<bool> CodigoBarrasExisteAsync(string codigoBarras, int? ignorarId = null)
    {
        return await _tabela.AnyAsync(p => p.CodigoBarras == codigoBarras && p.Id != ignorarId);
    }

    public async Task<bool> PossuiMovimentacoesAsync(int produtoId)
    {
        return await _contexto.Movimentacoes.AnyAsync(m => m.ProdutoId == produtoId);
    }

    public async Task<List<ProdutoAbaixoMinimo>> ListarAbaixoDoMinimoAsync()
    {
        // Chama a procedure do banco. O resultado é convertido para a classe
        // ProdutoAbaixoMinimo (as colunas e as propriedades têm o mesmo nome).
        return await _contexto.Database
            .SqlQuery<ProdutoAbaixoMinimo>($"EXEC sp_ProdutosAbaixoDoMinimo")
            .ToListAsync();
    }

    public async Task<int> ContarAtivosAsync()
    {
        return await _tabela.CountAsync(p => p.Ativo);
    }

    public async Task<decimal> CalcularValorEmEstoqueAsync()
    {
        return await _tabela.Where(p => p.Ativo).SumAsync(p => p.Preco * p.EstoqueAtual);
    }
}
