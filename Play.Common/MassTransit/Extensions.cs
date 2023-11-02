using System.Reflection;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Play.Common.Settings;

namespace Play.Common.Extensions;

public static class Extensions
{
    public static IServiceCollection AddMassTransitWithRabbitMQ(this IServiceCollection services)
    {
        services.AddMassTransit(config =>
        {
            config.AddConsumers(Assembly.GetEntryAssembly()!);

            config.UsingRabbitMq((ctx, cfg) =>
            {
                var configuration = ctx.GetService<IConfiguration>()!;
                var rabbitMQSettings = configuration.GetSection(nameof(RabbitMQSettings)).Get<RabbitMQSettings>();
                var serviceSettings = configuration.GetSection("ServiceSettings").Get<ServiceSettings>();

                cfg.Host(rabbitMQSettings!.Host);
                cfg.ConfigureEndpoints(ctx, new KebabCaseEndpointNameFormatter(serviceSettings!.ServiceName, false));
            });

        });

        services.Configure<MassTransitHostOptions>(options =>
        {
            options.WaitUntilStarted = true;
            options.StartTimeout = TimeSpan.FromSeconds(30);
            options.StopTimeout = TimeSpan.FromMinutes(1);
        });

        return services;
    }
}
