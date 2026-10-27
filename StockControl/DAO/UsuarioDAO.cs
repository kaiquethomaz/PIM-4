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

    public async Task<Usuario?> BuscarPorEmailAsync(string email)
    {
        return await _tabela.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<bool> EmailExisteAsync(string email, int? ignorarId = null)
    {
        return await _tabela.AnyAsync(u => u.Email == email && u.Id != ignorarId);
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
