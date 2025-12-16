namespace Fiap.FCG.Payment.Functions.Services.Models
{
    public class CriarPagamentoResponse
    {
        public bool Sucesso { get; set; }
        public int Valor { get; set; }
        public string? Status { get; set; }
        public string? Mensagem { get; set; }
    }
}
