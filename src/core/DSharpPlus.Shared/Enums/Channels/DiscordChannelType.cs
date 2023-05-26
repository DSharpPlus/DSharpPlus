// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace DSharpPlus.Entities;

/// <summary>
/// Enumerates the valid types for a channel.
/// </summary>
public enum DiscordChannelType
{
    /// <summary>
    /// A text channel within a guild.
    /// </summary>
    GuildText,

    /// <summary>
    /// A direct message between users.
    /// </summary>
    Dm,

    /// <summary>
    /// A voice channel within a guild.
    /// </summary>
    GuildVoice,

    /// <summary>
    /// A direct message between multiple users.
    /// </summary>
    GroupDm,

    /// <summary>
    /// A category channel within a guild that contains up to 50 channels.
    /// </summary>
    GuildCategory,

    /// <summary>
    /// A channel that users can follow and crosspost into their own guilds.
    /// </summary>
    GuildAnnouncement,

    /// <summary>
    /// A thread channel within an announcement channel.
    /// </summary>
    AnnouncementThread = 10,

    /// <summary>
    /// A public thread channel in a text or forum channel.
    /// </summary>
    PublicThread,

    /// <summary>
    /// A private thread channel in a text channel.
    /// </summary>
    PrivateThread,

    /// <summary>
    /// A stage channel.
    /// </summary>
    GuildStageVoice,

    /// <summary>
    /// A list of servers in a hub.
    /// </summary>
    GuildDirectory,

    /// <summary>
    /// A channel that can only contain public threads.
    /// </summary>
    GuildForum
}
