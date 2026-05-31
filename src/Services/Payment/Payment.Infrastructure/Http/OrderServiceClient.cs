using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Payment.Application.Abstractions;

namespace Payment.Infrastructure.Http;

public sealed class OrderServiceOptions
{
    public const string SectionName = "OrderService";
    public string BaseUrl { get; set; } = "http://localhost:5213";
}

internal sealed record OrderApiResponse(Guid Id, decimal TotalAmount, string Status);

public sealed class OrderServiceClient(HttpClient httpClient) : IOrderServiceClient
{
    public async Task<OrderSummary?> GetOrderAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetAsync($"api/orders/{orderId}", cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
        response.EnsureSuccessStatusCode();
        var order = await response.Content.ReadFromJsonAsync<OrderApiResponse>(cancellationToken: cancellationToken);
        return order is null ? null : new OrderSummary(order.Id, order.TotalAmount, order.Status);
    }
}

public static class OrderServiceClientExtensions
{
    public static IServiceCollection AddOrderServiceClient(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<OrderServiceOptions>(configuration.GetSection(OrderServiceOptions.SectionName));
        services.AddHttpClient<IOrderServiceClient, OrderServiceClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<OrderServiceOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
        });
        return services;
    }
}
