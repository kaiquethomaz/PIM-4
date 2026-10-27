using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using StockControl.Dtos;
using StockControl.Excecoes;

namespace StockControl.Controllers.Api;

/// <summary>
/// Filtro de exceção da API: quando um Service lança RegraNegocioException
/// (ex.: "Estoque insuficiente"), devolve HTTP 400 com { "mensagem": "..." }.
/// Assim não é preciso escrever try/catch em todos os endpoints.
/// </summary>
public class TratarRegraNegocioAttribute : ExceptionFilterAttribute
{
    public override void OnException(ExceptionContext contexto)
    {
        if (contexto.Exception is RegraNegocioException erro)
        {
            contexto.Result = new BadRequestObjectResult(new MensagemResposta(erro.Message));
            contexto.ExceptionHandled = true;
        }
    }
}
