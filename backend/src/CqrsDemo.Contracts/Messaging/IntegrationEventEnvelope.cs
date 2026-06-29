namespace CqrsDemo.Contracts.Messaging;

public sealed record IntegrationEventEnvelope(string EventType, string Payload);
