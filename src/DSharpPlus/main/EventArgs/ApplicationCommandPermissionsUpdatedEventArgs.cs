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

namespace DSharpPlus.EventArgs;

public class ApplicationCommandPermissionsUpdatedEventArgs : DiscordEventArgs
{
    /// <summary>
    /// The Id of the guild the command was updated for.
    /// </summary>
    [JsonProperty("guild_id")]
    public ulong GuildId { get; internal set; }

    /// <summary>
    /// The Id of the command that was updated.
    /// </summary>
    [JsonProperty("id")]
    public ulong CommandId { get; internal set; }

    /// <summary>
    /// The Id of the application the command was updated for.
    /// </summary>
    [JsonProperty("application_id")]
    public ulong ApplicationId { get; internal set; }

    /// <summary>
    /// The new permissions for the command.
    /// </summary>
    [JsonProperty("permissions")]
    public IReadOnlyList<ApplicationCommandPermissionUpdate> NewPermissions { get; internal set; }
}

public class ApplicationCommandPermissionUpdate
{
    /// <summary>
    /// The Id of the entity this permission is for.
    /// </summary>
    [JsonProperty("id")]
    public ulong Id { get; internal set; }

    /// <summary>
    /// Whether the role/user/channel [or anyone in the channel/with the role] is allowed to use the command.
    /// </summary>
    [JsonProperty("permission")]
    public bool Allow { get; internal set; }

    /// <summary>
    ///
    /// </summary>
    [JsonProperty("type")]
    public ApplicationCommandPermissionType Type { get; internal set; }
}
