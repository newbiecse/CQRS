namespace Reporting.Application.Abstractions;

public interface IReportingWriteRepository
{
    Task UpsertUserProfileAsync(
        Guid userId,
        string email,
        string displayName,
        bool isActive,
        DateTime updatedAt,
        CancellationToken cancellationToken = default);

    Task UpsertOrderFactAsync(
        Guid orderId,
        Guid userId,
        string? userEmail,
        string? userDisplayName,
        decimal totalAmount,
        string status,
        DateTime orderCreatedAt,
        CancellationToken cancellationToken = default);

    Task UpdateOrderStatusAsync(
        Guid orderId,
        string status,
        DateTime updatedAt,
        CancellationToken cancellationToken = default);

    Task DeactivateUserProfileAsync(Guid userId, DateTime deactivatedAt, CancellationToken cancellationToken = default);
}
