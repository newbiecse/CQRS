using CqrsDemo.BuildingBlocks.Messaging.Options;
using Order.Infrastructure;
using Order.Projection.Worker;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.Configure<AzureServiceBusOptions>(builder.Configuration.GetSection(AzureServiceBusOptions.SectionName));
builder.Services.AddOrderReadInfrastructure(builder.Configuration);
builder.Services.AddHostedService<OrderProjectionProcessor>();

var host = builder.Build();
await host.Services.InitializeOrderReadStoreAsync();
await host.RunAsync();
