using System.Net.Http.Json;
using CheckoutSaga.Application.Abstractions;
using CheckoutSaga.Infrastructure.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CheckoutSaga.Infrastructure.Http;

public sealed class CartCommandClient(HttpClient http) : ICartCommandClient
{
    public async Task<Guid> CheckoutAsync(Guid cartId, CancellationToken cancellationToken)
    {
        var response = await http.PostAsync($"/api/carts/{cartId}/checkout", null, cancellationToken);
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<CheckoutResponse>(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Cart checkout returned an empty response.");
        return body.OrderId;
    }

    private sealed record CheckoutResponse(Guid OrderId);
}

public sealed class PaymentCommandClient(HttpClient http) : IPaymentCommandClient
{
    public async Task<Guid> PayOrderAsync(Guid orderId, bool simulateFailure, CancellationToken cancellationToken)
    {
        var url = $"/api/orders/{orderId}/pay?simulateFailure={simulateFailure.ToString().ToLowerInvariant()}";
        var response = await http.PostAsync(url, null, cancellationToken);
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<PayResponse>(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Payment returned an empty response.");
        return body.PaymentId;
    }

    private sealed record PayResponse(Guid PaymentId);
}

public sealed class OrderCommandClient(HttpClient http) : IOrderCommandClient
{
    public async Task MarkOrderPaidAsync(Guid orderId, Guid paymentId, decimal amount, CancellationToken cancellationToken)
    {
        var response = await http.PostAsJsonAsync(
            $"/api/orders/{orderId}/mark-paid",
            new { paymentId, amount },
            cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task CancelOrderAsync(Guid orderId, string reason, CancellationToken cancellationToken)
    {
        var response = await http.PostAsJsonAsync(
            $"/api/orders/{orderId}/cancel",
            new { reason },
            cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}

public static class ShopServiceClientRegistration
{
    public static IServiceCollection AddShopServiceClients(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ShopServiceOptions>(configuration.GetSection(ShopServiceOptions.SectionName));
        var options = configuration.GetSection(ShopServiceOptions.SectionName).Get<ShopServiceOptions>()
            ?? new ShopServiceOptions();

        services.AddHttpClient<ICartCommandClient, CartCommandClient>(c => c.BaseAddress = new Uri(options.CartCommands));
        services.AddHttpClient<IPaymentCommandClient, PaymentCommandClient>(c => c.BaseAddress = new Uri(options.PaymentCommands));
        services.AddHttpClient<IOrderCommandClient, OrderCommandClient>(c => c.BaseAddress = new Uri(options.OrderCommands));
        return services;
    }
}
