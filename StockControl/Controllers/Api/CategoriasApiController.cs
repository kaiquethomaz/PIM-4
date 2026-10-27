using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockControl.Dtos;
using StockControl.Services;

namespace StockControl.Controllers.Api;

/// <summary>Categorias (consulta para todos; alterações para Admin e Gerente).</summary>
[Route("api/categorias")]
public class CategoriasApiController : ApiControllerBase
{
    private readonly ICategoriaService _categoriaService;

    public CategoriasApiController(ICategoriaService categoriaService)
    {
        _categoriaService = categoriaService;
    }

    /// <summary>Lista as categorias.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<CategoriaDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<CategoriaDto>>> Listar()
    {
        var categorias = await _categoriaService.ListarAsync();
        return Ok(categorias.Select(CategoriaDto.DeModelo).ToList());
    }

    /// <summary>Busca uma categoria pelo Id.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(CategoriaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MensagemResposta), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CategoriaDto>> Obter(int id)
    {
        var categoria = await _categoriaService.ObterAsync(id);
        if (categoria is null)
            return NotFound(new MensagemResposta("Categoria não encontrada."));

        return Ok(CategoriaDto.DeModelo(categoria));
    }

    /// <summary>Cadastra uma categoria (Admin e Gerente).</summary>
    [HttpPost]
    [Authorize(Roles = PerfisGestao)]
    [ProducesResponseType(typeof(CategoriaDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(MensagemResposta), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CategoriaDto>> Criar(CategoriaRequisicao requisicao)
    {
        var categoria = await _categoriaService.CriarAsync(requisicao.ParaModelo());
        return CreatedAtAction(nameof(Obter), new { id = categoria.Id }, CategoriaDto.DeModelo(categoria));
    }

    /// <summary>Altera uma categoria (Admin e Gerente).</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = PerfisGestao)]
    [ProducesResponseType(typeof(CategoriaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MensagemResposta), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CategoriaDto>> Alterar(int id, CategoriaRequisicao requisicao)
    {
        await _categoriaService.AtualizarAsync(requisicao.ParaModelo(id));
        var categoria = await _categoriaService.ObterAsync(id);
        return Ok(CategoriaDto.DeModelo(categoria!));
    }

    /// <summary>Exclui uma categoria (Admin e Gerente). Não é permitido se houver produtos nela.</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = PerfisGestao)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(MensagemResposta), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Excluir(int id)
    {
        await _categoriaService.ExcluirAsync(id);
        return NoContent();
    }
}
