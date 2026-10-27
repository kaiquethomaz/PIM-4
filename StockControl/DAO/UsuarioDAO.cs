using Microsoft.EntityFrameworkCore;
using StockControl.Data;
using StockControl.Models;

namespace StockControl.DAO;

public class UsuarioDAO : DAOBase<Usuario>, IUsuarioDAO
{
    public UsuarioDAO(AppDbContext contexto) : base(contexto)
    {
    }

    public override async Task<List<Usuario>> ListarTodosAsync()
    {
        return await _tabela.AsNoTracking().OrderBy(u => u.Nome).ToListAsync();
    }

    public override async Task<Usuario?> BuscarPorIdAsync(int id)
    {
        return await _tabela.Include(u => u.Estabelecimento).FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<Usuario?> BuscarPorEmailAsync(string email)
    {
        // Usado no LOGIN: ainda não sabemos o estabelecimento (ninguém está logado),
        // então ignoramos o filtro automático e buscamos no sistema inteiro.
        return await _tabela
            .IgnoreQueryFilters()
            .Include(u => u.Estabelecimento)
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<bool> EmailExisteAsync(string email, int? ignorarId = null)
    {
        // O e-mail é o login, então precisa ser único no sistema inteiro
        // (não só no estabelecimento): ignora o filtro automático.
        return await _tabela.IgnoreQueryFilters().AnyAsync(u => u.Email == email && u.Id != ignorarId);
    }

    public async Task<int> ContarAdminsAtivosAsync()
    {
        return await _tabela.CountAsync(u => u.Perfil == PerfilUsuario.Admin && u.Ativo);
    }

    public async Task<bool> PossuiMovimentacoesAsync(int usuarioId)
    {
        return await _contexto.Movimentacoes.AnyAsync(m => m.UsuarioId == usuarioId);
    }
}
