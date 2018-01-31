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
    public class CanonnCodexQuery : ModuleBase
    {
        private readonly CodexHandler handler;

        public CanonnCodexQuery(CodexHandler codexHandler)
        {
            handler = codexHandler;
        }

        [Command("codex")]
        [Summary("Queries the Canonn Codex for any relevant articles")]
        public async Task codexQuery (string query)
        {
            StringBuilder apiCall = new StringBuilder("https://canonn.science/wp-json/wp/v2/");

            MatchCollection codexCheck = Regex.Matches(query, ".*canonn.science/codex/(.*)");

            if (codexCheck.Count == 1)
            {
                query = codexCheck[0].Groups[1].Value;
                query = query.TrimEnd('/');
                query = query.Split('/').Last();
                apiCall.Append("posts?slug=");
                apiCall.Append(WebUtility.UrlDecode(query));
            }
            else
            {
                apiCall.Append("posts?categories=2&search=");
                apiCall.Append(WebUtility.UrlEncode(query));
            }

            string apiJson = "";
            try
            {
                apiJson = Task.Run(() =>
                {
                    using (WebClient client = new WebClient()) { return client.DownloadString(apiCall.ToString()); }
                }).Result;
            }
            catch (Exception ex)
            {
                await Context.Channel.SendMessageAsync("I was unable to find the specified entry.");
            }

            JArray entries = JArray.Parse(apiJson);

            List<string> titles = new List<string>();
            List<Embed> codexEntries = new List<Embed>();
            if (entries.Count < 9)
            {
                for (int i = 0; i < entries.Count(); i++)
                {
                    HtmlDocument doc = new HtmlDocument();
                    var builder = new EmbedBuilder();
                    doc.LoadHtml(entries[i]["title"]["rendered"].ToString());
                    titles.Add(WebUtility.HtmlDecode(doc.DocumentNode.FirstChild.InnerHtml));
                    builder.WithTitle(WebUtility.HtmlDecode(doc.DocumentNode.FirstChild.InnerHtml));
                    builder.WithUrl(entries[i]["link"].ToString());
                    doc.LoadHtml(entries[i]["excerpt"]["rendered"].ToString());
                    builder.AddField("Summary", WebUtility.HtmlDecode(doc.DocumentNode.FirstChild.InnerHtml));

                    string FeaturedImageURL = WebUtility.HtmlDecode(entries[0]["_links"]["wp:featuredmedia"][0]["href"].ToString());
                    string mediaContent;
                    using (WebClient client = new WebClient())
                        mediaContent = client.DownloadString(FeaturedImageURL);
                    JObject mediaJson = JObject.Parse(mediaContent);

                    builder.WithThumbnailUrl(WebUtility.HtmlDecode(mediaJson["guid"]["rendered"].ToString()));
                    codexEntries.Add(builder.Build());
                }
            }
            else
            {
                for (int i = 0; i < 9; i++)
                {
                    HtmlDocument doc = new HtmlDocument();
                    var builder = new EmbedBuilder();
                    doc.LoadHtml(entries[i]["title"]["rendered"].ToString());
                    titles.Add(WebUtility.HtmlDecode(doc.DocumentNode.FirstChild.InnerHtml));
                    builder.WithTitle(WebUtility.HtmlDecode(doc.DocumentNode.FirstChild.InnerHtml));
                    builder.WithUrl(entries[i]["link"].ToString());
                    doc.LoadHtml(entries[i]["excerpt"]["rendered"].ToString());
                    builder.AddField("Summary", WebUtility.HtmlDecode(doc.DocumentNode.FirstChild.InnerHtml));

                    string FeaturedImageURL = WebUtility.HtmlDecode(entries[i]["_links"]["wp:featuredmedia"][0]["href"].ToString());
                    string mediaContent;
                    using (WebClient client = new WebClient())
                        mediaContent = client.DownloadString(FeaturedImageURL);
                    JObject mediaJson = JObject.Parse(mediaContent);

                    builder.WithThumbnailUrl(WebUtility.HtmlDecode(mediaJson["guid"]["rendered"].ToString()));
                    codexEntries.Add(builder.Build());
                }
            }
            var titlesEmbed = new EmbedBuilder();
            titlesEmbed.WithTitle("Choose an Entry");
            int TitleNum = 0;
            foreach (string s in titles)
            {
                titlesEmbed.AddField(TitleNum.ToString(), s);
                TitleNum++;
            }
            codexEntries.Add(titlesEmbed.Build());
            //await Context.Channel.SendMessageAsync("Made debug pointer 1");
            CodexMessage codexMessage = new CodexMessage(codexEntries, Context.User);

            await handler.SendCodexMessage(Context.Channel, codexMessage);

            /*string CodexLink = entries[0]["link"].ToString();
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(entries[0]["title"]["rendered"].ToString());
            string CodexTitle = WebUtility.HtmlDecode(doc.DocumentNode.FirstChild.InnerHtml);
            doc.LoadHtml(entries[0]["excerpt"]["rendered"].ToString());
            string CodexExcerpt = WebUtility.HtmlDecode(doc.DocumentNode.FirstChild.InnerHtml);

            string FeaturedImageURL = WebUtility.HtmlDecode(entries[0]["_links"]["wp:featuredmedia"][0]["href"].ToString());
            string mediaContent;
            using (WebClient client = new WebClient())
            {
                mediaContent = client.DownloadString(FeaturedImageURL);
            }
            JObject mediaJson = JObject.Parse(mediaContent);
            

            var embed = new EmbedBuilder();
            embed.WithTitle(CodexTitle);
            embed.WithUrl(CodexLink);
            embed.AddField("Summary", CodexExcerpt);
            embed.WithThumbnailUrl(WebUtility.HtmlDecode(mediaJson["guid"]["rendered"].ToString()));

            await Context.Channel.SendMessageAsync("", false, embed);*/
        }
    }
}
