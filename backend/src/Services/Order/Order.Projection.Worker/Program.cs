using CqrsDemo.BuildingBlocks.Observability;
using CqrsDemo.BuildingBlocks.Messaging;
using CqrsDemo.Contracts.Messaging;
using Order.Infrastructure;
using Order.Projection.Worker.Consumers;

var builder = Host.CreateApplicationBuilder(args);
builder.AddPlatformObservability("order-projection-worker");
builder.Services.AddOrderReadInfrastructure(builder.Configuration);
builder.Services.AddKafkaConsumer<OrderProjectionConsumer>(
    builder.Configuration,
    KafkaConsumerGroups.OrderProjection);

var host = builder.Build();
await host.Services.InitializeOrderReadStoreAsync();
await host.RunAsync();
