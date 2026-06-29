using Confluent.Kafka;
using CqrsDemo.BuildingBlocks.Messaging.Abstractions;
using CqrsDemo.BuildingBlocks.Messaging.Options;
using CqrsDemo.Contracts.Messaging;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CqrsDemo.BuildingBlocks.Messaging;

public static class KafkaMessagingExtensions
{
    public static IServiceCollection AddKafkaProducer(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<KafkaOptions>(configuration.GetSection(KafkaOptions.SectionName));
        var topicName = ResolveTopicName(configuration);

        services.AddMassTransit(x =>
        {
            x.UsingInMemory((context, cfg) => cfg.ConfigureEndpoints(context));

            x.AddRider(rider =>
            {
                rider.AddProducer<string, IntegrationEventEnvelope>(topicName);

                rider.UsingKafka((context, k) =>
                {
                    var options = context.GetRequiredService<IOptions<KafkaOptions>>().Value;
                    k.Host(options.BootstrapServers);
                });
            });
        });

        services.AddSingleton<IIntegrationEventPublisher, MassTransitKafkaIntegrationEventPublisher>();
        return services;
    }

    public static IServiceCollection AddKafkaOutbox(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddKafkaProducer(configuration);
        services.AddHostedService<OutboxPublisherBackgroundService>();
        return services;
    }

    public static IServiceCollection AddKafkaConsumer<TConsumer>(
        this IServiceCollection services,
        IConfiguration configuration,
        string consumerGroup)
        where TConsumer : class, IConsumer<IntegrationEventEnvelope>
    {
        services.Configure<KafkaOptions>(configuration.GetSection(KafkaOptions.SectionName));

        services.AddMassTransit(x =>
        {
            x.AddConsumer<TConsumer>();

            x.UsingInMemory((context, cfg) => cfg.ConfigureEndpoints(context));

            x.AddRider(rider =>
            {
                rider.AddConsumer<TConsumer>();

                rider.UsingKafka((context, k) =>
                {
                    var options = context.GetRequiredService<IOptions<KafkaOptions>>().Value;
                    k.Host(options.BootstrapServers);

                    k.TopicEndpoint<string, IntegrationEventEnvelope>(
                        options.TopicName,
                        consumerGroup,
                        configure =>
                        {
                            configure.UseConsumeFilter<IntegrationEventCorrelationConsumeFilter>(context);
                            configure.ConfigureConsumer<TConsumer>(context);
                            configure.AutoOffsetReset = AutoOffsetReset.Earliest;
                        });
                });
            });
        });

        return services;
    }

    private static string ResolveTopicName(IConfiguration configuration) =>
        configuration.GetSection(KafkaOptions.SectionName).Get<KafkaOptions>()?.TopicName
        ?? KafkaTopics.ShopEvents;
}
