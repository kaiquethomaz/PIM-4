using StockControl.Extensions;

namespace StockControl.Services;

public class UsuarioAtual : IUsuarioAtual
{
    // IHttpContextAccessor dá acesso à requisição HTTP atual (e ao usuário logado).
    private readonly IHttpContextAccessor _acessor;

    public UsuarioAtual(IHttpContextAccessor acessor)
    {
        _acessor = acessor;
    }

    public int? EstabelecimentoId
    {
        get
        {
            // É lido na hora em que a consulta roda (e não antes), para já
            // enxergar o usuário do token JWT nas chamadas da API.
            var valor = _acessor.HttpContext?.User.FindFirst(ClaimsStockControl.EstabelecimentoId)?.Value;
            return int.TryParse(valor, out var id) ? id : null;
        }
    }
}
