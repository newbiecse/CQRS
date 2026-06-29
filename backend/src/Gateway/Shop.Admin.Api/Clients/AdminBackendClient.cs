using System.Net.Http.Json;
using System.Text.Json;

namespace Shop.Admin.Api.Clients;

public sealed class AdminBackendClient(IHttpClientFactory httpClientFactory)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private HttpClient Http => httpClientFactory.CreateClient("admin-backend");

    public Task<IResult> GetAsync(string baseUrl, string path, CancellationToken cancellationToken) =>
        SendAsync<object?>(HttpMethod.Get, baseUrl, path, null, cancellationToken);

    public Task<IResult> PostAsync<TBody>(
        string baseUrl,
        string path,
        TBody body,
        CancellationToken cancellationToken) =>
        SendAsync(HttpMethod.Post, baseUrl, path, body, cancellationToken);

    public Task<IResult> PutAsync<TBody>(
        string baseUrl,
        string path,
        TBody body,
        CancellationToken cancellationToken) =>
        SendAsync(HttpMethod.Put, baseUrl, path, body, cancellationToken);

    public async Task<T?> ReadJsonAsync<T>(string baseUrl, string path, CancellationToken cancellationToken)
    {
        var response = await Http.GetAsync(Combine(baseUrl, path), cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken);
    }

    private async Task<IResult> SendAsync<TBody>(
        HttpMethod method,
        string baseUrl,
        string path,
        TBody? body,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(method, Combine(baseUrl, path));
        if (body is not null)
            request.Content = JsonContent.Create(body, options: JsonOptions);

        var response = await Http.SendAsync(request, cancellationToken);
        return await ToResultAsync(response, cancellationToken);
    }

    private static async Task<IResult> ToResultAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(content))
            return Results.StatusCode((int)response.StatusCode);

        return Results.Content(content, "application/json", statusCode: (int)response.StatusCode);
    }

    private static string Combine(string baseUrl, string path) =>
        $"{baseUrl.TrimEnd('/')}{path}";
}
