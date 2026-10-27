using StockControl.Models;

namespace StockControl.DAO;

/// <summary>Acesso a dados da tabela Fornecedor.</summary>
public interface IFornecedorDAO : IDAOBase<Fornecedor>
{
    Task<bool> CnpjExisteAsync(string cnpj, int? ignorarId = null);
    Task<bool> PossuiProdutosAsync(int fornecedorId);
}
