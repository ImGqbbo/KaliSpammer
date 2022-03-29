﻿using Newtonsoft.Json;

namespace KaliSpammer.Discord
{
    class DiscordChannel
    {
        [JsonProperty("id")]
        public string Id { get; private set; }

        [JsonProperty("name")]
        public string Name { get; private set; }

        [JsonProperty("type")]
        public int Type { get; private set; }
    }
}