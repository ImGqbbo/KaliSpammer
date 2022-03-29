using Newtonsoft.Json;

namespace KaliSpammer.Discord
{
    class DiscordUser
    {
        [JsonProperty("id")]
        public string Id { get; private set; }

        [JsonProperty("username")]
        public string Username { get; private set; }

        [JsonProperty("discriminator")]
        public uint Discriminator { get; private set; }

        [JsonProperty("avatar")]
        public string AvatarHash { get; private set; }
    }
}
