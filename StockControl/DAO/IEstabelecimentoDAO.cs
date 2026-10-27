using StockControl.Models;

namespace StockControl.DAO;

/// <summary>Acesso a dados da tabela Estabelecimento.</summary>
public interface IEstabelecimentoDAO : IDAOBase<Estabelecimento>
{
    /// <summary>Verifica no sistema inteiro se o CNPJ já tem cadastro.</summary>
    Task<bool> CnpjExisteAsync(string cnpj);

    /// <summary>
    /// Cria o estabelecimento, o usuário dono (Admin) e as categorias iniciais,
    /// tudo em uma única transação (ou grava tudo, ou não grava nada).
    /// </summary>
    Task CadastrarComAdministradorAsync(Estabelecimento estabelecimento, Usuario administrador, IEnumerable<Categoria> categoriasIniciais);
}
