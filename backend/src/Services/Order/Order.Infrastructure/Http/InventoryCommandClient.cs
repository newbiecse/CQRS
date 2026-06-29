using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Order.Application.Abstractions;

namespace Order.Infrastructure.Http;

public sealed class InventoryServiceOptions
{
    public const string SectionName = "InventoryService";
    public string CommandsBaseUrl { get; set; } = "http://localhost:5208";
}

internal sealed record ReserveStockApiRequest(Guid OrderId, IReadOnlyList<ReserveStockLineApiRequest> Lines);
internal sealed record ReserveStockLineApiRequest(Guid ProductId, int Quantity);

public sealed class InventoryCommandClient(HttpClient httpClient) : IInventoryCommandClient
{
    public async Task ReserveAsync(Guid orderId, IReadOnlyList<InventoryStockLine> lines, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync(
            "api/inventory/reserve",
            new ReserveStockApiRequest(orderId, lines.Select(l => new ReserveStockLineApiRequest(l.ProductId, l.Quantity)).ToList()),
            cancellationToken);
        await EnsureSuccessAsync(response);
    }

    public async Task ReleaseAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsync($"api/inventory/release/{orderId}", null, cancellationToken);
        await EnsureSuccessAsync(response);
    }

    public async Task ConfirmAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsync($"api/inventory/confirm/{orderId}", null, cancellationToken);
        await EnsureSuccessAsync(response);
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode) return;

        var body = await response.Content.ReadAsStringAsync();
        throw response.StatusCode switch
        {
            System.Net.HttpStatusCode.NotFound => new KeyNotFoundException(body),
            System.Net.HttpStatusCode.BadRequest => new InvalidOperationException(body),
            _ => new HttpRequestException($"Inventory service returned {(int)response.StatusCode}: {body}")
        };
    }
}

public static class InventoryCommandClientExtensions
{
    public static IServiceCollection AddInventoryCommandClient(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<InventoryServiceOptions>(configuration.GetSection(InventoryServiceOptions.SectionName));
        services.AddHttpClient<IInventoryCommandClient, InventoryCommandClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<InventoryServiceOptions>>().Value;
            client.BaseAddress = new Uri(options.CommandsBaseUrl.TrimEnd('/') + "/");
        });
        return services;
    }
}
