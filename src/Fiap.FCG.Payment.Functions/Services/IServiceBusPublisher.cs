namespace Fiap.FCG.Payment.Functions.Services
{
    public interface IServiceBusPublisher
    {
        Task PublishAsync(string queueName, object payload, CancellationToken ct = default);
    }
}
