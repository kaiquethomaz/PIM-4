namespace StockControl.Services;

/// <summary>
/// Informa de qual estabelecimento é o usuário logado na requisição atual
/// (lido do cookie, no site, ou do token JWT, na API).
/// </summary>
public interface IUsuarioAtual
{
    /// <summary>Null quando ninguém está logado (ex.: tela de login).</summary>
    int? EstabelecimentoId { get; }
}
