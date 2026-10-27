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
        fornecedor.CNPJ = Fornecedor.LimparCnpj(fornecedor.CNPJ);   // tira pontos, barra e traço
        fornecedor.Telefone = string.IsNullOrWhiteSpace(fornecedor.Telefone) ? null : fornecedor.Telefone.Trim();
        fornecedor.Email = string.IsNullOrWhiteSpace(fornecedor.Email) ? null : fornecedor.Email.Trim().ToLower();

        if (fornecedor.RazaoSocial.Length == 0)
            throw new RegraNegocioException("Informe a razão social.");

        if (!CnpjEhValido(fornecedor.CNPJ))
            throw new RegraNegocioException("CNPJ inválido.");

        if (await _fornecedorDAO.CnpjExisteAsync(fornecedor.CNPJ, fornecedor.Id))
            throw new RegraNegocioException("Já existe um fornecedor com este CNPJ.");
    }

    /// <summary>
    /// Confere os 2 dígitos verificadores do CNPJ (regra oficial da Receita Federal).
    /// Cada dígito é calculado multiplicando os números anteriores por pesos e
    /// usando o resto da divisão por 11.
    /// </summary>
    public static bool CnpjEhValido(string cnpj)
    {
        if (cnpj.Length != 14 || !cnpj.All(char.IsDigit))
            return false;

        // CNPJs com todos os números iguais (00000000000000, 11111111111111...) são inválidos.
        if (cnpj.Distinct().Count() == 1)
            return false;

        int[] pesos1 = { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
        int[] pesos2 = { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };

        int digito1 = CalcularDigito(cnpj, pesos1);
        int digito2 = CalcularDigito(cnpj, pesos2);

        return cnpj[12] - '0' == digito1 && cnpj[13] - '0' == digito2;
    }

    private static int CalcularDigito(string cnpj, int[] pesos)
    {
        int soma = 0;
        for (int i = 0; i < pesos.Length; i++)
            soma += (cnpj[i] - '0') * pesos[i];

        int resto = soma % 11;
        return resto < 2 ? 0 : 11 - resto;
    }
}
