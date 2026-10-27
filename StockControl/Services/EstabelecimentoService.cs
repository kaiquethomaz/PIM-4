using StockControl.DAO;
using StockControl.Excecoes;
using StockControl.Models;

namespace StockControl.Services;

public class EstabelecimentoService : IEstabelecimentoService
{
    // Categorias criadas automaticamente para todo mercadinho novo,
    // para ele não começar com o sistema "vazio".
    private static readonly string[] CategoriasIniciais =
        { "Mercearia", "Bebidas", "Laticínios e Frios", "Limpeza", "Higiene Pessoal", "Hortifrúti" };

    private readonly IEstabelecimentoDAO _estabelecimentoDAO;
    private readonly IUsuarioDAO _usuarioDAO;
    private readonly IUsuarioAtual _usuarioAtual;

    public EstabelecimentoService(IEstabelecimentoDAO estabelecimentoDAO, IUsuarioDAO usuarioDAO, IUsuarioAtual usuarioAtual)
    {
        _estabelecimentoDAO = estabelecimentoDAO;
        _usuarioDAO = usuarioDAO;
        _usuarioAtual = usuarioAtual;
    }

    public async Task<Usuario> CadastrarAsync(Estabelecimento estabelecimento, Usuario dono, string senha)
    {
        // ---- Dados do estabelecimento ----
        estabelecimento.NomeFantasia = (estabelecimento.NomeFantasia ?? string.Empty).Trim();
        estabelecimento.CNPJ = Cnpj.Limpar(estabelecimento.CNPJ);
        estabelecimento.Telefone = string.IsNullOrWhiteSpace(estabelecimento.Telefone) ? null : estabelecimento.Telefone.Trim();
        estabelecimento.Email = string.IsNullOrWhiteSpace(estabelecimento.Email) ? null : estabelecimento.Email.Trim().ToLower();
        estabelecimento.Ativo = true;
        estabelecimento.CriadoEm = DateTime.Now;

        if (estabelecimento.NomeFantasia.Length == 0)
            throw new RegraNegocioException("Informe o nome do estabelecimento.");

        if (!Cnpj.EhValido(estabelecimento.CNPJ))
            throw new RegraNegocioException("CNPJ do estabelecimento inválido.");

        if (await _estabelecimentoDAO.CnpjExisteAsync(estabelecimento.CNPJ))
            throw new RegraNegocioException("Este CNPJ já está cadastrado. Peça ao administrador do estabelecimento para criar o seu usuário.");

        // ---- Dados do dono (primeiro usuário, sempre Admin) ----
        dono.Nome = (dono.Nome ?? string.Empty).Trim();
        dono.Email = (dono.Email ?? string.Empty).Trim().ToLower();
        dono.Perfil = PerfilUsuario.Admin;
        dono.Ativo = true;
        dono.CriadoEm = DateTime.Now;

        if (dono.Nome.Length == 0)
            throw new RegraNegocioException("Informe o seu nome.");

        if (!dono.Email.Contains('@'))
            throw new RegraNegocioException("Informe um e-mail válido.");

        if (await _usuarioDAO.EmailExisteAsync(dono.Email))
            throw new RegraNegocioException("Este e-mail já está em uso. Use outro e-mail ou entre com a sua conta.");

        if (string.IsNullOrEmpty(senha) || senha.Length < UsuarioService.TamanhoMinimoSenha)
            throw new RegraNegocioException($"A senha deve ter pelo menos {UsuarioService.TamanhoMinimoSenha} caracteres.");

        dono.SenhaHash = BCrypt.Net.BCrypt.HashPassword(senha);

        var categorias = CategoriasIniciais.Select(nome => new Categoria { Nome = nome }).ToList();

        // Grava tudo em uma transação (estabelecimento + dono + categorias).
        await _estabelecimentoDAO.CadastrarComAdministradorAsync(estabelecimento, dono, categorias);

        dono.Estabelecimento = estabelecimento;
        return dono;
    }

    public async Task<Estabelecimento?> ObterAtualAsync()
    {
        if (_usuarioAtual.EstabelecimentoId is not int id)
            return null;

        // O filtro automático garante que só o próprio estabelecimento é encontrado.
        return await _estabelecimentoDAO.BuscarPorIdAsync(id);
    }

    public async Task AtualizarAsync(Estabelecimento estabelecimento)
    {
        var existente = await ObterAtualAsync()
            ?? throw new RegraNegocioException("Estabelecimento não encontrado.");

        var nome = (estabelecimento.NomeFantasia ?? string.Empty).Trim();
        if (nome.Length == 0)
            throw new RegraNegocioException("Informe o nome do estabelecimento.");

        existente.NomeFantasia = nome;
        existente.Telefone = string.IsNullOrWhiteSpace(estabelecimento.Telefone) ? null : estabelecimento.Telefone.Trim();
        existente.Email = string.IsNullOrWhiteSpace(estabelecimento.Email) ? null : estabelecimento.Email.Trim().ToLower();

        await _estabelecimentoDAO.AtualizarAsync(existente);
    }
}
