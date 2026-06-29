using CqrsDemo.BuildingBlocks.Observability;
using CqrsDemo.BuildingBlocks.Messaging;
using CqrsDemo.Contracts.Messaging;
using Inventory.Application;
using Inventory.Application.Integration;
using Inventory.Infrastructure;
using Inventory.Integration.Worker.Consumers;
using Microsoft.Extensions.DependencyInjection;

var builder = Host.CreateApplicationBuilder(args);
builder.AddPlatformObservability("inventory-integration-worker");
builder.Services.AddInventoryApplication();
builder.Services.AddInventoryInfrastructure(builder.Configuration);
builder.Services.AddScoped<InventoryIntegrationHandlers>();
builder.Services.AddKafkaConsumer<InventoryIntegrationConsumer>(
    builder.Configuration,
    KafkaConsumerGroups.InventoryIntegration);

var host = builder.Build();
await host.Services.InitializeInventoryStoreAsync();
await host.RunAsync();
