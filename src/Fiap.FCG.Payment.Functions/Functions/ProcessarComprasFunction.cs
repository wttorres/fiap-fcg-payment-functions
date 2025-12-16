using Fiap.FCG.Payment.Functions.Contracts;
using Fiap.FCG.Payment.Functions.Options;
using Fiap.FCG.Payment.Functions.Services;
using Fiap.FCG.Payment.Functions.Services.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Fiap.FCG.Payment.Functions.Functions
{
    public class ProcessarComprasFunction
    {
        private readonly ILogger<ProcessarComprasFunction> _logger;
        private readonly IPaymentApiClient _paymentApi;
        private readonly IServiceBusPublisher _publisher;
        private readonly ServiceBusOptions _sbOptions;

        public ProcessarComprasFunction(
            ILogger<ProcessarComprasFunction> logger,
            IPaymentApiClient paymentApi,
            IServiceBusPublisher publisher,
            IOptions<ServiceBusOptions> sbOptions)
        {
            _logger = logger;
            _paymentApi = paymentApi;
            _publisher = publisher;
            _sbOptions = sbOptions.Value;
        }

        [Function(nameof(ProcessarComprasFunction))]
        public async Task Run(
            [ServiceBusTrigger("compras-realizadas", Connection = "SERVICEBUS_CONNECTION")]
            string message,
            FunctionContext context,
            CancellationToken ct)
        {
            _logger.LogInformation(
                "Mensagem recebida da fila compras-realizadas: {Message}",
                message
            );

            CompraRealizadaEvent compra;
            try
            {
                compra = JsonSerializer.Deserialize<CompraRealizadaEvent>(
                    message,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                ) ?? throw new InvalidOperationException("Evento de compra veio nulo após deserialize.");
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Falha ao desserializar CompraRealizadaEvent. Mensagem será reprocessada ou enviada para DLQ."
                );
                throw; // força retry / DLQ
            }

            if (compra.Itens.Count == 0)
            {
                _logger.LogWarning(
                    "Compra sem itens. Ignorando. UsuarioId: {UsuarioId}",
                    compra.UsuarioId
                );
                return;
            }

            foreach (var item in compra.Itens)
            {
                _logger.LogInformation(
                    "Processando pagamento. UsuarioId={UsuarioId}, JogoId={JogoId}, Valor={Valor}",
                    compra.UsuarioId,
                    item.JogoId,
                    item.Valor
                );

                var pagamentoResp = await _paymentApi.CriarPagamentoAsync(
                    new CriarPagamentoRequest
                    {
                        UsuarioId = compra.UsuarioId,
                        JogoId = item.JogoId,
                        Valor = item.Valor
                    },
                    ct
                );

                if (!pagamentoResp.Sucesso || pagamentoResp.Valor  <= 0)
                {
                    _logger.LogError(
                        "Payment API retornou falha. UsuarioId={UsuarioId}, JogoId={JogoId}, Msg={Msg}",
                        compra.UsuarioId,
                        item.JogoId,
                        pagamentoResp.Mensagem
                    );

                    throw new InvalidOperationException(
                        $"Falha ao criar pagamento: {pagamentoResp.Mensagem}"
                    );
                }

                var evt = new PagamentoCriadoEvent
                {
                    PagamentoId = pagamentoResp.Valor,
                    UsuarioId = compra.UsuarioId,
                    JogoId = item.JogoId,
                    Valor = item.Valor,
                    Status = pagamentoResp.Status ?? "Pendente",
                    CriadoEm = DateTime.UtcNow
                };

                await _publisher.PublishAsync(
                    _sbOptions.PagamentosQueueName,
                    evt,
                    ct
                );

                _logger.LogInformation(
                    "Pagamento publicado em {Queue}. PagamentoId={PagamentoId}",
                    _sbOptions.PagamentosQueueName,
                    evt.PagamentoId
                );
            }
        }
    }
}
