namespace Fiap.FCG.Payment.Functions.Contracts
{
    public class CompraRealizadaEvent
    {
        public int UsuarioId { get; set; }
        public DateTime DataCompra { get; set; }
        public List<CompraRealizadaItemEvent> Itens { get; set; } = new();
    }

    public class CompraRealizadaItemEvent
    {
        public int JogoId { get; set; }
        public decimal Valor { get; set; }
    }
}
