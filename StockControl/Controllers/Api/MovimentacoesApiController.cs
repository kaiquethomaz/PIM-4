using Microsoft.AspNetCore.Mvc;
using StockControl.Dtos;
using StockControl.Extensions;
using StockControl.Services;

namespace StockControl.Controllers.Api;

/// <summary>Entradas e vendas (saídas) de estoque. Todos os perfis podem registrar.</summary>
[Route("api/movimentacoes")]
public class MovimentacoesApiController : ApiControllerBase
{
    private readonly IMovimentacaoService _movimentacaoService;
    private readonly IProdutoService _produtoService;

    public MovimentacoesApiController(IMovimentacaoService movimentacaoService, IProdutoService produtoService)
    {
        _movimentacaoService = movimentacaoService;
        _produtoService = produtoService;
    }

    /// <summary>Registra uma entrada ou uma venda (registro rápido pelo app).</summary>
    /// <remarks>
    /// Informe o `produtoId` **ou** o `codigoBarras`. Exemplo de venda:
    ///
    ///     { "codigoBarras": "7891000000011", "tipo": "SAIDA", "quantidade": 2 }
    ///
    /// A resposta traz o produto com o estoque já atualizado.
    /// </remarks>
    /// <response code="201">Movimentação registrada.</response>
    /// <response code="400">Dados inválidos ou estoque insuficiente.</response>
    [HttpPost]
    [ProducesResponseType(typeof(MovimentacaoRegistradaDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(MensagemResposta), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MovimentacaoRegistradaDto>> Registrar(MovimentacaoRequisicao requisicao)
    {
        // Descobre o produto pelo Id ou pelo código de barras.
        int produtoId;
        if (requisicao.ProdutoId is > 0)
        {
            produtoId = requisicao.ProdutoId.Value;
        }
        else if (!string.IsNullOrWhiteSpace(requisicao.CodigoBarras))
        {
            var encontrado = await _produtoService.ObterPorCodigoBarrasAsync(requisicao.CodigoBarras);
            if (encontrado is null)
                return NotFound(new MensagemResposta($"Nenhum produto com o código de barras {requisicao.CodigoBarras}."));
            produtoId = encontrado.Id;
        }
        else
        {
            return BadRequest(new MensagemResposta("Informe o produtoId ou o codigoBarras."));
        }

        // Mesmo Service usado pelo sistema web: as regras (ex.: estoque insuficiente) são as mesmas.
        var movimentacao = await _movimentacaoService.RegistrarAsync(
            produtoId,
            requisicao.TipoConvertido(),
            requisicao.Quantidade,
            User.ObterUsuarioId(),
            requisicao.Observacao);

        var produto = await _produtoService.ObterAsync(produtoId);
        var resposta = new MovimentacaoRegistradaDto
        {
            Movimentacao = MovimentacaoDto.DeModelo(movimentacao),
            Produto = ProdutoDto.DeModelo(produto!)
        };
        resposta.Movimentacao.Produto = produto!.Nome;

        return StatusCode(StatusCodes.Status201Created, resposta);
    }

    /// <summary>Lista as movimentações de um período (os dois dias inclusos).</summary>
    /// <remarks>Sem datas, devolve os últimos 7 dias. Formato: `2026-10-01`.</remarks>
    [HttpGet]
    [ProducesResponseType(typeof(List<MovimentacaoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MensagemResposta), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<MovimentacaoDto>>> Listar(DateTime? dataInicio, DateTime? dataFim)
    {
        var fim = dataFim ?? DateTime.Today;
        var inicio = dataInicio ?? fim.AddDays(-7);

        var lista = await _movimentacaoService.ListarPorPeriodoAsync(inicio, fim);
        return Ok(lista.Select(MovimentacaoDto.DeRelatorio).ToList());
    }

    /// <summary>Últimas movimentações registradas.</summary>
    [HttpGet("ultimas")]
    [ProducesResponseType(typeof(List<MovimentacaoDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<MovimentacaoDto>>> Ultimas(int quantidade = 10)
    {
        quantidade = Math.Clamp(quantidade, 1, 100);
        var lista = await _movimentacaoService.ListarUltimasAsync(quantidade);
        return Ok(lista.Select(MovimentacaoDto.DeModelo).ToList());
    }
}
