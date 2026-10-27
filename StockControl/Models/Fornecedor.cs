namespace StockControl.Models;

/// <summary>Fornecedor de produtos (tabela Fornecedor).</summary>
public class Fornecedor : EntidadeBase
{
    public string RazaoSocial { get; set; } = string.Empty;

    // Só os 14 números, sem pontos, barra e traço (ex.: 11222333000181).
    public string CNPJ { get; set; } = string.Empty;

    public string? Telefone { get; set; }
    public string? Email { get; set; }

    public ICollection<Produto> Produtos { get; set; } = new List<Produto>();

    /// <summary>Remove a máscara: "11.222.333/0001-81" vira "11222333000181".</summary>
    public static string LimparCnpj(string? cnpj)
    {
        return new string((cnpj ?? string.Empty).Where(char.IsDigit).ToArray());
    }

    /// <summary>Formata para exibir na tela: 11.222.333/0001-81.</summary>
    public string CnpjFormatado()
    {
        return CNPJ.Length == 14
            ? Convert.ToUInt64(CNPJ).ToString(@"00\.000\.000\/0000\-00")
            : CNPJ;
    }
}
