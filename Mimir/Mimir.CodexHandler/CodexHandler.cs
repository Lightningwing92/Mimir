using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace Mimir.Handlers
{
    public class CodexHandler
    {
        private readonly Dictionary<ulong, CodexMessage> _messages;
        private readonly DiscordSocketClient _client;

        public CodexHandler(DiscordSocketClient client)
        {
            _messages = new Dictionary<ulong, CodexMessage>();
            _client = client;
            _client.ReactionAdded += reactionHandler;
        }

        public async Task<IUserMessage> SendCodexMessage(IMessageChannel channel, CodexMessage codexMessage)
        {
            //await channel.SendMessageAsync("Made debug point 2");
            var message = await channel.SendMessageAsync("", false, codexMessage.GetEmbed());
            
            EmojiSet reactions = new EmojiSet();
            await message.AddReactionAsync(reactions.EmoteBACK);
            for (int i = 0; i < codexMessage.Count - 1; i++)
            {
                await message.AddReactionAsync(reactions.GetEmote(i));
            }
            await message.AddReactionAsync(reactions.EmoteTRASH);
            //await channel.SendMessageAsync("Made debug point 3");

            _messages.Add(message.Id, codexMessage);
            //await channel.SendMessageAsync("Made debug point 4");

            return message;
        }

        public async Task reactionHandler(Cacheable<IUserMessage, ulong> messageParam, ISocketMessageChannel channel, SocketReaction reactionParam)
        {
            var reactions = new EmojiSet();
            var message = await messageParam.GetOrDownloadAsync();
            if (message == null) return;
            if (!reactionParam.User.IsSpecified) return;
            if (_messages.TryGetValue(message.Id, out CodexMessage codex))
            {
                if (reactionParam.UserId == _client.CurrentUser.Id) return;
                if (codex.User != null && reactionParam.UserId != codex.User.Id)
                {
                    var _ = message.RemoveReactionAsync(reactionParam.Emote, reactionParam.User.Value);
                    return;
                }
                await message.RemoveReactionAsync(reactionParam.Emote, reactionParam.User.Value);
                if (reactionParam.Emote.Name == reactions.EmoteTRASH.Name)
                {
                    if (codex.EmoteStopAction == StopAction.DeleteMessage)
                        await message.DeleteAsync();
                    else if (codex.EmoteStopAction == StopAction.ClearReactions)
                        await message.RemoveAllReactionsAsync();
                    _messages.Remove(message.Id);
                }
                else if (reactionParam.Emote.Name == reactions.EmoteBACK.Name)
                {
                    codex.CurrentPage = codex.Count;
                    await message.ModifyAsync(x => x.Embed = codex.GetEmbed());
                }
                else
                {
                    codex.CurrentPage = reactions.GetNum(reactionParam.Emote);
                    await message.ModifyAsync(x => x.Embed = codex.GetEmbed());
                }
            }
        }
    }

    public class CodexMessage
    {
        public CodexMessage(List<Embed> embeds, IUser user)
        {
            Articles = embeds;
            User = user;
            CurrentPage = (embeds.Count());
        }

        internal Embed GetEmbed()
        {
            return Articles.ElementAtOrDefault(CurrentPage - 1);
        }

        public StopAction EmoteStopAction { get; set; } = StopAction.ClearReactions;
        public StopAction TimeoutAction { get; set; } = StopAction.DeleteMessage;
        internal IReadOnlyCollection<Embed> Articles { get; }
        internal IUser User { get; }
        internal int CurrentPage { get; set; }
        internal int Count => Articles.Count();
    }

    public enum StopAction
    {
        ClearReactions,
        DeleteMessage
    }

    public class EmojiSet
    {
        public const string BACK = "◀";
        public const string ZERO = "0⃣";
        public const string ONE = "1⃣";
        public const string TWO = "2⃣";
        public const string THREE = "3⃣";
        public const string FOUR = "4⃣";
        public const string FIVE = "5⃣";
        public const string SIX = "6⃣";
        public const string SEVEN = "7⃣";
        public const string EIGHT = "8⃣";
        public const string NINE = "9⃣";
        public const string TRASH = "⏹";

        public IEmote EmoteZERO { get; set; } = new Emoji(ZERO);
        public IEmote EmoteONE { get; set; } = new Emoji(ONE);
        public IEmote EmoteTWO { get; set; } = new Emoji(TWO);
        public IEmote EmoteTHREE { get; set; } = new Emoji(THREE);
        public IEmote EmoteFOUR { get; set; } = new Emoji(FOUR);
        public IEmote EmoteFIVE { get; set; } = new Emoji(FIVE);
        public IEmote EmoteSIX { get; set; } = new Emoji(SIX);
        public IEmote EmoteSEVEN { get; set; } = new Emoji(SEVEN);
        public IEmote EmoteEIGHT { get; set; } = new Emoji(EIGHT);
        public IEmote EmoteNINE { get; set; } = new Emoji(NINE);
        public IEmote EmoteTRASH { get; set; } = new Emoji(TRASH);
        public IEmote EmoteBACK { get; set; } = new Emoji(BACK);

        public IEmote GetEmote(int num)
        {
            switch (num)
            {
                default: return null;
                case (0): return EmoteZERO;
                case (1): return EmoteONE;
                case (2): return EmoteTWO;
                case (3): return EmoteTHREE;
                case (4): return EmoteFOUR;
                case (5): return EmoteFIVE;
                case (6): return EmoteSIX;
                case (7): return EmoteSEVEN;
                case (8): return EmoteEIGHT;
                case (9): return EmoteNINE;
            }
        }

        public int GetNum(IEmote emote)
        {
            switch (emote.Name)
            {
                default: return -1;
                case (ZERO): return 1;
                case (ONE): return 2;
                case (TWO): return 3;
                case (THREE): return 4;
                case (FOUR): return 5;
                case (FIVE): return 6;
                case (SIX): return 7;
                case (SEVEN): return 8;
                case (EIGHT): return 9;
                case (NINE): return 10;
            }
        }
    }
}
