using System;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using Mimir.Modules;

namespace Mimir
{
    class Program
    {
        //Initialize CommandService, Client and ServiceProvider
        private CommandService _commands;
        private DiscordSocketClient _mainClient;
        private IServiceProvider _services;

        //Main Async lambda
        public static void Main(string[] args) => new Program().MainAsync().GetAwaiter().GetResult();
        
        //Main async task
        public async Task MainAsync()
        {
            //Create new CommandService and Client
            _mainClient = new DiscordSocketClient();
            _commands = new CommandService();

            //Initialize string for the token, and read it from a file "bot.token" in the same directory as the EXE
            string token;

            using (var reader = new StreamReader("bot.token"))
            {
                token = reader.ReadLine();
                reader.Close();
            }
            
            //Run "InstallAsync" then login to the client using the bot token
            await InstallAsync();
            await _mainClient.LoginAsync(TokenType.Bot, token);
            await _mainClient.StartAsync();

            //Handle each message received
            _mainClient.MessageReceived += HandleCommand;

            await Task.Delay(-1);
        }

        public async Task InstallAsync()
        {
            //Load modules adding dependencies
            _services = new ServiceCollection()
                .AddSingleton(_mainClient)
                .AddSingleton(_commands)
                .BuildServiceProvider();

            await _commands.AddModulesAsync(Assembly.GetEntryAssembly());
        }

        public async Task HandleCommand(SocketMessage messageParam)
        {
            //Read command as SocketUserMessage, return if message is null, return if message is sent by the bot
            var message = messageParam as SocketUserMessage;
            if (message == null) return;
            if (message.Author.Id == _mainClient.CurrentUser.Id) return;

            //Argpos for the command
            int argPos = 0;

            //Check to ensure that the prefix is either a mention or ! (subject to change, want to add in dynamic bangs on a server - server basis
            if (!(message.HasCharPrefix('!', ref argPos) || message.HasMentionPrefix(_mainClient.CurrentUser, ref argPos))) return;

            //New socket command context using the main client and message
            var context = new SocketCommandContext(_mainClient, message);

            //Execute the command, ensure that it is a success
            var result = await _commands.ExecuteAsync(context, argPos, _services);
            if (!result.IsSuccess)
                await context.Channel.SendMessageAsync(result.ErrorReason);
        }
    }
}