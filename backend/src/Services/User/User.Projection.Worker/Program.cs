using CqrsDemo.BuildingBlocks.Observability;
using CqrsDemo.BuildingBlocks.Messaging;
using CqrsDemo.Contracts.Messaging;
using User.Infrastructure;
using User.Projection.Worker.Consumers;

var builder = Host.CreateApplicationBuilder(args);
builder.AddPlatformObservability("user-projection-worker");
builder.Services.AddUserReadInfrastructure(builder.Configuration);
builder.Services.AddKafkaConsumer<UserProjectionConsumer>(
    builder.Configuration,
    KafkaConsumerGroups.UserProjection);

var host = builder.Build();
await host.Services.InitializeUserReadStoreAsync();
await host.RunAsync();
