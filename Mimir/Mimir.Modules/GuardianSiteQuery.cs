using System;
using System.Net;
using System.Threading.Tasks;
using Discord.Commands;
using Discord;
using Newtonsoft.Json.Linq;

namespace Mimir.Modules
{
    public class GuardianSiteQuery : ModuleBase
    {
        [Command("gs")]
        [Summary("Queries a Guardian Site from Canonn R&D's API")]
        public async Task GS(int ID)
        {
            //Declares contents string, and downloads all information from the API using a WebClient, then parses it into a JObject
            string contents;
            using (var client = new WebClient())
            {
                contents = client.DownloadString("http://api.canonn.technology:3000/v2/gr_sites/" + ID.ToString());
                client.Dispose();
            }
            JObject site = JObject.Parse(contents);
            using (var client = new WebClient())
            {
                contents = client.DownloadString("http://api.canonn.technology:3000/v2/locations/" + site["locationid"].ToString());
                client.Dispose();
            }
            JObject location = JObject.Parse(contents);
            using (var client = new WebClient())
            {
                contents = client.DownloadString("http://api.canonn.technology:3000/v2/systems/" + location["systemid"].ToString());
                client.Dispose();
            }
            JObject system = JObject.Parse(contents);
            using (var client = new WebClient())
            {
                contents = client.DownloadString("http://api.canonn.technology:3000/v2/bodies/" + location["bodyid"].ToString());
                client.Dispose();
            }
            JObject body = JObject.Parse(contents);

            //Format and send embed
            var embed = new EmbedBuilder();
            embed.WithTitle("Guardian Site " + ID.ToString());
            embed.AddField("System", system["name"].ToString());
            embed.AddField("Planet", body["name"].ToString());
            embed.AddField("Latitude", location["latitude"].ToString());
            embed.AddField("Longitude", location["longitude"].ToString());
            embed.WithUrl("https://ruins.canonn.technology/#GR" + ID.ToString());

            await Context.Channel.SendMessageAsync("", false, embed);
        }
    }
}
