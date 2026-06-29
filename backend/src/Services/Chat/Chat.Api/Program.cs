using System.Text.Json;
using Chat.Api.Endpoints;
using Chat.Infrastructure;
using CqrsDemo.BuildingBlocks.Observability;

var builder = WebApplication.CreateBuilder(args);
builder.AddPlatformObservability("chat-api");
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddChatInfrastructure(builder.Configuration);
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins("http://localhost:3001", "http://localhost:8000", "http://localhost:5100", "http://localhost:5000")
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var app = builder.Build();
app.UsePlatformObservability();
app.UseCors();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", () => Results.Ok(new
{
    service = "Chat.Api",
    port = 5220,
    endpoints = new[] { "POST /api/chat/completions" },
    note = "OpenAI-compatible streaming chat with shop catalog context"
}));

app.MapChatEndpoints();

app.Run();
