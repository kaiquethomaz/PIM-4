using Microsoft.EntityFrameworkCore;
using StockControl.Data;
using StockControl.Models;

namespace StockControl.DAO;

/// <summary>
/// Implementação genérica do CRUD usando o AppDbContext.
/// É "abstract": não é usada sozinha, só serve de base (herança) para
/// ProdutoDAO, CategoriaDAO, FornecedorDAO e UsuarioDAO.
/// Os métodos são "virtual" para que um DAO filho possa sobrescrever (override)
/// quando precisar de algo diferente (ex.: listar produtos junto com a categoria).
/// </summary>
public abstract class DAOBase<T> : IDAOBase<T> where T : EntidadeBase
{
    // "protected": os DAOs filhos também enxergam o contexto e a tabela.
    protected readonly AppDbContext _contexto;
    protected readonly DbSet<T> _tabela;

    protected DAOBase(AppDbContext contexto)
    {
        _contexto = contexto;
        _tabela = contexto.Set<T>();
    }

    public virtual async Task<List<T>> ListarTodosAsync()
    {
        // AsNoTracking: só leitura, o EF não precisa "vigiar" os objetos (mais rápido).
        return await _tabela.AsNoTracking().OrderBy(e => e.Id).ToListAsync();
    }

    public virtual async Task<T?> BuscarPorIdAsync(int id)
    {
        return await _tabela.FirstOrDefaultAsync(e => e.Id == id);
    }

    public virtual async Task<int> ContarAsync()
    {
        return await _tabela.CountAsync();   // SELECT COUNT(*)
    }

    public virtual async Task InserirAsync(T entidade)
    {
        entidade.MarcarComoAtualizado();
        _tabela.Add(entidade);
        await _contexto.SaveChangesAsync();   // INSERT no banco; o Id gerado volta para o objeto
    }

    public virtual async Task AtualizarAsync(T entidade)
    {
        entidade.MarcarComoAtualizado();

        // Se o objeto veio do BuscarPorIdAsync, o EF já está "vigiando" ele e sabe
        // o que mudou. Só chamamos Update() para objetos que o EF não conhece
        // (Detached), senão ele marcaria como alterados também os objetos ligados
        // (ex.: a Categoria do Produto) e faria UPDATEs desnecessários.
        if (_contexto.Entry(entidade).State == EntityState.Detached)
            _tabela.Update(entidade);

        await _contexto.SaveChangesAsync();   // UPDATE no banco
    }

    public virtual async Task ExcluirAsync(T entidade)
    {
        _tabela.Remove(entidade);
        await _contexto.SaveChangesAsync();   // DELETE no banco
    }
}
