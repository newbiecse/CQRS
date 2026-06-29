using CheckoutSaga.Application;
using CheckoutSaga.Infrastructure;
using CheckoutSaga.Worker.Consumers;
using CqrsDemo.BuildingBlocks.Messaging;
using CqrsDemo.Contracts.Messaging;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddCheckoutSagaApplication();
builder.Services.AddCheckoutSagaInfrastructure(builder.Configuration);
builder.Services.AddKafkaConsumer<CheckoutSagaOrchestrationConsumer>(
    builder.Configuration,
    KafkaConsumerGroups.CheckoutSagaOrchestration);

var host = builder.Build();
await host.Services.InitializeCheckoutSagaStoreAsync();
await host.RunAsync();
