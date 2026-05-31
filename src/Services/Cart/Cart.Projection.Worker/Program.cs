using Cart.Infrastructure;
using Cart.Projection.Worker;
using CqrsDemo.BuildingBlocks.Messaging.Options;
using Microsoft.Extensions.DependencyInjection;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.Configure<AzureServiceBusOptions>(builder.Configuration.GetSection(AzureServiceBusOptions.SectionName));
builder.Services.AddCartReadInfrastructure(builder.Configuration);
builder.Services.AddHostedService<CartProjectionProcessor>();

var host = builder.Build();
await host.Services.InitializeCartReadStoreAsync();
await host.RunAsync();
