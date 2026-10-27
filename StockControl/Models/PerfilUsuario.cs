namespace StockControl.Models;

/// <summary>
/// Perfis de acesso. No banco ficam gravados como texto ("Admin", "Gerente", "Operador")
/// e o mesmo texto é usado no [Authorize(Roles = "Admin")].
/// </summary>
public enum PerfilUsuario
{
    Admin,      // acesso total, inclusive cadastro de usuários
    Gerente,    // cadastros (produtos, categorias, fornecedores) e relatórios
    Operador    // registra entradas e vendas
}
