// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2022 DSharpPlus Contributors
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

using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DSharpPlus.Core.RestEntities
{
    public sealed record DiscordAuditLog
    {
        /// <summary>
        /// A list of <see cref="DiscordAuditLogEntry"/>.
        /// </summary>
        [JsonProperty("audit_log_entries", NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyList<DiscordAuditLogEntry> AuditLogEntries { get; init; } = Array.Empty<DiscordAuditLogEntry>();

        /// <summary>
        /// A list of <see cref="DiscordGuildScheduledEvent"/> found in the audit log.
        /// </summary>
        [JsonProperty("guild_scheduled_events", NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyList<DiscordGuildScheduledEvent> GuildScheduledEvents { get; init; } = Array.Empty<DiscordGuildScheduledEvent>();

        /// <summary>
        /// A list of partial integration objects.
        /// </summary>
        [JsonProperty("integrations", NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyList<DiscordIntegration> Integrations { get; init; } = Array.Empty<DiscordIntegration>();

        /// <summary>
        /// A list of threads found in the audit log.
        /// </summary>
        /// <remarks>
        /// * Threads referenced in THREAD_CREATE and THREAD_UPDATE events are included in the threads map, since archived threads might not be kept in memory by clients.
        /// </remarks>
        [JsonProperty("threads", NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyList<DiscordChannel> Threads { get; init; } = Array.Empty<DiscordChannel>();

        /// <summary>
        /// A list of <see cref="DiscordUser"/> found in the audit log.
        /// </summary>
        [JsonProperty("users", NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyList<DiscordUser> Users { get; init; } = Array.Empty<DiscordUser>();

        /// <summary>
        /// A list of webhooks found in the audit log.
        /// </summary>
        [JsonProperty("webhooks", NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyList<DiscordWebhook> Webhooks { get; init; } = Array.Empty<DiscordWebhook>();
    }
}
