using CqrsDemo.BuildingBlocks.Observability;
using CqrsDemo.BuildingBlocks.Messaging;
using CqrsDemo.Contracts.Messaging;
using Order.Application;
using Order.Infrastructure;
using Order.Integration.Worker.Consumers;

var builder = Host.CreateApplicationBuilder(args);
builder.AddPlatformObservability("order-integration-worker");
builder.Services.AddOrderApplication();
builder.Services.AddOrderWriteInfrastructure(builder.Configuration);
builder.Services.AddKafkaConsumer<OrderIntegrationConsumer>(
    builder.Configuration,
    KafkaConsumerGroups.OrderIntegration);

var host = builder.Build();
await host.Services.InitializeOrderWriteStoreAsync();
await host.RunAsync();
