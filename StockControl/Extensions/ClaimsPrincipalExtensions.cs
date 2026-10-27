using System.Security.Claims;

namespace StockControl.Extensions;

/// <summary>
/// Métodos de extensão para ler os dados do usuário logado
/// (os "claims" gravados no cookie ou no token JWT).
/// Uso: User.ObterUsuarioId()
/// </summary>
public static class ClaimsPrincipalExtensions
{
    public static int ObterUsuarioId(this ClaimsPrincipal usuario)
    {
        // No cookie (web) o Id fica em NameIdentifier; no token JWT (API) fica em "sub".
        var valor = usuario.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? usuario.FindFirstValue("sub");
        return int.TryParse(valor, out var id) ? id : 0;
    }
}
