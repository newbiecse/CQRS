using Chat.Application.Abstractions;
using Chat.Application.Options;
using Chat.Infrastructure.Agents;
using Chat.Infrastructure.Catalog;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Chat.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddChatInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ChatAgentOptions>(configuration.GetSection(ChatAgentOptions.SectionName));
        services.AddHttpClient<IShopContextProvider, HttpShopContextProvider>();
        services.AddHttpClient("openai-chat");
        services.AddSingleton<IChatAgentService, ShopChatAgentService>();
        return services;
    }
}
