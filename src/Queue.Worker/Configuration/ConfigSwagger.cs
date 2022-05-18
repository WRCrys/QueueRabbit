using Microsoft.OpenApi.Models;

namespace Queue.Worker.Configuration;

public static class ConfigSwagger
{
    public static IServiceCollection AddSwaggerConfig(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "API Notas Fiscais",
                Description = "API para criar notas fiscais",
                Contact = new OpenApiContact {Name = "Pague Menos"},
                Version = "v1"
            });
        });

        return services;
    }
}