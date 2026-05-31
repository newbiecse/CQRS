using CqrsDemo.Contracts.Serialization;

namespace CqrsDemo.Messaging.Serialization;

public static class IntegrationEventJson
{
    public static string Serialize<T>(T value) => IntegrationEventSerializer.Serialize(value);

    public static T Deserialize<T>(string json) => IntegrationEventSerializer.Deserialize<T>(json);
}
