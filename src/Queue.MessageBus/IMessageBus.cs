using Queue.MessageBus.Integration;

namespace Queue.MessageBus;

public interface IMessageBus
{
    bool IsConnected { get; }
    void Publish<T>(T payload, string exchange = "", string routingKey = "");
    void Subscribe<T>(string queueName, Func<T, Task> onMessage) where T : class;
    void Subscribe<T>(string queueName, Func<T, IChannel, DeliverEventArgs, Task> onMessage) where T : class;
    void Close();
}