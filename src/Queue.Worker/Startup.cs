using System.Globalization;
using System.IO.Compression;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.ResponseCompression;
using Queue.MessageBus;
using Queue.Worker.Configuration;

namespace Queue.Worker;

public class Startup
{

    private static readonly string RABBIT_HOST_NAME = Environment.GetEnvironmentVariable("RABBIT_HOST_NAME");
    private static readonly string RABBIT_PORT = Environment.GetEnvironmentVariable("RABBIT_PORT");
    private static readonly string RABBIT_USER_NAME = Environment.GetEnvironmentVariable("RABBIT_USER_NAME");
    private static readonly string RABBIT_PASSWORD = Environment.GetEnvironmentVariable("RABBIT_PASSWORD");

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAllOrigins",
                builder =>
                {
                    builder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
        });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("faturamento", policy => policy.RequireClaim("client_id", "rzc6qp47nbrvauhlkeij"));
        });

        services.Configure<GzipCompressionProviderOptions>(options => options.Level = CompressionLevel.Optimal);


        services.AddResponseCompression();

        services.AddControllers()
            .AddJsonOptions(options => options.JsonSerializerOptions.IgnoreNullValues = true);



        services.ResolveFilterException();

        services.AddMessageBus("queue.worker", RABBIT_HOST_NAME, int.Parse(RABBIT_PORT), RABBIT_USER_NAME,
            RABBIT_PASSWORD);

        services.AddHostedService<Worker>();

        services.AddSwaggerConfig();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
    {
        app.UseResponseCompression();

        var supportedCultures = new[] {new CultureInfo("pt-BR")};
        app.UseRequestLocalization(new RequestLocalizationOptions
        {
            DefaultRequestCulture = new RequestCulture("pt-BR", "pt-BR"),
            SupportedCultures = supportedCultures,
            SupportedUICultures = supportedCultures
        });

        if (env.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c => { c.SwaggerEndpoint("v1/swagger.json", "Queue.Worker V1"); });
        }

        app.UseRouting();

        app.UseAuthorization();

        app.UseCors("AllowAllOrigins");

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}