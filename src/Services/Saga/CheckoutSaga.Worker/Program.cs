using CheckoutSaga.Application;
using CheckoutSaga.Infrastructure;
using CheckoutSaga.Worker;
using CqrsDemo.BuildingBlocks.Messaging.Options;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.Configure<AzureServiceBusOptions>(builder.Configuration.GetSection(AzureServiceBusOptions.SectionName));
builder.Services.AddCheckoutSagaApplication();
builder.Services.AddCheckoutSagaInfrastructure(builder.Configuration);
builder.Services.AddHostedService<CheckoutSagaOrchestrationProcessor>();

var host = builder.Build();
await host.Services.InitializeCheckoutSagaStoreAsync();
await host.RunAsync();
