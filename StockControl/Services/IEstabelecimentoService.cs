using StockControl.Models;

namespace StockControl.Services;

/// <summary>Regras de negócio do Estabelecimento (cadastro de um novo mercadinho e seus dados).</summary>
public interface IEstabelecimentoService
{
    /// <summary>
    /// Cadastra um novo estabelecimento junto com o dono (perfil Admin).
    /// Devolve o usuário criado, já pronto para entrar no sistema.
    /// </summary>
    Task<Usuario> CadastrarAsync(Estabelecimento estabelecimento, Usuario dono, string senha);

    /// <summary>Estabelecimento do usuário logado.</summary>
    Task<Estabelecimento?> ObterAtualAsync();

    /// <summary>Altera nome, telefone e e-mail do estabelecimento do usuário logado (o CNPJ não muda).</summary>
    Task AtualizarAsync(Estabelecimento estabelecimento);
}
