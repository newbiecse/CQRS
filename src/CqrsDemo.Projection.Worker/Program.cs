using CqrsDemo.Messaging;
using CqrsDemo.Projection.Worker.Projections;
using CqrsDemo.Projection.Worker.ServiceBus;
using CqrsDemo.Queries.Infrastructure;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddQueriesInfrastructure(builder.Configuration);
builder.Services.AddAzureServiceBusMessaging(builder.Configuration);
builder.Services.AddScoped<ShopProjectionHandler>();
builder.Services.AddHostedService<ProductProjectionProcessor>();

var host = builder.Build();

await host.Services.MigrateQueriesDatabaseAsync();

await host.RunAsync();
