using Newtonsoft.Json;

namespace KaliSpammer.Discord
{
    class DiscordGuild
    {
        [JsonProperty("id")]
        public string Id { get; private set; }

        [JsonProperty("name")]
        public string Name { get; private set; }
    }
}
