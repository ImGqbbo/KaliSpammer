using Newtonsoft.Json;

namespace KaliSpammer.Discord
{
    internal class EmbedField
    {
        [JsonProperty("inline")]
        public bool InLine { get; private set; }

        [JsonProperty("name")]
        public string Name { get; private set; }

        [JsonProperty("value")]
        public string Value { get; private set; }
    }
}
