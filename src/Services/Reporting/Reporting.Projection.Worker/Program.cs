using CqrsDemo.BuildingBlocks.Messaging.Options;
using Reporting.Infrastructure;
using Reporting.Projection.Worker;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.Configure<AzureServiceBusOptions>(builder.Configuration.GetSection(AzureServiceBusOptions.SectionName));
builder.Services.AddReportingInfrastructure(builder.Configuration);
builder.Services.AddHostedService<ReportingProjectionProcessor>();

var host = builder.Build();
await host.Services.InitializeReportingStoreAsync();
await host.RunAsync();
