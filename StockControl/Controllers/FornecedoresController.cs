using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockControl.Excecoes;
using StockControl.Services;
using StockControl.ViewModels;

namespace StockControl.Controllers;

/// <summary>CRUD de fornecedores. Só Admin e Gerente acessam.</summary>
[Authorize(Roles = "Admin,Gerente")]
public class FornecedoresController : Controller
{
    private readonly IFornecedorService _fornecedorService;

    public FornecedoresController(IFornecedorService fornecedorService)
    {
        _fornecedorService = fornecedorService;
    }

    public async Task<IActionResult> Index()
    {
        var fornecedores = await _fornecedorService.ListarAsync();
        return View(fornecedores);
    }

    [HttpGet]
    public IActionResult Criar()
    {
        return View("Form", new FornecedorFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Criar(FornecedorFormViewModel modelo)
    {
        if (!ModelState.IsValid)
            return View("Form", modelo);

        try
        {
            await _fornecedorService.CriarAsync(modelo.ParaModelo());
            TempData["Sucesso"] = $"Fornecedor \"{modelo.RazaoSocial}\" cadastrado.";
            return RedirectToAction(nameof(Index));
        }
        catch (RegraNegocioException erro)
        {
            ModelState.AddModelError(string.Empty, erro.Message);
            return View("Form", modelo);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Editar(int id)
    {
        var fornecedor = await _fornecedorService.ObterAsync(id);
        if (fornecedor is null)
            return NotFound();

        return View("Form", FornecedorFormViewModel.DeModelo(fornecedor));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, FornecedorFormViewModel modelo)
    {
        modelo.Id = id;
        if (!ModelState.IsValid)
            return View("Form", modelo);

        try
        {
            await _fornecedorService.AtualizarAsync(modelo.ParaModelo());
            TempData["Sucesso"] = $"Fornecedor \"{modelo.RazaoSocial}\" atualizado.";
            return RedirectToAction(nameof(Index));
        }
        catch (RegraNegocioException erro)
        {
            ModelState.AddModelError(string.Empty, erro.Message);
            return View("Form", modelo);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Excluir(int id)
    {
        var fornecedor = await _fornecedorService.ObterAsync(id);
        if (fornecedor is null)
            return NotFound();

        return View(fornecedor);
    }

    [HttpPost, ActionName("Excluir")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ExcluirConfirmado(int id)
    {
        try
        {
            await _fornecedorService.ExcluirAsync(id);
            TempData["Sucesso"] = "Fornecedor excluído.";
        }
        catch (RegraNegocioException erro)
        {
            TempData["Erro"] = erro.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}
