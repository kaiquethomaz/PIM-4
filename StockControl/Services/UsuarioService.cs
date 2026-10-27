using StockControl.DAO;
using StockControl.Excecoes;
using StockControl.Models;

namespace StockControl.Services;

public class UsuarioService : IUsuarioService
{
    private const int TamanhoMinimoSenha = 6;

    private readonly IUsuarioDAO _usuarioDAO;

    public UsuarioService(IUsuarioDAO usuarioDAO)
    {
        _usuarioDAO = usuarioDAO;
    }

    public Task<List<Usuario>> ListarAsync() => _usuarioDAO.ListarTodosAsync();

    public Task<Usuario?> ObterAsync(int id) => _usuarioDAO.BuscarPorIdAsync(id);

    public async Task<Usuario?> AutenticarAsync(string email, string senha)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrEmpty(senha))
            return null;

        var usuario = await _usuarioDAO.BuscarPorEmailAsync(NormalizarEmail(email));
        if (usuario is null || !usuario.Ativo)
            return null;

        // BCrypt.Verify gera o hash da senha digitada e compara com o hash salvo.
        // A senha original nunca é "descriptografada" (hash não tem volta).
        return BCrypt.Net.BCrypt.Verify(senha, usuario.SenhaHash) ? usuario : null;
    }

    public async Task<Usuario> CriarAsync(Usuario usuario, string senha)
    {
        await ValidarAsync(usuario);
        ValidarSenha(senha);

        usuario.SenhaHash = BCrypt.Net.BCrypt.HashPassword(senha);
        usuario.CriadoEm = DateTime.Now;

        await _usuarioDAO.InserirAsync(usuario);
        return usuario;
    }

    public async Task AtualizarAsync(Usuario usuario, string? novaSenha, int idUsuarioLogado)
    {
        var existente = await _usuarioDAO.BuscarPorIdAsync(usuario.Id)
            ?? throw new RegraNegocioException("Usuário não encontrado.");

        await ValidarAsync(usuario);

        // Protege o sistema de ficar sem administrador.
        bool deixaDeSerAdminAtivo = existente.EhAdmin() && existente.Ativo
                                    && (!usuario.EhAdmin() || !usuario.Ativo);
        if (deixaDeSerAdminAtivo && await _usuarioDAO.ContarAdminsAtivosAsync() <= 1)
            throw new RegraNegocioException("O sistema precisa ter pelo menos um administrador ativo.");

        if (usuario.Id == idUsuarioLogado && !usuario.Ativo)
            throw new RegraNegocioException("Você não pode desativar o seu próprio usuário.");

        existente.Nome = usuario.Nome;
        existente.Email = usuario.Email;
        existente.Perfil = usuario.Perfil;
        existente.Ativo = usuario.Ativo;

        if (!string.IsNullOrEmpty(novaSenha))
        {
            ValidarSenha(novaSenha);
            existente.SenhaHash = BCrypt.Net.BCrypt.HashPassword(novaSenha);
        }

        await _usuarioDAO.AtualizarAsync(existente);
    }

    private async Task ValidarAsync(Usuario usuario)
    {
        usuario.Nome = (usuario.Nome ?? string.Empty).Trim();
        usuario.Email = NormalizarEmail(usuario.Email);

        if (usuario.Nome.Length == 0)
            throw new RegraNegocioException("Informe o nome.");

        if (!usuario.Email.Contains('@'))
            throw new RegraNegocioException("Informe um e-mail válido.");

        if (await _usuarioDAO.EmailExisteAsync(usuario.Email, usuario.Id))
            throw new RegraNegocioException("Já existe um usuário com este e-mail.");
    }

    private static void ValidarSenha(string? senha)
    {
        if (string.IsNullOrEmpty(senha) || senha.Length < TamanhoMinimoSenha)
            throw new RegraNegocioException($"A senha deve ter pelo menos {TamanhoMinimoSenha} caracteres.");
    }

    // E-mail sempre minúsculo e sem espaços, para "Admin@X.com" e "admin@x.com" serem o mesmo login.
    private static string NormalizarEmail(string? email) => (email ?? string.Empty).Trim().ToLower();
}
