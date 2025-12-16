namespace Fiap.FCG.Payment.Functions.Options
{
    public class ServiceBusOptions
    {
        public string ComprasQueueName { get; set; } = "compras-realizadas";
        public string PagamentosQueueName { get; set; } = "pagamentos-aprovados";
    }
}
