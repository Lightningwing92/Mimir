using System;
using System.Net;
using System.Threading.Tasks;
using Discord.Commands;
using Discord;
using Newtonsoft.Json.Linq;

namespace Mimir.Modules
{
    public class ThargoidStructureQuery : ModuleBase
    {
        [Command("ts")]
        [Summary("Queries JSON file for Thargoid Structure based off of ID")]
        public async Task TS(int ID)
        {
            //Contents string declared, used by the WebClients in order to download API information, which is then parsed and stored by a JObject
            string contents;
            using (WebClient client = new WebClient())
            {
                contents = client.DownloadString("http://api.canonn.technology:3000/v2/ts_sites/" + ID.ToString());
                client.Dispose();
            }
            JObject structure = JObject.Parse(contents);
            using (WebClient client = new WebClient())
            {
                contents = client.DownloadString("http://api.canonn.technology:3000/v2/locations/" + structure["locationid"].ToString());
                client.Dispose();
            }
            JObject location = JObject.Parse(contents);
            using (WebClient client = new WebClient())
            {
                contents = client.DownloadString("http://api.canonn.technology:3000/v2/systems/" + location["systemid"].ToString());
                client.Dispose();
            }
            JObject system = JObject.Parse(contents);
            using (WebClient client = new WebClient())
            {
                contents = client.DownloadString("http://api.canonn.technology:3000/v2/bodies/" + location["bodyid"].ToString());
                client.Dispose();
            }
            JObject body = JObject.Parse(contents);

            //Format and send embed
            var embed = new EmbedBuilder();
            embed.WithTitle("Thargoid Structure " + ID.ToString());
            embed.AddField("System", system["name"]);
            embed.AddField("Body", body["name"]);
            embed.AddField("Latitude", location["latitude"]);
            embed.AddField("Longitude", location["longitude"]);
            if (structure["statusid"].ToString() == "2")
            {
                embed.AddField("Active", "True");
            }
            else if (structure["statusid"].ToString() == "1")
            {
                embed.AddField("Active", "False");
            }

            await Context.Channel.SendMessageAsync("", false, embed);
        }
    }
}
