using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Discord.WebSocket;

namespace Mimir.Handlers
{
    public static class CodexHandlerExtensions
    {
        public static IServiceCollection AddCodexHandler(this IServiceCollection collection, DiscordSocketClient client)
        {
            collection.AddSingleton(new CodexHandler(client));
            return collection;
        }
    }
}
