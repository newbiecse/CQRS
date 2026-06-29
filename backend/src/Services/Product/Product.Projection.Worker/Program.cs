using CqrsDemo.BuildingBlocks.Observability;
using CqrsDemo.BuildingBlocks.Messaging;
using CqrsDemo.Contracts.Messaging;
using Product.Infrastructure;
using Product.Projection.Worker.Consumers;

var builder = Host.CreateApplicationBuilder(args);
builder.AddPlatformObservability("product-projection-worker");
builder.Services.AddProductReadInfrastructure(builder.Configuration);
builder.Services.AddProductElasticsearchSearch(builder.Configuration, enableReindexOnStartup: true);
builder.Services.AddKafkaConsumer<ProductProjectionConsumer>(
    builder.Configuration,
    KafkaConsumerGroups.ProductProjection);

var host = builder.Build();
await host.Services.InitializeProductReadStoreAsync();
await host.RunAsync();
