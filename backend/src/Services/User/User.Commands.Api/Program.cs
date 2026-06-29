using CqrsDemo.BuildingBlocks.Observability;
using MediatR;
using User.Application;
using User.Application.Commands.DeactivateUser;
using User.Application.Commands.RegisterUser;
using User.Application.Commands.UpdateUserProfile;
using User.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.AddPlatformObservability("user-commands");
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddUserApplication();
builder.Services.AddUserWriteInfrastructure(builder.Configuration);

var app = builder.Build();
app.UsePlatformObservability();
await app.Services.InitializeUserWriteStoreAsync();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", () => Results.Ok(new
{
    service = "User.Commands.Api",
    port = 5206,
    note = "Register users here. Use returned userId as cart CustomerId."
}));

app.MapPost("/api/users", async (RegisterUserRequest request, IMediator mediator) =>
{
    try
    {
        var id = await mediator.Send(new RegisterUserCommand(request.Email, request.DisplayName));
        return Results.Created($"/api/users/{id}", new { userId = id });
    }
    catch (ArgumentException ex) { return Results.BadRequest(new { message = ex.Message }); }
});

app.MapPut("/api/users/{userId:guid}/profile", async (Guid userId, UpdateProfileRequest request, IMediator mediator) =>
    await ExecuteAsync(() => mediator.Send(new UpdateUserProfileCommand(userId, request.DisplayName))));

app.MapPost("/api/users/{userId:guid}/deactivate", async (Guid userId, IMediator mediator) =>
    await ExecuteAsync(() => mediator.Send(new DeactivateUserCommand(userId))));

app.Run();

static async Task<IResult> ExecuteAsync(Func<Task> action)
{
    try
    {
        await action();
        return Results.NoContent();
    }
    catch (KeyNotFoundException ex) { return Results.NotFound(new { message = ex.Message }); }
    catch (InvalidOperationException ex) { return Results.BadRequest(new { message = ex.Message }); }
    catch (ArgumentException ex) { return Results.BadRequest(new { message = ex.Message }); }
}

internal sealed record RegisterUserRequest(string Email, string DisplayName);
internal sealed record UpdateProfileRequest(string DisplayName);
