using Newtonsoft.Json;

namespace KaliSpammer.Discord
{
    class DiscordInvite
    {
        [JsonProperty("guild")]
        public DiscordGuild Guild { get; private set; }

        [JsonProperty("channel")]
        public DiscordChannel Channel { get; private set; }

        [JsonProperty("code")]
        public string Code { get; private set; }
    }
}
