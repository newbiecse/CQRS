using CqrsDemo.BuildingBlocks.Observability.Http;
using CqrsDemo.BuildingBlocks.Observability.Logging;
using CqrsDemo.BuildingBlocks.Observability.Middleware;
using CqrsDemo.BuildingBlocks.Observability.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Formatting.Compact;
using Serilog.Sinks.Elasticsearch;

namespace CqrsDemo.BuildingBlocks.Observability;

public static class ObservabilityExtensions
{
    public static WebApplicationBuilder AddPlatformObservability(
        this WebApplicationBuilder builder,
        string serviceName)
    {
        RegisterOptions(builder.Configuration, builder.Services, serviceName);
        builder.Host.UseSerilog((context, _, loggerConfiguration) =>
            ConfigureLogger(loggerConfiguration, context.Configuration, serviceName));
        ConfigureOpenTelemetry(builder.Services, builder.Configuration, serviceName);
        return builder;
    }

    public static HostApplicationBuilder AddPlatformObservability(
        this HostApplicationBuilder builder,
        string serviceName)
    {
        RegisterOptions(builder.Configuration, builder.Services, serviceName);
        builder.Services.AddSerilog((services, loggerConfiguration) =>
            ConfigureLogger(loggerConfiguration, services.GetRequiredService<IConfiguration>(), serviceName));
        ConfigureOpenTelemetry(builder.Services, builder.Configuration, serviceName);
        return builder;
    }

    public static WebApplication UsePlatformObservability(this WebApplication app)
    {
        app.UseMiddleware<CorrelationIdMiddleware>();
        return app;
    }

    public static IServiceCollection AddCorrelationForwardingHttpClient(
        this IServiceCollection services,
        string clientName)
    {
        services.AddTransient<CorrelationForwardingHandler>();
        services.AddHttpClient(clientName).AddHttpMessageHandler<CorrelationForwardingHandler>();
        return services;
    }

    private static void RegisterOptions(
        IConfiguration configuration,
        IServiceCollection services,
        string serviceName)
    {
        services.Configure<ObservabilityOptions>(configuration.GetSection(ObservabilityOptions.SectionName));
        services.PostConfigure<ObservabilityOptions>(options =>
        {
            if (!string.IsNullOrWhiteSpace(serviceName))
                options.ServiceName = serviceName;
        });
    }

    private static ObservabilityOptions ResolveOptions(IConfiguration configuration, string serviceName)
    {
        var options = configuration.GetSection(ObservabilityOptions.SectionName).Get<ObservabilityOptions>()
            ?? new ObservabilityOptions();

        if (!string.IsNullOrWhiteSpace(serviceName))
            options.ServiceName = serviceName;

        return options;
    }

    private static void ConfigureLogger(
        LoggerConfiguration loggerConfiguration,
        IConfiguration configuration,
        string serviceName)
    {
        var options = ResolveOptions(configuration, serviceName);

        loggerConfiguration
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .Enrich.With<CorrelationEnricher>()
            .Enrich.WithEnvironmentName()
            .Enrich.WithProperty("service.name", options.ServiceName)
            .WriteTo.Console(new CompactJsonFormatter());

        if (options.EnableElasticsearchLogSink)
        {
            loggerConfiguration.WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(options.ElasticsearchUrl))
            {
                AutoRegisterTemplate = true,
                IndexFormat = options.LogIndexFormat,
                BatchPostingLimit = 50,
                Period = TimeSpan.FromSeconds(2)
            });
        }
    }

    private static void ConfigureOpenTelemetry(
        IServiceCollection services,
        IConfiguration configuration,
        string serviceName)
    {
        var options = ResolveOptions(configuration, serviceName);
        if (!options.EnableOtlpExporter)
            return;

        var resourceBuilder = ResourceBuilder.CreateDefault().AddService(options.ServiceName);

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(options.ServiceName))
            .WithTracing(tracing => tracing
                .SetResourceBuilder(resourceBuilder)
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddSource("MassTransit")
                .AddOtlpExporter(otlp => otlp.Endpoint = new Uri(options.OtlpEndpoint)))
            .WithMetrics(metrics => metrics
                .SetResourceBuilder(resourceBuilder)
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddOtlpExporter(otlp => otlp.Endpoint = new Uri(options.OtlpEndpoint)));
    }
}
