using CqrsDemo.BuildingBlocks.Observability;
using MediatR;
using User.Application;
using User.Application.Commands.DeactivateUser;
using User.Application.Commands.ProvisionUser;
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
    note = "User profile commands — identity is handled by Auth.Api."
}));

app.MapPost("/api/users/provision", async (ProvisionUserRequest request, IMediator mediator) =>
{
    try
    {
        await mediator.Send(new ProvisionUserCommand(request.UserId, request.Email, request.DisplayName));
        return Results.Accepted($"/api/users/{request.UserId}", new { userId = request.UserId });
    }
    catch (ArgumentException ex) { return Results.BadRequest(new { message = ex.Message }); }
});

app.MapPost("/api/users", async (RegisterUserRequest request, IMediator mediator) =>
{
    try
    {
        var id = await mediator.Send(new RegisterUserCommand(request.Email, request.DisplayName));
        return Results.Created($"/api/users/{id}", new { userId = id });
    }
    catch (ArgumentException ex) { return Results.BadRequest(new { message = ex.Message }); }
    catch (InvalidOperationException ex) { return Results.BadRequest(new { message = ex.Message }); }
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

internal sealed record ProvisionUserRequest(Guid UserId, string Email, string DisplayName);
internal sealed record RegisterUserRequest(string Email, string DisplayName);
internal sealed record UpdateProfileRequest(string DisplayName);
