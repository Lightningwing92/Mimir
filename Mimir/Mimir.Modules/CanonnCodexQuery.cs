using System;
using System.Net;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord.Commands;
using Discord;
using Newtonsoft.Json.Linq;
using HtmlAgilityPack;
using Mimir.Handlers;

namespace Mimir.Modules
{
    public class CanonnCodexQuery : ModuleBase // Declare CanonnCodexQuery class, inherits ModuleBase class
    {
        private readonly CodexHandler handler; // Declares a CodexHandler

        public CanonnCodexQuery(CodexHandler codexHandler) // Constructor, takes codexHandler parameter
        {
            handler = codexHandler; // handler equals parameter
        }

        [Command("codex")] // Declares the command keyword
        [Summary("Queries the Canonn Codex for any relevant articles")] // Summary of command
        public async Task codexQuery (string query) // Declares codexQuery async task, string parameter
        {
            StringBuilder apiCall = new StringBuilder("https://canonn.science/wp-json/wp/v2/"); // Creates a string builder for the API call

            MatchCollection codexCheck = Regex.Matches(query, ".*canonn.science/codex/(.*)"); // MatchCollection for a regex match, check if the passed string is a canonn.science/codex link

            if (codexCheck.Count == 1) // Checks if the parameter is a codex entry link based off the MatchCollection
            {
                query = codexCheck[0].Groups[1].Value; // Query is link
                query = query.TrimEnd('/'); // Trims end of link
                query = query.Split('/').Last(); // Splits the link, gets the last string
                apiCall.Append("posts?slug="); // Appends posts?slug= to the apiCall
                apiCall.Append(WebUtility.UrlDecode(query)); // Appends the post name to the apiCall
            }
            else
            {
                apiCall.Append("posts?categories=2&search="); // Appends posts?categories=2&search= to the apiCall
                apiCall.Append(WebUtility.UrlEncode(query)); // Appends the query to the apiCall
            }

            string apiJson = ""; // Declares string for the apiJson
            try // Tries to run task
            {
                apiJson = Task.Run(() =>
                {
                    using (WebClient client = new WebClient()) { return client.DownloadString(apiCall.ToString()); } // Uses webclient to download the apiCall in a string format
                }).Result; // apiJson equals result of the task
            }
            catch (Exception ex) // Catch exception
            {
                await Context.Channel.SendMessageAsync("I was unable to find the specified entry."); // If failure, print "I was unable to find the specified entry."
            }

            JArray entries = JArray.Parse(apiJson); // Declare JArray for the entries, parse the apiJson string into a JArray

            List<string> titles = new List<string>(); // Declares list for article titles
            List<Embed> codexEntries = new List<Embed>(); // Declares list for codexEntries
            if (entries.Count < 9) // If the amount of entries is less than 9
            {
                for (int i = 0; i < entries.Count(); i++) // For each entry
                {
                    HtmlDocument doc = new HtmlDocument(); // Declares HtmlDocument variable
                    var builder = new EmbedBuilder(); // Declares EmbedBuilder
                    doc.LoadHtml(entries[i]["title"]["rendered"].ToString()); // Loads the title into the document
                    titles.Add(WebUtility.HtmlDecode(doc.DocumentNode.FirstChild.InnerHtml)); // Adds the title to the titles list
                    builder.WithTitle(WebUtility.HtmlDecode(doc.DocumentNode.FirstChild.InnerHtml)); // Adds the title to the builder as the embed title
                    builder.WithUrl(entries[i]["link"].ToString()); // Adds the entry URL to the embed
                    doc.LoadHtml(entries[i]["excerpt"]["rendered"].ToString()); // Loads the excerpt into the document
                    builder.AddField("Summary", WebUtility.HtmlDecode(doc.DocumentNode.FirstChild.InnerHtml)); // Adds the excerpt to the embed as a field named "Summary"

                    string FeaturedImageURL = WebUtility.HtmlDecode(entries[i]["_links"]["wp:featuredmedia"][0]["href"].ToString()); // Declare FeaturedImageURL off of JSON
                    string mediaContent; // Declares mediaContent string
                    using (WebClient client = new WebClient())
                        mediaContent = client.DownloadString(FeaturedImageURL); // Using a webclient, download the featured image JSON as a string
                    JObject mediaJson = JObject.Parse(mediaContent); // Parse mediaContent into a JObject

                    builder.WithThumbnailUrl(WebUtility.HtmlDecode(mediaJson["guid"]["rendered"].ToString())); // Adds an image thumbnail using the data from the JObject
                    codexEntries.Add(builder.Build()); // Adds the built embed to the codexEntries list
                }
            }
            else
            {
                for (int i = 0; i < 9; i++) // For 10 entries
                {
                    HtmlDocument doc = new HtmlDocument(); // Declares HtmlDocument variable
                    var builder = new EmbedBuilder(); // Declares EmbedBuilder
                    doc.LoadHtml(entries[i]["title"]["rendered"].ToString()); // Loads the title into the document
                    titles.Add(WebUtility.HtmlDecode(doc.DocumentNode.FirstChild.InnerHtml)); // Adds the title to the titles list
                    builder.WithTitle(WebUtility.HtmlDecode(doc.DocumentNode.FirstChild.InnerHtml)); // Adds the title to the builder as the embed title
                    builder.WithUrl(entries[i]["link"].ToString()); // Adds the entry URL to the embed
                    doc.LoadHtml(entries[i]["excerpt"]["rendered"].ToString()); // Loads the excerpt into the document
                    builder.AddField("Summary", WebUtility.HtmlDecode(doc.DocumentNode.FirstChild.InnerHtml)); // Adds the excerpt to the embed as a field named "Summary"

                    string FeaturedImageURL = WebUtility.HtmlDecode(entries[i]["_links"]["wp:featuredmedia"][0]["href"].ToString()); // Declare FeaturedImageURL off of JSON
                    string mediaContent; // Declares mediaContent string
                    using (WebClient client = new WebClient())
                        mediaContent = client.DownloadString(FeaturedImageURL); // Using a webclient, download the featured image JSON as a string
                    JObject mediaJson = JObject.Parse(mediaContent); // Parse mediaContent into a JObject

                    builder.WithThumbnailUrl(WebUtility.HtmlDecode(mediaJson["guid"]["rendered"].ToString())); // Adds an image thumbnail using the data from the JObject
                    codexEntries.Add(builder.Build()); // Adds the built embed to the codexEntries list
                }
            }
            var titlesEmbed = new EmbedBuilder(); // Declares EmbedBuilder named titlesEmbed
            titlesEmbed.WithTitle("Choose an Entry"); // Sets the embed title to "Choose an Entry"
            int TitleNum = 0; // Declares TitleNum int
            foreach (string s in titles) // Foreach string in titles
            {
                titlesEmbed.AddField(TitleNum.ToString(), s); // Add a field to the embed based off the "TitleNum" integer, with the contents of the title
                TitleNum++; // Increment TitleNum by 1
            }
            codexEntries.Add(titlesEmbed.Build()); // Add the built titles embed to the list of embeds
            CodexMessage codexMessage = new CodexMessage(codexEntries, Context.User); // Create codex message based off of the user and entries

            await handler.SendCodexMessage(Context.Channel, codexMessage); // Wait for the handler to send the message
        }
    }
}
