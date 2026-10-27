using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockControl.Excecoes;
using StockControl.Services;
using StockControl.ViewModels;

namespace StockControl.Controllers;

/// <summary>CRUD de categorias. Só Admin e Gerente acessam.</summary>
[Authorize(Roles = "Admin,Gerente")]
public class CategoriasController : Controller
{
    private readonly ICategoriaService _categoriaService;

    public CategoriasController(ICategoriaService categoriaService)
    {
        _categoriaService = categoriaService;
    }

    // GET /Categorias
    public async Task<IActionResult> Index()
    {
        var categorias = await _categoriaService.ListarAsync();
        return View(categorias);
    }

    // GET /Categorias/Criar
    [HttpGet]
    public IActionResult Criar()
    {
        return View("Form", new CategoriaFormViewModel());
    }

    // POST /Categorias/Criar
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Criar(CategoriaFormViewModel modelo)
    {
        // 1º valida as DataAnnotations do ViewModel (campos obrigatórios, tamanho...)
        if (!ModelState.IsValid)
            return View("Form", modelo);

        try
        {
            // 2º o Service aplica as regras de negócio (ex.: nome repetido)
            await _categoriaService.CriarAsync(modelo.ParaModelo());
            TempData["Sucesso"] = $"Categoria \"{modelo.Nome}\" cadastrada.";
            return RedirectToAction(nameof(Index));
        }
        catch (RegraNegocioException erro)
        {
            ModelState.AddModelError(string.Empty, erro.Message);
            return View("Form", modelo);
        }
    }

    // GET /Categorias/Editar/5
    [HttpGet]
    public async Task<IActionResult> Editar(int id)
    {
        var categoria = await _categoriaService.ObterAsync(id);
        if (categoria is null)
            return NotFound();

        return View("Form", CategoriaFormViewModel.DeModelo(categoria));
    }

    // POST /Categorias/Editar/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, CategoriaFormViewModel modelo)
    {
        modelo.Id = id;
        if (!ModelState.IsValid)
            return View("Form", modelo);

        try
        {
            await _categoriaService.AtualizarAsync(modelo.ParaModelo());
            TempData["Sucesso"] = $"Categoria \"{modelo.Nome}\" atualizada.";
            return RedirectToAction(nameof(Index));
        }
        catch (RegraNegocioException erro)
        {
            ModelState.AddModelError(string.Empty, erro.Message);
            return View("Form", modelo);
        }
    }

    // GET /Categorias/Excluir/5  -> página de confirmação
    [HttpGet]
    public async Task<IActionResult> Excluir(int id)
    {
        var categoria = await _categoriaService.ObterAsync(id);
        if (categoria is null)
            return NotFound();

        return View(categoria);
    }

    // POST /Categorias/Excluir/5  -> exclui de verdade
    [HttpPost, ActionName("Excluir")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ExcluirConfirmado(int id)
    {
        try
        {
            await _categoriaService.ExcluirAsync(id);
            TempData["Sucesso"] = "Categoria excluída.";
        }
        catch (RegraNegocioException erro)
        {
            TempData["Erro"] = erro.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}
