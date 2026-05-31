using CqrsDemo.BuildingBlocks.Messaging.Options;
using User.Infrastructure;
using User.Projection.Worker;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.Configure<AzureServiceBusOptions>(builder.Configuration.GetSection(AzureServiceBusOptions.SectionName));
builder.Services.AddUserReadInfrastructure(builder.Configuration);
builder.Services.AddHostedService<UserProjectionProcessor>();

var host = builder.Build();
await host.Services.InitializeUserReadStoreAsync();
await host.RunAsync();
