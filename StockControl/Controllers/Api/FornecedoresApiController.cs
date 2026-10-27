using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockControl.Dtos;
using StockControl.Services;

namespace StockControl.Controllers.Api;

/// <summary>Fornecedores (consulta para todos; alterações para Admin e Gerente).</summary>
[Route("api/fornecedores")]
public class FornecedoresApiController : ApiControllerBase
{
    private readonly IFornecedorService _fornecedorService;

    public FornecedoresApiController(IFornecedorService fornecedorService)
    {
        _fornecedorService = fornecedorService;
    }

    /// <summary>Lista os fornecedores.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<FornecedorDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<FornecedorDto>>> Listar()
    {
        var fornecedores = await _fornecedorService.ListarAsync();
        return Ok(fornecedores.Select(FornecedorDto.DeModelo).ToList());
    }

    /// <summary>Busca um fornecedor pelo Id.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(FornecedorDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MensagemResposta), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FornecedorDto>> Obter(int id)
    {
        var fornecedor = await _fornecedorService.ObterAsync(id);
        if (fornecedor is null)
            return NotFound(new MensagemResposta("Fornecedor não encontrado."));

        return Ok(FornecedorDto.DeModelo(fornecedor));
    }

    /// <summary>Cadastra um fornecedor (Admin e Gerente). O CNPJ pode vir com ou sem máscara.</summary>
    [HttpPost]
    [Authorize(Roles = PerfisGestao)]
    [ProducesResponseType(typeof(FornecedorDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(MensagemResposta), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<FornecedorDto>> Criar(FornecedorRequisicao requisicao)
    {
        var fornecedor = await _fornecedorService.CriarAsync(requisicao.ParaModelo());
        return CreatedAtAction(nameof(Obter), new { id = fornecedor.Id }, FornecedorDto.DeModelo(fornecedor));
    }

    /// <summary>Altera um fornecedor (Admin e Gerente).</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = PerfisGestao)]
    [ProducesResponseType(typeof(FornecedorDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MensagemResposta), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<FornecedorDto>> Alterar(int id, FornecedorRequisicao requisicao)
    {
        await _fornecedorService.AtualizarAsync(requisicao.ParaModelo(id));
        var fornecedor = await _fornecedorService.ObterAsync(id);
        return Ok(FornecedorDto.DeModelo(fornecedor!));
    }

    /// <summary>Exclui um fornecedor (Admin e Gerente). Não é permitido se houver produtos dele.</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = PerfisGestao)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(MensagemResposta), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Excluir(int id)
    {
        await _fornecedorService.ExcluirAsync(id);
        return NoContent();
    }
}
