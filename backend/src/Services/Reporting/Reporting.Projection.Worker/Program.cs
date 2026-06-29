using CqrsDemo.BuildingBlocks.Observability;
using CqrsDemo.BuildingBlocks.Messaging;
using CqrsDemo.Contracts.Messaging;
using Reporting.Infrastructure;
using Reporting.Projection.Worker.Consumers;

var builder = Host.CreateApplicationBuilder(args);
builder.AddPlatformObservability("reporting-projection-worker");
builder.Services.AddReportingInfrastructure(builder.Configuration);
builder.Services.AddKafkaConsumer<ReportingProjectionConsumer>(
    builder.Configuration,
    KafkaConsumerGroups.ReportingProjection);

var host = builder.Build();
await host.Services.InitializeReportingStoreAsync();
await host.RunAsync();
