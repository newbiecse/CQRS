namespace CqrsDemo.Commands.Application.Abstractions;

public sealed record StoredEventDto(
    Guid Id,
    Guid StreamId,
    string StreamType,
    long Version,
    string EventType,
    string Payload,
    DateTime OccurredOn);
