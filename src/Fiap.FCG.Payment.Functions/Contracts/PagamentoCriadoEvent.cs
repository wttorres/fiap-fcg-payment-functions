namespace Fiap.FCG.Payment.Functions.Contracts
{
    public class PagamentoCriadoEvent
    {
        public int PagamentoId { get; set; }
        public int UsuarioId { get; set; }
        public int JogoId { get; set; }
        public decimal Valor { get; set; }
        public string Status { get; set; } = "Pendente";
        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    }
}
