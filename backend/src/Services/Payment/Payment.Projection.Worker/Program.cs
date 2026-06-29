using CqrsDemo.BuildingBlocks.Messaging;
using CqrsDemo.Contracts.Messaging;
using Payment.Infrastructure;
using Payment.Projection.Worker.Consumers;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddPaymentReadInfrastructure(builder.Configuration);
builder.Services.AddKafkaConsumer<PaymentProjectionConsumer>(
    builder.Configuration,
    KafkaConsumerGroups.PaymentProjection);

var host = builder.Build();
await host.Services.InitializePaymentReadStoreAsync();
await host.RunAsync();
