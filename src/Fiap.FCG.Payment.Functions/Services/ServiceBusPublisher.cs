using Azure.Messaging.ServiceBus;
using Fiap.FCG.Payment.Functions.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Fiap.FCG.Payment.Functions.Services
{
    public class ServiceBusPublisher : IServiceBusPublisher, IAsyncDisposable
    {
        private readonly ServiceBusClient _client;

        public ServiceBusPublisher(IConfiguration configuration)
        {
            // MESMO nome usado no ServiceBusTrigger
            var connectionString = configuration["SERVICEBUS_CONNECTION"];

            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException("SERVICEBUS_CONNECTION não configurado.");

            _client = new ServiceBusClient(connectionString);
        }

        public async Task PublishAsync(
            string queueName,
            object payload,
            CancellationToken ct = default)
        {
            var sender = _client.CreateSender(queueName);

            var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var message = new ServiceBusMessage(json)
            {
                ContentType = "application/json",
                Subject = payload.GetType().Name
            };

            await sender.SendMessageAsync(message, ct);
        }

        public async ValueTask DisposeAsync()
        {
            await _client.DisposeAsync();
        }
    }
}
