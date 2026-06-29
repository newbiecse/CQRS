using CqrsDemo.BuildingBlocks.Messaging;
using CqrsDemo.Contracts.Messaging;
using Product.Infrastructure;
using Product.Projection.Worker.Consumers;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddProductReadInfrastructure(builder.Configuration);
builder.Services.AddKafkaConsumer<ProductProjectionConsumer>(
    builder.Configuration,
    KafkaConsumerGroups.ProductProjection);

var host = builder.Build();
await host.Services.InitializeProductReadStoreAsync();
await host.RunAsync();
