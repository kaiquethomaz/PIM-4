using StockControl.Models;

namespace StockControl.Services;

/// <summary>Regras de negócio de Usuário e de login.</summary>
public interface IUsuarioService
{
    Task<List<Usuario>> ListarAsync();
    Task<Usuario?> ObterAsync(int id);

    /// <summary>Confere e-mail e senha. Retorna null se estiverem errados ou se o usuário estiver inativo.</summary>
    Task<Usuario?> AutenticarAsync(string email, string senha);

    Task<Usuario> CriarAsync(Usuario usuario, string senha);

    /// <summary>Atualiza os dados. Se novaSenha vier vazia, a senha atual é mantida.</summary>
    Task AtualizarAsync(Usuario usuario, string? novaSenha, int idUsuarioLogado);
}
