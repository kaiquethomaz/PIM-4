using Microsoft.EntityFrameworkCore;
using StockControl.Data;
using StockControl.Models;

namespace StockControl.DAO;

public class LogAuditoriaDAO : ILogAuditoriaDAO
{
    private readonly AppDbContext _contexto;

    public LogAuditoriaDAO(AppDbContext contexto)
    {
        _contexto = contexto;
    }

    public async Task<List<LogAuditoria>> ListarRecentesAsync(int quantidade)
    {
        // O filtro automático do AppDbContext já limita ao estabelecimento logado.
        return await _contexto.LogsAuditoria.AsNoTracking()
            .OrderByDescending(l => l.Data)
            .ThenByDescending(l => l.Id)
            .Take(quantidade)
            .ToListAsync();
    }

    public async Task<Dictionary<int, string>> BuscarNomesDeProdutosAsync(IEnumerable<int> ids)
    {
        var lista = ids.Distinct().ToList();
        return await _contexto.Produtos.AsNoTracking()
            .Where(p => lista.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, p => p.Nome);
    }
}
