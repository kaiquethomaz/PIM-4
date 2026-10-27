using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockControl.Dtos;
using StockControl.Extensions;
using StockControl.Services;

namespace StockControl.Controllers.Api;

/// <summary>Produtos (consulta para todos; cadastro e alteração para Admin e Gerente).</summary>
[Route("api/produtos")]
public class ProdutosApiController : ApiControllerBase
{
    private readonly IProdutoService _produtoService;

    public ProdutosApiController(IProdutoService produtoService)
    {
        _produtoService = produtoService;
    }

    /// <summary>Lista os produtos.</summary>
    /// <remarks>
    /// - Sem parâmetros: todos os produtos **ativos**.
    /// - `busca`: filtra por parte do nome ou código de barras exato.
    /// - `atualizadoDesde`: **sincronização** — devolve só os produtos alterados depois dessa data
    ///   (inclusive os desativados, para o app removê-los). Ex.: `2026-10-27T10:00:00`.
    ///   Na próxima sincronização, use o maior `atualizadoEm` recebido.
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(typeof(List<ProdutoDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ProdutoDto>>> Listar(string? busca, DateTime? atualizadoDesde)
    {
        var produtos = atualizadoDesde.HasValue
            ? await _produtoService.ListarAtualizadosDesdeAsync(atualizadoDesde.Value)
            : await _produtoService.ListarAsync(busca);

        return Ok(produtos.Select(ProdutoDto.DeModelo).ToList());
    }

    /// <summary>Busca um produto pelo Id.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProdutoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MensagemResposta), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProdutoDto>> Obter(int id)
    {
        var produto = await _produtoService.ObterAsync(id);
        if (produto is null)
            return NotFound(new MensagemResposta("Produto não encontrado."));

        return Ok(ProdutoDto.DeModelo(produto));
    }

    /// <summary>Busca um produto pelo código de barras (leitura pela câmera do celular).</summary>
    [HttpGet("codigo-barras/{codigo}")]
    [ProducesResponseType(typeof(ProdutoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MensagemResposta), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProdutoDto>> ObterPorCodigoBarras(string codigo)
    {
        var produto = await _produtoService.ObterPorCodigoBarrasAsync(codigo);
        if (produto is null)
            return NotFound(new MensagemResposta($"Nenhum produto com o código de barras {codigo}."));

        return Ok(ProdutoDto.DeModelo(produto));
    }

    /// <summary>Cadastra um produto (Admin e Gerente).</summary>
    /// <response code="201">Produto criado.</response>
    /// <response code="400">Dados inválidos ou regra de negócio (ex.: código de barras repetido).</response>
    /// <response code="403">Perfil sem permissão.</response>
    [HttpPost]
    [Authorize(Roles = PerfisGestao)]
    [ProducesResponseType(typeof(ProdutoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(MensagemResposta), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProdutoDto>> Criar(ProdutoRequisicao requisicao)
    {
        var criado = await _produtoService.CriarAsync(
            requisicao.ParaModelo(), requisicao.EstoqueInicial, User.ObterUsuarioId());

        // Busca de novo para devolver o estoque já atualizado pela trigger e os nomes de categoria/fornecedor.
        var produto = await _produtoService.ObterAsync(criado.Id);

        // 201 Created + cabeçalho Location apontando para GET /api/produtos/{id}
        return CreatedAtAction(nameof(Obter), new { id = criado.Id }, ProdutoDto.DeModelo(produto!));
    }

    /// <summary>Altera um produto (Admin e Gerente). O estoque não é alterado aqui (só por movimentação).</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = PerfisGestao)]
    [ProducesResponseType(typeof(ProdutoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MensagemResposta), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProdutoDto>> Alterar(int id, ProdutoRequisicao requisicao)
    {
        await _produtoService.AtualizarAsync(requisicao.ParaModelo(id));
        var produto = await _produtoService.ObterAsync(id);
        return Ok(ProdutoDto.DeModelo(produto!));
    }

    /// <summary>Exclui um produto (Admin e Gerente). Se ele tiver movimentações, é apenas desativado.</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = PerfisGestao)]
    [ProducesResponseType(typeof(MensagemResposta), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MensagemResposta), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MensagemResposta>> Excluir(int id)
    {
        var mensagem = await _produtoService.ExcluirAsync(id);
        return Ok(new MensagemResposta(mensagem));
    }
}
