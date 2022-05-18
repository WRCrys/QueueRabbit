using Queue.Core.Integration;
using Queue.MessageBus;

namespace Queue.Worker;

public class Worker : BackgroundService
{
    private const string QueueName = "conferencia.loja.pedido";
    private readonly ILogger<Worker> _logger;
    private readonly IMessageBus _messageBus;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public Worker(ILogger<Worker> logger, IServiceScopeFactory serviceScopeFactory, IMessageBus messageBus)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
        _messageBus = messageBus;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        stoppingToken.ThrowIfCancellationRequested();

        _messageBus.Subscribe<PedidoEvent>(QueueName, ObterNumeroPedido);
    }

    private async Task ObterNumeroPedido(PedidoEvent evento)
    {
        using var scope = _serviceScopeFactory.CreateScope();

        Console.WriteLine($"Numero do Pedido: {evento.NumeroPedido} e Filial: {evento.Filial}");
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await base.StopAsync(cancellationToken);
        _messageBus.Close();
        _logger.LogInformation("RabbitMq connection is closed.");
    }
}