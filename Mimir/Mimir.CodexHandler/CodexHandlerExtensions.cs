using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Discord.WebSocket;

namespace Mimir.Handlers
{
    public static class CodexHandlerExtensions // Declare CodexHandlerExtension class
    {
        public static IServiceCollection AddCodexHandler(this IServiceCollection collection, DiscordSocketClient client) // Declare AddCodexHandler method, takes client parameter
        {
            collection.AddSingleton(new CodexHandler(client)); // Add a new CodexHandler with teh client as a singleton to the collection
            return collection; // Return collection
        }
    }
}
