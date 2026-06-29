using Shop.Admin.Api.Clients;
using Shop.Admin.Api.Endpoints;
using Shop.Admin.Api.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<AdminShopServiceOptions>(
    builder.Configuration.GetSection(AdminShopServiceOptions.SectionName));
builder.Services.AddHttpClient("admin-backend");
builder.Services.AddSingleton<AdminBackendClient>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins("http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();

app.MapGet("/", () => Results.Ok(new
{
    service = "Shop.Admin.Api",
    port = 5100,
    description = "BFF for admin portal — orchestrates domain microservices",
    routes = "/api/admin/*"
}));

app.MapAdminEndpoints();
app.Run();
