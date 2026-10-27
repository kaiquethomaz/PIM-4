namespace StockControl.Models;

/// <summary>
/// Funções de CNPJ usadas pelo Estabelecimento e pelo Fornecedor.
/// "static": não precisa criar objeto, é só chamar Cnpj.EhValido("...").
/// </summary>
public static class Cnpj
{
    /// <summary>Remove a máscara: "11.222.333/0001-81" vira "11222333000181".</summary>
    public static string Limpar(string? cnpj)
    {
        return new string((cnpj ?? string.Empty).Where(char.IsDigit).ToArray());
    }

    /// <summary>Formata para exibir na tela: 11.222.333/0001-81.</summary>
    public static string Formatar(string cnpj)
    {
        return cnpj.Length == 14
            ? Convert.ToUInt64(cnpj).ToString(@"00\.000\.000\/0000\-00")
            : cnpj;
    }

    /// <summary>
    /// Confere os 2 dígitos verificadores do CNPJ (regra oficial da Receita Federal).
    /// Cada dígito é calculado multiplicando os números anteriores por pesos e
    /// usando o resto da divisão por 11.
    /// </summary>
    public static bool EhValido(string cnpj)
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
