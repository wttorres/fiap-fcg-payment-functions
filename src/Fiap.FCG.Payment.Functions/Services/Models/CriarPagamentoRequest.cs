namespace Fiap.FCG.Payment.Functions.Services.Models
{
    public class CriarPagamentoRequest
    {
        public int UsuarioId { get; set; }
        public int JogoId { get; set; }
        public decimal Valor { get; set; }
    }
}
