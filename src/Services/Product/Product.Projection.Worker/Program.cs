using CqrsDemo.BuildingBlocks.Messaging.Options;
using Microsoft.Extensions.DependencyInjection;
using Product.Infrastructure;
using Product.Projection.Worker;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.Configure<AzureServiceBusOptions>(builder.Configuration.GetSection(AzureServiceBusOptions.SectionName));
builder.Services.AddProductReadInfrastructure(builder.Configuration);
builder.Services.AddHostedService<ProductProjectionProcessor>();

var host = builder.Build();
await host.Services.InitializeProductReadStoreAsync();
await host.RunAsync();
