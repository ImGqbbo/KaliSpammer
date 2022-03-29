using Newtonsoft.Json;
using System.Collections.Generic;

namespace KaliSpammer.Discord
{
    class DiscordMessage
    {
        [JsonProperty("id")]
        public string Id { get; private set; }

        [JsonProperty("author")]
        public DiscordUser Author { get; private set; }

        [JsonProperty("tts")]
        public bool Tts { get; private set; }

        [JsonProperty("content")]
        public string Content { get; private set; }

        [JsonProperty("mentions")]
        public List<DiscordUser> Mentions { get; private set; }

        [JsonProperty("embeds")]
        public List<DiscordEmbed> Embeds { get; private set; }
    }
}