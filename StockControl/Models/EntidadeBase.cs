namespace StockControl.Models;

/// <summary>
/// Classe base (abstrata) das entidades de cadastro.
/// POO aplicada:
///  - Herança: Usuario, Categoria, Fornecedor e Produto herdam daqui, então
///    ninguém precisa repetir as propriedades Id e AtualizadoEm.
///  - Abstração: "abstract" impede criar uma EntidadeBase sozinha (new EntidadeBase()
///    não compila), porque ela não representa nada no mundo real.
///  - Encapsulamento: AtualizadoEm só pode ser alterada pelo método
///    MarcarComoAtualizado(), e não diretamente de fora da classe.
/// </summary>
public abstract class EntidadeBase
{
    public int Id { get; set; }

    // "protected set": só esta classe e as filhas podem alterar o valor.
    public DateTime AtualizadoEm { get; protected set; } = DateTime.Now;

    /// <summary>
    /// Registra que o objeto foi alterado agora.
    /// O app mobile usa AtualizadoEm para baixar só o que mudou.
    /// </summary>
    public void MarcarComoAtualizado()
    {
        AtualizadoEm = DateTime.Now;
    }
}
