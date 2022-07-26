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
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DSharpPlus
{
    /// <summary>
    /// Discord OAuth2 Scopes. As represented by <see href="https://discord.com/developers/docs/topics/oauth2#shared-resources-oauth2-scopes"/>
    /// </summary>
    public enum DiscordScopes : ulong
    {
        // note: these should represent the scope, but in camelcase instead
        // activities.read would become ActivitiesRead
        ActivitiesRead,
        ActivitiesWrite,
        ApplicationsBuildsRead,
        ApplicationsBuildsUpload,
        ApplicationsCommands,
        ApplicationsCommandsUpdate,
        ApplicationsCommandsPermissionsUpdate,
        ApplicationsEntitlements,
        ApplicationsStoreUpdate,
        Bot,
        Connections,
        Dm_channelsRead,
        Email,
        GdmJoin,
        Guilds,
        GuildsJoin,
        GuildsMembersRead,
        Identify,
        MessagesRead,
        RelationshipsRead,
        Rpc,
        RpcActivitiesWrite,
        RpcNotificationsRead,
        RpcVoiceRead,
        RpcVoiceWrite,
        Voice,
        WebhookIncoming
    }
}
