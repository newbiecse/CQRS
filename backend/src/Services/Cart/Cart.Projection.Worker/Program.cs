using CqrsDemo.BuildingBlocks.Observability;
using Cart.Infrastructure;
using Cart.Projection.Worker.Consumers;
using CqrsDemo.BuildingBlocks.Messaging;
using CqrsDemo.Contracts.Messaging;

var builder = Host.CreateApplicationBuilder(args);
builder.AddPlatformObservability("cart-projection-worker");
builder.Services.AddCartReadInfrastructure(builder.Configuration);
builder.Services.AddKafkaConsumer<CartProjectionConsumer>(
    builder.Configuration,
    KafkaConsumerGroups.CartProjection);

var host = builder.Build();
await host.Services.InitializeCartReadStoreAsync();
await host.RunAsync();
