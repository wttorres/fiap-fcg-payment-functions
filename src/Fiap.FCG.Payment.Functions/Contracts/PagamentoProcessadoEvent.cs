namespace Fiap.FCG.Payment.Functions.Contracts
{
    public class PagamentoProcessadoEvent
    {
        public int CompraId { get; set; }
        public int UsuarioId { get; set; }
        public decimal Valor { get; set; }
        public string Status { get; set; } // APROVADO | REPROVADO
        public DateTime ProcessadoEm { get; set; }
    }

}
