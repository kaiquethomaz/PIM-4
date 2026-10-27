using StockControl.Models;

namespace StockControl.DAO;

/// <summary>Acesso a dados da tabela Usuario.</summary>
public interface IUsuarioDAO : IDAOBase<Usuario>
{
    Task<Usuario?> BuscarPorEmailAsync(string email);
    Task<bool> EmailExisteAsync(string email, int? ignorarId = null);
    Task<int> ContarAdminsAtivosAsync();
    Task<bool> PossuiMovimentacoesAsync(int usuarioId);
}
