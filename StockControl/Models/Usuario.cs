namespace StockControl.Models;

/// <summary>Usuário que acessa o sistema web e o app (tabela Usuario).</summary>
public class Usuario : EntidadeBase, IPertenceAEstabelecimento
{
    // Estabelecimento em que o usuário trabalha.
    public int EstabelecimentoId { get; set; }
    public Estabelecimento? Estabelecimento { get; set; }

    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    // Guardamos só o hash BCrypt. A senha digitada nunca é salva.
    public string SenhaHash { get; set; } = string.Empty;

    public PerfilUsuario Perfil { get; set; } = PerfilUsuario.Operador;
    public bool Ativo { get; set; } = true;
    public DateTime CriadoEm { get; set; } = DateTime.Now;

    public bool EhAdmin() => Perfil == PerfilUsuario.Admin;
}
