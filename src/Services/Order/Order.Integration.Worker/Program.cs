using CqrsDemo.BuildingBlocks.Messaging.Options;
using Order.Infrastructure;
using Order.Integration.Worker;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.Configure<AzureServiceBusOptions>(builder.Configuration.GetSection(AzureServiceBusOptions.SectionName));
builder.Services.AddOrderWriteInfrastructure(builder.Configuration);
builder.Services.AddHostedService<OrderIntegrationProcessor>();

var host = builder.Build();
await host.Services.InitializeOrderWriteStoreAsync();
await host.RunAsync();
