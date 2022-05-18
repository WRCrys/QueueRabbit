using System.Text;
using System.Text.Json;
using Polly;
using Queue.MessageBus.Integration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace Queue.MessageBus;

public class MessageBus : IMessageBus
{
    private readonly string _appId;

    private readonly string _host;
    private readonly string _password;
    private readonly int _port;
    private readonly string _user;
    private IModel _channel;
    private IConnection _connection;

    public MessageBus(string host, int port, string user, string password, string appId)
    {
        _host = host;
        _port = port;
        _user = user;
        _password = password;
        _appId = appId;
    }

    public bool IsConnected => _connection?.IsOpen ?? false;

    public void Publish<T>(T payload, string exchange = "", string routingKey = "")
    {
        TryConnect();
        var props = _channel.CreateBasicProperties();
        props.AppId = _appId;
        props.UserId = _user;
        props.MessageId = Guid.NewGuid().ToString("N");
        props.Persistent = true;

        var body = Serialize(payload);

        _channel.BasicPublish(exchange, routingKey, props, body);
    }

    public void Subscribe<T>(string queueName, Func<T, Task> onMessage) where T : class
    {
        PreparingConsumer(queueName, async (_, ea) =>
        {
            try
            {
                await onMessage(Convert<T>(ea)!);

                _channel.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception e)
            {
                _channel.BasicNack(ea.DeliveryTag, false, false);
            }
        });
    }

    public void Subscribe<T>(string queueName, Func<T, IChannel, DeliverEventArgs, Task> onMessage) where T : class
    {
        PreparingConsumer(queueName,
            async (_, ea) => await onMessage(Convert<T>(ea)!, (IChannel) _channel, (DeliverEventArgs) ea));
    }

    public void Close()
    {
        _connection.Close();
    }

    private void TryConnect()
    {
        if (IsConnected) return;

        var policy = Policy.Handle<AlreadyClosedException>()
            .Or<BrokerUnreachableException>()
            .WaitAndRetry(3, retryAttempt =>
                TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

        policy.Execute(() =>
        {
            var connectionFactory = new ConnectionFactory
            {
                HostName = _host,
                Port = _port,
                UserName = _user,
                Password = _password,
                ClientProvidedName = _appId,
                DispatchConsumersAsync = true
            };
            _connection = connectionFactory.CreateConnection();
            _channel = _connection.CreateModel();
        });
    }

    private byte[] Serialize<T>(T payload)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var msg = JsonSerializer.Serialize(payload, options);
        return Encoding.UTF8.GetBytes(msg);
    }

    private void PreparingConsumer(string queueName, AsyncEventHandler<BasicDeliverEventArgs> handler)
    {
        TryConnect();

        _channel.QueueDeclarePassive(queueName);
        _channel.BasicQos(0, 1, false);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.Received += handler;

        _channel.BasicConsume(queueName, false, consumer);
    }

    private T? Convert<T>(BasicDeliverEventArgs args)
    {
        try
        {
            var msg = Encoding.UTF8.GetString(args.Body.ToArray());

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            return JsonSerializer.Deserialize<T>(msg, options);
        }
        catch (JsonException)
        {
            _channel.BasicNack(args.DeliveryTag, false, false);
            throw;
        }
    }
}