namespace Shop.Admin.Api.Options;

public sealed class AdminShopServiceOptions
{
    public const string SectionName = "ShopServices";

    public string ProductCommands { get; set; } = "http://localhost:5201";
    public string ProductQueries { get; set; } = "http://localhost:5211";
    public string UserCommands { get; set; } = "http://localhost:5206";
    public string UserQueries { get; set; } = "http://localhost:5216";
    public string AuthApi { get; set; } = "http://localhost:5207";
    public string CartCommands { get; set; } = "http://localhost:5202";
    public string CartQueries { get; set; } = "http://localhost:5212";
    public string OrderCommands { get; set; } = "http://localhost:5203";
    public string OrderQueries { get; set; } = "http://localhost:5213";
    public string InventoryCommands { get; set; } = "http://localhost:5208";
    public string InventoryQueries { get; set; } = "http://localhost:5218";
    public string ReportingQueries { get; set; } = "http://localhost:5217";
    public string CheckoutSaga { get; set; } = "http://localhost:5205";
    public string ChatApi { get; set; } = "http://localhost:5220";
}
