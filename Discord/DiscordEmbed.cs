using Newtonsoft.Json;
using System.Collections.Generic;

namespace KaliSpammer.Discord
{
    class DiscordEmbed
    {
        [JsonProperty("title")]
        public string Title { get; private set; }

        [JsonProperty("description")]
        public string Description { get; private set; }

        [JsonProperty("url")]
        public string Url { get; private set; }

        [JsonProperty("color")]
        public int Color { get; private set; }

        [JsonProperty("fields")]
        public List<EmbedField> Fields { get; private set; }
    }
}
