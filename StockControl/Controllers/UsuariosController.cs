using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockControl.Excecoes;
using StockControl.Extensions;
using StockControl.Services;
using StockControl.ViewModels;

namespace StockControl.Controllers;

/// <summary>
/// Gerenciamento de usuários: a "estrutura administrativa" do sistema.
/// Somente o perfil Admin acessa.
/// </summary>
[Authorize(Roles = "Admin")]
public class UsuariosController : Controller
{
    private readonly IUsuarioService _usuarioService;

    public UsuariosController(IUsuarioService usuarioService)
    {
        _usuarioService = usuarioService;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.UsuarioLogadoId = User.ObterUsuarioId();
        var usuarios = await _usuarioService.ListarAsync();
        return View(usuarios);
    }

    [HttpGet]
    public IActionResult Criar()
    {
        return View("Form", new UsuarioFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Criar(UsuarioFormViewModel modelo)
    {
        // No cadastro a senha é obrigatória.
        if (string.IsNullOrEmpty(modelo.Senha))
            ModelState.AddModelError(nameof(modelo.Senha), "Informe a senha.");

        if (!ModelState.IsValid)
            return View("Form", modelo);

        try
        {
            await _usuarioService.CriarAsync(modelo.ParaModelo(), modelo.Senha!);
            TempData["Sucesso"] = $"Usuário \"{modelo.Nome}\" cadastrado.";
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
        var usuario = await _usuarioService.ObterAsync(id);
        if (usuario is null)
            return NotFound();

        return View("Form", UsuarioFormViewModel.DeModelo(usuario));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, UsuarioFormViewModel modelo)
    {
        modelo.Id = id;
        if (!ModelState.IsValid)
            return View("Form", modelo);

        try
        {
            // Senha em branco = mantém a atual (regra no Service).
            await _usuarioService.AtualizarAsync(modelo.ParaModelo(), modelo.Senha, User.ObterUsuarioId());
            TempData["Sucesso"] = $"Usuário \"{modelo.Nome}\" atualizado.";
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
        var usuario = await _usuarioService.ObterAsync(id);
        if (usuario is null)
            return NotFound();

        return View(usuario);
    }

    [HttpPost, ActionName("Excluir")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ExcluirConfirmado(int id)
    {
        try
        {
            TempData["Sucesso"] = await _usuarioService.ExcluirAsync(id, User.ObterUsuarioId());
        }
        catch (RegraNegocioException erro)
        {
            TempData["Erro"] = erro.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}
