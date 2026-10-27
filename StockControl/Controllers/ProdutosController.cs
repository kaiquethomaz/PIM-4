using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using StockControl.Excecoes;
using StockControl.Extensions;
using StockControl.Services;
using StockControl.ViewModels;

namespace StockControl.Controllers;

/// <summary>
/// Produtos. Todos os usuários logados consultam;
/// só Admin e Gerente cadastram, editam e excluem.
/// </summary>
[Authorize]
public class ProdutosController : Controller
{
    private const string PerfisGestao = "Admin,Gerente";

    private readonly IProdutoService _produtoService;
    private readonly ICategoriaService _categoriaService;
    private readonly IFornecedorService _fornecedorService;

    public ProdutosController(
        IProdutoService produtoService,
        ICategoriaService categoriaService,
        IFornecedorService fornecedorService)
    {
        _produtoService = produtoService;
        _categoriaService = categoriaService;
        _fornecedorService = fornecedorService;
    }

    // GET /Produtos?busca=arroz&mostrarInativos=true
    public async Task<IActionResult> Index(string? busca, bool mostrarInativos = false)
    {
        var podeGerenciar = User.IsInRole("Admin") || User.IsInRole("Gerente");

        // Operador não vê produtos inativos.
        var incluirInativos = mostrarInativos && podeGerenciar;

        var modelo = new ProdutoListaViewModel
        {
            Busca = busca,
            MostrarInativos = incluirInativos,
            PodeGerenciar = podeGerenciar,
            Produtos = await _produtoService.ListarAsync(busca, somenteAtivos: !incluirInativos)
        };

        return View(modelo);
    }

    [HttpGet]
    [Authorize(Roles = PerfisGestao)]
    public async Task<IActionResult> Criar()
    {
        var modelo = new ProdutoFormViewModel();
        await CarregarListasAsync(modelo);
        return View("Form", modelo);
    }

    [HttpPost]
    [Authorize(Roles = PerfisGestao)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Criar(ProdutoFormViewModel modelo)
    {
        if (!ModelState.IsValid)
        {
            await CarregarListasAsync(modelo);
            return View("Form", modelo);
        }

        try
        {
            await _produtoService.CriarAsync(modelo.ParaModelo(), modelo.EstoqueInicial, User.ObterUsuarioId());
            TempData["Sucesso"] = $"Produto \"{modelo.Nome}\" cadastrado.";
            return RedirectToAction(nameof(Index));
        }
        catch (RegraNegocioException erro)
        {
            ModelState.AddModelError(string.Empty, erro.Message);
            await CarregarListasAsync(modelo);
            return View("Form", modelo);
        }
    }

    [HttpGet]
    [Authorize(Roles = PerfisGestao)]
    public async Task<IActionResult> Editar(int id)
    {
        var produto = await _produtoService.ObterAsync(id);
        if (produto is null)
            return NotFound();

        var modelo = ProdutoFormViewModel.DeModelo(produto);
        await CarregarListasAsync(modelo);
        return View("Form", modelo);
    }

    [HttpPost]
    [Authorize(Roles = PerfisGestao)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, ProdutoFormViewModel modelo)
    {
        modelo.Id = id;
        if (!ModelState.IsValid)
        {
            await CarregarListasAsync(modelo);
            return View("Form", modelo);
        }

        try
        {
            await _produtoService.AtualizarAsync(modelo.ParaModelo());
            TempData["Sucesso"] = $"Produto \"{modelo.Nome}\" atualizado.";
            return RedirectToAction(nameof(Index));
        }
        catch (RegraNegocioException erro)
        {
            ModelState.AddModelError(string.Empty, erro.Message);
            await CarregarListasAsync(modelo);
            return View("Form", modelo);
        }
    }

    [HttpGet]
    [Authorize(Roles = PerfisGestao)]
    public async Task<IActionResult> Excluir(int id)
    {
        var produto = await _produtoService.ObterAsync(id);
        if (produto is null)
            return NotFound();

        return View(produto);
    }

    [HttpPost, ActionName("Excluir")]
    [Authorize(Roles = PerfisGestao)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ExcluirConfirmado(int id)
    {
        try
        {
            // O Service decide: exclui de verdade ou só desativa (se já teve movimentações).
            TempData["Sucesso"] = await _produtoService.ExcluirAsync(id);
        }
        catch (RegraNegocioException erro)
        {
            TempData["Erro"] = erro.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    /// <summary>Preenche as opções dos campos Categoria e Fornecedor.</summary>
    private async Task CarregarListasAsync(ProdutoFormViewModel modelo)
    {
        var categorias = await _categoriaService.ListarAsync();
        var fornecedores = await _fornecedorService.ListarAsync();

        modelo.Categorias = categorias
            .Select(c => new SelectListItem(c.Nome, c.Id.ToString()))
            .ToList();

        modelo.Fornecedores = fornecedores
            .Select(f => new SelectListItem(f.RazaoSocial, f.Id.ToString()))
            .ToList();
    }
}
