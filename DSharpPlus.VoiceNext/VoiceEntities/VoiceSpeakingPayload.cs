﻿using Newtonsoft.Json;

namespace DSharpPlus.VoiceNext.VoiceEntities
{
    internal sealed class VoiceSpeakingPayload
    {
        [JsonProperty("speaking")]
        public bool Speaking { get; set; }

        [JsonProperty("delay", NullValueHandling = NullValueHandling.Ignore)]
        public int? Delay { get; set; }

        [JsonProperty("ssrc", NullValueHandling = NullValueHandling.Ignore)]
        public uint? Ssrc { get; set; }

        [JsonProperty("user_id", NullValueHandling = NullValueHandling.Ignore)]
        public ulong? UserId { get; set; }
    }
}
