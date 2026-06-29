namespace CheckoutSaga.Infrastructure.Options;

public sealed class ShopServiceOptions
{
    public const string SectionName = "ShopServices";

    public string CartCommands { get; set; } = "http://localhost:5202";
    public string OrderCommands { get; set; } = "http://localhost:5203";
    public string PaymentCommands { get; set; } = "http://localhost:5204";
}
