using System.Security.Claims;
using StockControl.Models;

namespace StockControl.Extensions;

/// <summary>
/// Nomes dos "claims" próprios do StockControl (informações gravadas no
/// cookie do site e no token JWT da API) e a montagem da identidade do usuário.
/// </summary>
public static class ClaimsStockControl
{
    public const string EstabelecimentoId = "EstabelecimentoId";
    public const string Estabelecimento = "Estabelecimento";

    /// <summary>Cria a lista de claims do usuário logado (usada no cookie do site).</summary>
    public static List<Claim> CriarClaims(Usuario usuario, string nomeEstabelecimento) => new()
    {
        new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
        new(ClaimTypes.Name, usuario.Nome),
        new(ClaimTypes.Email, usuario.Email),
        new(ClaimTypes.Role, usuario.Perfil.ToString()),          // usado no [Authorize(Roles = ...)]
        new(EstabelecimentoId, usuario.EstabelecimentoId.ToString()),  // isola os dados de cada mercadinho
        new(Estabelecimento, nomeEstabelecimento)                  // nome mostrado no cabeçalho
    };
}
