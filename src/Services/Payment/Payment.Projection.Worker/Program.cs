using CqrsDemo.BuildingBlocks.Messaging.Options;
using Payment.Infrastructure;
using Payment.Projection.Worker;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.Configure<AzureServiceBusOptions>(builder.Configuration.GetSection(AzureServiceBusOptions.SectionName));
builder.Services.AddPaymentReadInfrastructure(builder.Configuration);
builder.Services.AddHostedService<PaymentProjectionProcessor>();

var host = builder.Build();
await host.Services.InitializePaymentReadStoreAsync();
await host.RunAsync();
