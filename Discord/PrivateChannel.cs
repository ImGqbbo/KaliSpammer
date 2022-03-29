using Newtonsoft.Json;
using System.Collections.Generic;

namespace KaliSpammer.Discord
{
    class PrivateChannel
    {
        [JsonProperty("recipients")]
        public List<DiscordUser> Recipients { get; private set; }

        [JsonProperty("id")]
        public string Id { get; private set; }

        [JsonProperty("type")]
        public int Type { get; private set; }

        [JsonProperty("name")]
        public string Name { get; private set; }
    }
}
