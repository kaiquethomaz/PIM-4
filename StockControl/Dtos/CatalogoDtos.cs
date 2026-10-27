using System.ComponentModel.DataAnnotations;
using StockControl.Models;

namespace StockControl.Dtos;

// ---------------------------------------------------------------------
// Produto
// ---------------------------------------------------------------------

/// <summary>Produto devolvido pela API.</summary>
public class ProdutoDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string CodigoBarras { get; set; } = string.Empty;
    public decimal Preco { get; set; }
    public int EstoqueAtual { get; set; }
    public int EstoqueMinimo { get; set; }

    /// <summary>True quando EstoqueAtual &lt; EstoqueMinimo (precisa de reposição).</summary>
    public bool AbaixoDoMinimo { get; set; }

    public int CategoriaId { get; set; }
    public string Categoria { get; set; } = string.Empty;
    public int FornecedorId { get; set; }
    public string Fornecedor { get; set; } = string.Empty;

    /// <summary>False = produto desativado (o app deve escondê-lo).</summary>
    public bool Ativo { get; set; }

    /// <summary>Última alteração. Usar o maior valor recebido como "atualizadoDesde" na próxima sincronização.</summary>
    public DateTime AtualizadoEm { get; set; }

    public static ProdutoDto DeModelo(Produto produto) => new()
    {
        Id = produto.Id,
        Nome = produto.Nome,
        CodigoBarras = produto.CodigoBarras,
        Preco = produto.Preco,
        EstoqueAtual = produto.EstoqueAtual,
        EstoqueMinimo = produto.EstoqueMinimo,
        AbaixoDoMinimo = produto.EstaAbaixoDoMinimo(),
        CategoriaId = produto.CategoriaId,
        Categoria = produto.Categoria?.Nome ?? string.Empty,
        FornecedorId = produto.FornecedorId,
        Fornecedor = produto.Fornecedor?.RazaoSocial ?? string.Empty,
        Ativo = produto.Ativo,
        AtualizadoEm = produto.AtualizadoEm
    };
}

/// <summary>Dados para cadastrar ou alterar um produto.</summary>
public class ProdutoRequisicao
{
    /// <example>Biscoito Recheado 140g</example>
    [Required(ErrorMessage = "Informe o nome.")]
    [StringLength(120, ErrorMessage = "O nome pode ter no máximo {1} caracteres.")]
    public string Nome { get; set; } = string.Empty;

    /// <example>7891000000165</example>
    [Required(ErrorMessage = "Informe o código de barras.")]
    [RegularExpression(@"^\d{1,20}$", ErrorMessage = "O código de barras deve ter só números (até 20).")]
    public string CodigoBarras { get; set; } = string.Empty;

    /// <example>10.5</example>
    [Range(typeof(decimal), "0", "99999999", ErrorMessage = "O preço deve ser maior ou igual a zero.")]
    public decimal Preco { get; set; }

    /// <example>4</example>
    [Range(0, 1_000_000, ErrorMessage = "O estoque mínimo deve ser zero ou maior.")]
    public int EstoqueMinimo { get; set; }

    /// <example>1</example>
    [Range(1, int.MaxValue, ErrorMessage = "Informe a categoria.")]
    public int CategoriaId { get; set; }

    /// <example>1</example>
    [Range(1, int.MaxValue, ErrorMessage = "Informe o fornecedor.")]
    public int FornecedorId { get; set; }

    /// <summary>Só no cadastro (POST): quantidade inicial, registrada como ENTRADA.</summary>
    /// <example>5</example>
    [Range(0, 1_000_000, ErrorMessage = "O estoque inicial deve ser zero ou maior.")]
    public int EstoqueInicial { get; set; }

    /// <summary>Só na alteração (PUT).</summary>
    public bool Ativo { get; set; } = true;

    public Produto ParaModelo(int id = 0) => new()
    {
        Id = id,
        Nome = Nome,
        CodigoBarras = CodigoBarras,
        Preco = Preco,
        EstoqueMinimo = EstoqueMinimo,
        CategoriaId = CategoriaId,
        FornecedorId = FornecedorId,
        Ativo = Ativo
    };
}

// ---------------------------------------------------------------------
// Categoria
// ---------------------------------------------------------------------

public class CategoriaDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public DateTime AtualizadoEm { get; set; }

    public static CategoriaDto DeModelo(Categoria categoria) => new()
    {
        Id = categoria.Id,
        Nome = categoria.Nome,
        Descricao = categoria.Descricao,
        AtualizadoEm = categoria.AtualizadoEm
    };
}

public class CategoriaRequisicao
{
    /// <example>Padaria</example>
    [Required(ErrorMessage = "Informe o nome.")]
    [StringLength(60, ErrorMessage = "O nome pode ter no máximo {1} caracteres.")]
    public string Nome { get; set; } = string.Empty;

    /// <example>Pães e bolos</example>
    [StringLength(250, ErrorMessage = "A descrição pode ter no máximo {1} caracteres.")]
    public string? Descricao { get; set; }

    public Categoria ParaModelo(int id = 0) => new() { Id = id, Nome = Nome, Descricao = Descricao };
}

// ---------------------------------------------------------------------
// Fornecedor
// ---------------------------------------------------------------------

public class FornecedorDto
{
    public int Id { get; set; }
    public string RazaoSocial { get; set; } = string.Empty;

    /// <summary>Só os 14 números.</summary>
    public string CNPJ { get; set; } = string.Empty;

    /// <summary>Com máscara: 11.222.333/0001-81.</summary>
    public string CnpjFormatado { get; set; } = string.Empty;

    public string? Telefone { get; set; }
    public string? Email { get; set; }
    public DateTime AtualizadoEm { get; set; }

    public static FornecedorDto DeModelo(Fornecedor fornecedor) => new()
    {
        Id = fornecedor.Id,
        RazaoSocial = fornecedor.RazaoSocial,
        CNPJ = fornecedor.CNPJ,
        CnpjFormatado = fornecedor.CnpjFormatado(),
        Telefone = fornecedor.Telefone,
        Email = fornecedor.Email,
        AtualizadoEm = fornecedor.AtualizadoEm
    };
}

public class FornecedorRequisicao
{
    /// <example>Delta Hortifruti Ltda</example>
    [Required(ErrorMessage = "Informe a razão social.")]
    [StringLength(150, ErrorMessage = "A razão social pode ter no máximo {1} caracteres.")]
    public string RazaoSocial { get; set; } = string.Empty;

    /// <summary>Com ou sem máscara.</summary>
    /// <example>11.444.777/0001-61</example>
    [Required(ErrorMessage = "Informe o CNPJ.")]
    [StringLength(18, MinimumLength = 14, ErrorMessage = "CNPJ inválido.")]
    public string CNPJ { get; set; } = string.Empty;

    /// <example>(11) 3333-4004</example>
    [StringLength(20, ErrorMessage = "O telefone pode ter no máximo {1} caracteres.")]
    public string? Telefone { get; set; }

    /// <example>contato@delta.exemplo</example>
    [EmailAddress(ErrorMessage = "E-mail inválido.")]
    [StringLength(150, ErrorMessage = "O e-mail pode ter no máximo {1} caracteres.")]
    public string? Email { get; set; }

    public Fornecedor ParaModelo(int id = 0) => new()
    {
        Id = id,
        RazaoSocial = RazaoSocial,
        CNPJ = CNPJ,
        Telefone = Telefone,
        Email = Email
    };
}
