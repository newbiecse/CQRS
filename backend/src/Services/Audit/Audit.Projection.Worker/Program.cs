using CqrsDemo.BuildingBlocks.Observability;
using CqrsDemo.BuildingBlocks.Messaging;
using CqrsDemo.Contracts.Messaging;
using Audit.Infrastructure;
using Audit.Projection.Worker.Consumers;

var builder = Host.CreateApplicationBuilder(args);
builder.AddPlatformObservability("audit-projection-worker");
builder.Services.AddAuditElasticsearch(builder.Configuration);
builder.Services.AddKafkaConsumer<BusinessAuditConsumer>(
    builder.Configuration,
    KafkaConsumerGroups.BusinessAudit);

var host = builder.Build();
await host.RunAsync();
