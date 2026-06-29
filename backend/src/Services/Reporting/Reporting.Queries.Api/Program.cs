using Reporting.Application;
using Reporting.Infrastructure;
using Reporting.Queries.Api;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddReportingApplication();
builder.Services.AddReportingInfrastructure(builder.Configuration);

var app = builder.Build();
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
