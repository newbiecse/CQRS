using CqrsDemo.BuildingBlocks.Observability;
using Reporting.Application;
using Reporting.Infrastructure;
using Reporting.Queries.Api;

var builder = WebApplication.CreateBuilder(args);
builder.AddPlatformObservability("reporting-queries");
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddReportingApplication();
builder.Services.AddReportingInfrastructure(builder.Configuration);

var app = builder.Build();
app.UsePlatformObservability();
await app.Services.InitializeReportingStoreAsync();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", () => Results.Ok(new
{
    service = "Reporting.Queries.Api",
    port = 5217,
    pattern = "Analytics read model (CqrsDemo_Reporting) — no cross-service HTTP"
}));

app.MapReportingEndpoints();

app.Run();
