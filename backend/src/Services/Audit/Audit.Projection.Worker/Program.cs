using CqrsDemo.BuildingBlocks.Messaging;
using CqrsDemo.Contracts.Messaging;
using Audit.Infrastructure;
using Audit.Projection.Worker.Consumers;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddAuditElasticsearch(builder.Configuration);
builder.Services.AddKafkaConsumer<BusinessAuditConsumer>(
    builder.Configuration,
    KafkaConsumerGroups.BusinessAudit);

var host = builder.Build();
await host.RunAsync();
