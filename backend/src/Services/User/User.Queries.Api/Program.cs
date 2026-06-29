using CqrsDemo.BuildingBlocks.Observability;
using MediatR;
using User.Application;
using User.Application.Queries.GetAllUsers;
using User.Application.Queries.GetUserById;
using User.Application.Queries.GetUsersByIds;
using User.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.AddPlatformObservability("user-queries");
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddUserApplication();
builder.Services.AddUserReadInfrastructure(builder.Configuration);

var app = builder.Build();
app.UsePlatformObservability();
await app.Services.InitializeUserReadStoreAsync();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", () => Results.Ok(new { service = "User.Queries.Api", port = 5216 }));

app.MapGet("/api/users", async (IMediator mediator) => Results.Ok(await mediator.Send(new GetAllUsersQuery())));

app.MapGet("/api/users/{userId:guid}", async (Guid userId, IMediator mediator) =>
{
    var user = await mediator.Send(new GetUserByIdQuery(userId));
    return user is null ? Results.NotFound() : Results.Ok(user);
});

app.MapGet("/api/users/by-ids", async (string ids, IMediator mediator) =>
{
    if (string.IsNullOrWhiteSpace(ids))
        return Results.BadRequest(new { message = "Query 'ids' is required (comma-separated GUIDs)." });

    var userIds = new List<Guid>();
    foreach (var part in ids.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
    {
        if (!Guid.TryParse(part, out var id))
            return Results.BadRequest(new { message = $"Invalid GUID: {part}" });
        userIds.Add(id);
    }

    return Results.Ok(await mediator.Send(new GetUsersByIdsQuery(userIds)));
});

app.Run();
