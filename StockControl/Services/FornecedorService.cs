using StockControl.DAO;
using StockControl.Excecoes;
using StockControl.Models;

namespace StockControl.Services;

public class FornecedorService : IFornecedorService
{
    private readonly IFornecedorDAO _fornecedorDAO;

    public FornecedorService(IFornecedorDAO fornecedorDAO)
    {
        _fornecedorDAO = fornecedorDAO;
    }

    public Task<List<Fornecedor>> ListarAsync() => _fornecedorDAO.ListarTodosAsync();

    public Task<Fornecedor?> ObterAsync(int id) => _fornecedorDAO.BuscarPorIdAsync(id);

    public async Task<Fornecedor> CriarAsync(Fornecedor fornecedor)
    {
        await ValidarAsync(fornecedor);
        await _fornecedorDAO.InserirAsync(fornecedor);
        return fornecedor;
    }

    public async Task AtualizarAsync(Fornecedor fornecedor)
    {
        var existente = await _fornecedorDAO.BuscarPorIdAsync(fornecedor.Id)
            ?? throw new RegraNegocioException("Fornecedor não encontrado.");

        await ValidarAsync(fornecedor);

        existente.RazaoSocial = fornecedor.RazaoSocial;
        existente.CNPJ = fornecedor.CNPJ;
        existente.Telefone = fornecedor.Telefone;
        existente.Email = fornecedor.Email;
        await _fornecedorDAO.AtualizarAsync(existente);
    }

    public async Task ExcluirAsync(int id)
    {
        var fornecedor = await _fornecedorDAO.BuscarPorIdAsync(id)
            ?? throw new RegraNegocioException("Fornecedor não encontrado.");

        if (await _fornecedorDAO.PossuiProdutosAsync(id))
            throw new RegraNegocioException("Não é possível excluir: existem produtos deste fornecedor.");

        await _fornecedorDAO.ExcluirAsync(fornecedor);
    }

    private async Task ValidarAsync(Fornecedor fornecedor)
    {
        fornecedor.RazaoSocial = (fornecedor.RazaoSocial ?? string.Empty).Trim();
        fornecedor.CNPJ = Cnpj.Limpar(fornecedor.CNPJ);   // tira pontos, barra e traço
        fornecedor.Telefone = string.IsNullOrWhiteSpace(fornecedor.Telefone) ? null : fornecedor.Telefone.Trim();
        fornecedor.Email = string.IsNullOrWhiteSpace(fornecedor.Email) ? null : fornecedor.Email.Trim().ToLower();

        if (fornecedor.RazaoSocial.Length == 0)
            throw new RegraNegocioException("Informe a razão social.");

        if (!Cnpj.EhValido(fornecedor.CNPJ))
            throw new RegraNegocioException("CNPJ inválido.");

        // A busca passa pelo filtro automático: o CNPJ é único dentro do estabelecimento.
        if (await _fornecedorDAO.CnpjExisteAsync(fornecedor.CNPJ, fornecedor.Id))
            throw new RegraNegocioException("Já existe um fornecedor com este CNPJ.");
    }
}
