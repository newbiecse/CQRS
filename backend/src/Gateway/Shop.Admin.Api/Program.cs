using CqrsDemo.BuildingBlocks.Auth;
using CqrsDemo.BuildingBlocks.Observability;
using CqrsDemo.BuildingBlocks.Observability.Http;
using Audit.Infrastructure;
using Shop.Admin.Api.Clients;
using Shop.Admin.Api.Endpoints;
using Shop.Admin.Api.Http;
using Shop.Admin.Api.Options;

var builder = WebApplication.CreateBuilder(args);
builder.AddPlatformObservability("shop-admin-api");

builder.Services.Configure<AdminShopServiceOptions>(
    builder.Configuration.GetSection(AdminShopServiceOptions.SectionName));
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<AuthorizationForwardingHandler>();
builder.Services.AddTransient<CorrelationForwardingHandler>();
builder.Services.AddHttpClient("admin-backend")
    .AddHttpMessageHandler<AuthorizationForwardingHandler>()
    .AddHttpMessageHandler<CorrelationForwardingHandler>();
builder.Services.AddSingleton<AdminBackendClient>();
builder.Services.AddAuditElasticsearch(builder.Configuration);
builder.Services.AddPlatformJwtAuthentication(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins("http://localhost:3000", "http://localhost:8000")
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var app = builder.Build();
app.UsePlatformObservability();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => Results.Ok(new
{
    service = "Shop.Admin.Api",
    port = 5100,
    description = "BFF for admin portal — JWT auth + orchestrates domain microservices",
    routes = new[] { "/api/auth/*", "/api/admin/*" }
}));

app.MapAuthEndpoints();
app.MapAdminEndpoints();
app.Run();
