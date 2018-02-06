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
    public class CodexHandler // Declare the CodexHandler class
    {
        private readonly Dictionary<ulong, CodexMessage> _messages; // Create dictionary to store all active handled messages
        private readonly DiscordSocketClient _client; // Create client variable

        public CodexHandler(DiscordSocketClient client)
        {
            _messages = new Dictionary<ulong, CodexMessage>(); // Create new dictionary in _messages
            _client = client; // Set _client to equal the client of the bot
            _client.ReactionAdded += ReactionHandler; // If reaction is added then call reaction handler
        }

        public async Task<IUserMessage> SendCodexMessage(IMessageChannel channel, CodexMessage codexMessage)
        {
            var message = await channel.SendMessageAsync("", false, codexMessage.GetEmbed()); // Sent message
            
            EmojiSet reactions = new EmojiSet(); // Get emoji set
            await message.AddReactionAsync(reactions.EmoteBACK); // Add back emote
            for (int i = 0; i < codexMessage.Count - 1; i++) // For each message in Codex Message minus 1
            {
                await message.AddReactionAsync(reactions.GetEmote(i)); // Add the number reaction
            }
            await message.AddReactionAsync(reactions.EmoteTRASH); // Add end emote
            
            _messages.Add(message.Id, codexMessage); // Add the message to the messages dictionary

            return message; // Return the message
        }

        public async Task ReactionHandler(Cacheable<IUserMessage, ulong> messageParam, ISocketMessageChannel channel, SocketReaction reactionParam)
        {
            var reactions = new EmojiSet(); // Get emoji set
            var message = await messageParam.GetOrDownloadAsync(); // Get message
            if (message == null) return; // Return if null
            if (!reactionParam.User.IsSpecified) return; // Return if the user is not specified
            if (_messages.TryGetValue(message.Id, out CodexMessage codex)) // Try to get the value of the message and store it in a variable
            {
                if (reactionParam.UserId == _client.CurrentUser.Id) return; // Return if the user is the bot
                if (codex.User != null && reactionParam.UserId != codex.User.Id) // Return if the users is not the user that initialized the command
                {
                    var _ = message.RemoveReactionAsync(reactionParam.Emote, reactionParam.User.Value); // Remove reaction
                    return;
                }
                await message.RemoveReactionAsync(reactionParam.Emote, reactionParam.User.Value); // Remove reaction
                if (reactionParam.Emote.Name == reactions.EmoteTRASH.Name)
                {
                    if (codex.EmoteStopAction == StopAction.DeleteMessage) // If the emotestop action is to delete, delete message
                        await message.DeleteAsync();
                    else if (codex.EmoteStopAction == StopAction.ClearReactions) // If the emotestop action is to clear reactions, remove all reactions
                        await message.RemoveAllReactionsAsync();
                    _messages.Remove(message.Id); // Remove message from dictionary
                }
                else if (reactionParam.Emote.Name == reactions.EmoteBACK.Name) // If the reaction is EmoteBACK, then modify the message to display the embed list
                {
                    codex.CurrentPage = codex.Count; // Set current page to the last embed (this is always the title embed)
                    await message.ModifyAsync(x => x.Embed = codex.GetEmbed()); // Modify message to title page
                }
                else
                {
                    codex.CurrentPage = reactions.GetNum(reactionParam.Emote); // Set current page to the numerical equivelent of the chosen reaction
                    await message.ModifyAsync(x => x.Embed = codex.GetEmbed()); // Modify the message to match current page
                }
            }
        }
    }

    public class CodexMessage // Declare the CodexMessage class
    {
        public CodexMessage(List<Embed> embeds, IUser user) // Constructor
        {
            Articles = embeds; // Articles equals embeds parameter
            User = user; // User equals user parameter
            CurrentPage = (embeds.Count()); // Current page is last embed
        }

        internal Embed GetEmbed() // GetEmbed method
        {
            return Articles.ElementAtOrDefault(CurrentPage - 1); // Returns the current embed
        }

        public StopAction EmoteStopAction { get; set; } = StopAction.ClearReactions; // Sets emote stop action to clear reactions
        public StopAction TimeoutAction { get; set; } = StopAction.DeleteMessage; // Sets timeout action to delete the message
        internal IReadOnlyCollection<Embed> Articles { get; } // Declares Embed collection, unmodifiable
        internal IUser User { get; } // Declares User int, unmodifiable
        internal int CurrentPage { get; set; } // Declares CurrentPage int, modifiable
        internal int Count => Articles.Count(); // Count is equal to Articles.Count()
    }

    public enum StopAction // Declares StopAction enumerable
    {
        ClearReactions,
        DeleteMessage
    }

    public class EmojiSet // Declares EmojiSet class
    {
        public const string BACK = "◀"; // Constant back unicode
        public const string ZERO = "0⃣"; // Constant zero unicode
        public const string ONE = "1⃣"; // Constant one unicode
        public const string TWO = "2⃣"; // Constant two unicode
        public const string THREE = "3⃣"; // Constant three unicode
        public const string FOUR = "4⃣"; // Constant four unicode
        public const string FIVE = "5⃣"; // Constant five unicode
        public const string SIX = "6⃣"; // Constant six unicode
        public const string SEVEN = "7⃣"; // Constant seven unicode
        public const string EIGHT = "8⃣"; // Constant eight unicode
        public const string NINE = "9⃣"; // Constant nine unicode
        public const string TRASH = "⏹"; // Constant block (stop) unicode

        public IEmote EmoteZERO { get; set; } = new Emoji(ZERO); // Public zero emote
        public IEmote EmoteONE { get; set; } = new Emoji(ONE); // Public one emote
        public IEmote EmoteTWO { get; set; } = new Emoji(TWO); // Public two emote
        public IEmote EmoteTHREE { get; set; } = new Emoji(THREE); // Public three emote
        public IEmote EmoteFOUR { get; set; } = new Emoji(FOUR); // Public four emote
        public IEmote EmoteFIVE { get; set; } = new Emoji(FIVE); // Public five emote
        public IEmote EmoteSIX { get; set; } = new Emoji(SIX); // Public six emote
        public IEmote EmoteSEVEN { get; set; } = new Emoji(SEVEN); // Public seven emote
        public IEmote EmoteEIGHT { get; set; } = new Emoji(EIGHT); // Public eight emote
        public IEmote EmoteNINE { get; set; } = new Emoji(NINE); // Public nine emote
        public IEmote EmoteTRASH { get; set; } = new Emoji(TRASH); // Public trash (stop) emote
        public IEmote EmoteBACK { get; set; } = new Emoji(BACK); // Public back emote

        public IEmote GetEmote(int num) // Declares GetEmote method, takes int parameter
        {
            switch (num) // Switch on num parameter
            {
                default: return null; // Return null as default case
                case (0): return EmoteZERO; // If 0 return EmoteZERO
                case (1): return EmoteONE; // If 1 return EmoteONE
                case (2): return EmoteTWO; // If 2 return EmoteTWO
                case (3): return EmoteTHREE; // If 3 return EmoteTHREE
                case (4): return EmoteFOUR; // If 4 return EmoteFOUR
                case (5): return EmoteFIVE; // If 5 return EmoteFIVE
                case (6): return EmoteSIX; // If 6 return EmoteSIX
                case (7): return EmoteSEVEN; // If 7 return EmoteSEVEN
                case (8): return EmoteEIGHT; // If 8 return EmoteEIGHT
                case (9): return EmoteNINE; // If 9 return EmoteNINE
            }
        }

        public int GetNum(IEmote emote) // Declares GetNum method, takes IEmote parameter
        {
            switch (emote.Name) // Switch on the name of the emote
            {
                default: return -1; // Return -1 (invalid) as default
                case (ZERO): return 1; // If the emote name is the same as ZERO return 1
                case (ONE): return 2; // If the emote name is the same as ONE return 2
                case (TWO): return 3; // If the emote name is the same as TWO return 3
                case (THREE): return 4; // If the emote name is the same as THREE return 4
                case (FOUR): return 5; // If the emote name is the same as FOUR return 5
                case (FIVE): return 6; // If the emote name is the same as FIVE return 6
                case (SIX): return 7; // If the emote name is the same as SIX return 7
                case (SEVEN): return 8; // If the emote name is the same as SEVEN return 8
                case (EIGHT): return 9; // If the emote name is the same as EIGHT return 9
                case (NINE): return 10; // If the emote name is the same as NINE return 10
            }
        }
    }
}
