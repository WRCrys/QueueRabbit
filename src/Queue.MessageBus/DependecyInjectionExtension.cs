using Microsoft.Extensions.DependencyInjection;

namespace Queue.MessageBus;

public static class DependecyInjectionExtension
{
    public static IServiceCollection AddMessageBus(this IServiceCollection services, string appId, string host,
        int port, string user, string password)
    {
        if (string.IsNullOrEmpty(appId) || string.IsNullOrEmpty(host) || string.IsNullOrEmpty(user) ||
            string.IsNullOrEmpty(password) || port <= 0)
            throw new ArgumentException();

        services.AddSingleton<IMessageBus>(new MessageBus(host, port, user, password, appId));

        return services;
    }
}