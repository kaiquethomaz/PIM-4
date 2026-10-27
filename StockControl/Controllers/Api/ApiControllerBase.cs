using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace StockControl.Controllers.Api;

/// <summary>
/// Classe base de todos os controllers da API (herança).
/// Tudo que vale para a API inteira fica aqui, uma vez só:
///  - [ApiController]: valida o JSON recebido automaticamente (devolve 400 se inválido);
///  - [Authorize(JWT)]: exige o token JWT no cabeçalho Authorization (e NÃO o cookie do site);
///  - [EnableCors]: permite chamadas do app mobile rodando em outro endereço;
///  - [TratarRegraNegocio]: converte erros de regra de negócio em HTTP 400.
/// </summary>
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[EnableCors("Mobile")]
[TratarRegraNegocio]
[Produces("application/json")]
public abstract class ApiControllerBase : ControllerBase
{
    protected const string PerfisGestao = "Admin,Gerente";
}
