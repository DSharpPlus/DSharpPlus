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

using System.Text.Json.Serialization;

namespace DSharpPlus.Core.Entities
{
    public sealed record DiscordAuditLog
    {
        /// <summary>
        /// A list of <see cref="DiscordAuditLogEntry"/>.
        /// </summary>
        [JsonPropertyName("audit_log_entries")]
        public DiscordAuditLogEntry[] AuditLogEntries { get; init; } = null!;

        /// <summary>
        /// A list of <see cref="DiscordGuildScheduledEvent"/> found in the audit log.
        /// </summary>
        [JsonPropertyName("guild_scheduled_events")]
        public DiscordGuildScheduledEvent[] GuildScheduledEvents { get; init; } = null!;

        /// <summary>
        /// A list of partial integration objects.
        /// </summary>
        [JsonPropertyName("integrations")]
        public DiscordIntegration[] Integrations { get; init; } = null!;

        /// <summary>
        /// A list of threads found in the audit log.
        /// </summary>
        /// <remarks>
        /// * Threads referenced in THREAD_CREATE and THREAD_UPDATE events are included in the threads map, since archived threads might not be kept in memory by clients.
        /// </remarks>
        [JsonPropertyName("threads")]
        public DiscordChannel[] Threads { get; init; } = null!;

        /// <summary>
        /// A list of <see cref="DiscordUser"/> found in the audit log.
        /// </summary>
        [JsonPropertyName("users")]
        public DiscordUser[] Users { get; init; } = null!;

        /// <summary>
        /// A list of webhooks found in the audit log.
        /// </summary>
        [JsonPropertyName("webhooks")]
        public DiscordWebhook[] Webhooks { get; init; } = null!;
    }
}
