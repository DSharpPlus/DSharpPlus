// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2021 DSharpPlus Contributors
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System.Collections.Generic;
using Newtonsoft.Json;

namespace DSharpPlus.Net.Abstractions
{
    internal sealed class TransportApplication
    {
        [JsonProperty("id", NullValueHandling = NullValueHandling.Include)]
        public ulong Id { get; set; }

        [JsonProperty("name", NullValueHandling = NullValueHandling.Include)]
        public string Name { get; set; }

        [JsonProperty("icon", NullValueHandling = NullValueHandling.Include)]
        public string IconHash { get; set; }

        [JsonProperty("description", NullValueHandling = NullValueHandling.Include)]
        public string Description { get; set; }

        [JsonProperty("summary", NullValueHandling = NullValueHandling.Include)]
        public string Summary { get; set; }

        [JsonProperty("bot_public", NullValueHandling = NullValueHandling.Include)]
        public bool IsPublicBot { get; set; }

        [JsonProperty("bot_require_code_grant", NullValueHandling = NullValueHandling.Include)]
        public bool BotRequiresCodeGrant { get; set; }

        [JsonProperty("terms_of_service_url", NullValueHandling = NullValueHandling.Ignore)]
        public string TermsOfServiceUrl { get; set; }

        [JsonProperty("privacy_policy_url", NullValueHandling = NullValueHandling.Ignore)]
        public string PrivacyPolicyUrl { get; set; }

        // Json.NET can figure the type out
        [JsonProperty("rpc_origins", NullValueHandling = NullValueHandling.Ignore)]
        public IList<string> RpcOrigins { get; set; }

        [JsonProperty("owner", NullValueHandling = NullValueHandling.Include)]
        public TransportUser Owner { get; set; }

        [JsonProperty("team", NullValueHandling = NullValueHandling.Include)]
        public TransportTeam Team { get; set; }

        [JsonProperty("flags", NullValueHandling = NullValueHandling.Ignore)]
        public ApplicationFlags? Flags { get; set; }

        // These are dispatch (store) properties - can't imagine them being needed in bots
        //[JsonProperty("verify_key", NullValueHandling = NullValueHandling.Include)]
        //public string VerifyKey { get; set; }

        //[JsonProperty("guild_id")]
        //public Optional<ulong> GuildId { get; set; }

        //[JsonProperty("primary_sku_id")]
        //public Optional<ulong> PrimarySkuId { get; set; }

        //[JsonProperty("slug")] // sluggg :DDDDDD
        //public Optional<string> SkuSlug { get; set; }

        //[JsonProperty("cover_image")]
        //public Optional<string> CoverImageHash { get; set; }

        internal TransportApplication() { }
    }
}
