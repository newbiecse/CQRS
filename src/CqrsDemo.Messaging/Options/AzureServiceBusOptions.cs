namespace CqrsDemo.Messaging.Options;

public sealed class AzureServiceBusOptions
{
    public const string SectionName = "AzureServiceBus";

    public string ConnectionString { get; set; } = string.Empty;
    public string TopicName { get; set; } = "product-events";
}
