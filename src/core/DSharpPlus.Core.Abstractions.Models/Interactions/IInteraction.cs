// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using DSharpPlus.Entities;

using OneOf;

using Remora.Rest.Core;

namespace DSharpPlus.Core.Abstractions.Models;

/// <summary>
/// Represents an incoming interaction, received when an user submits an application command
/// or uses a message component. <br/>
/// For application commands, it includes the submitted options; <br/>
/// For context menu commands it includes the context; <br/>
/// For message components it includes information about the component as well as metadata 
/// about how the interaction was triggered.
/// </summary>
public interface IInteraction
{
    /// <summary>
    /// The snowflake identifier of this interaction.
    /// </summary>
    public Snowflake Id { get; }

    /// <summary>
    /// The snowflake identifier of the application this interaction is sent to.
    /// </summary>
    public Snowflake ApplicationId { get; }

    /// <summary>
    /// The type of this interaction.
    /// </summary>
    public DiscordInteractionType Type { get; }

    /// <summary>
    /// The data payload, depending on the <seealso cref="Type"/>.
    /// </summary>
    public Optional<OneOf<IApplicationCommandInteractionData, IMessageComponentInteractionData, IModalInteractionData>> Data { get; }

    /// <summary>
    /// The snowflake identifier of the guild this interaction was sent from, if applicable.
    /// </summary>
    public Optional<Snowflake> GuildId { get; }

    /// <summary>
    /// The channel this interaction was sent from.
    /// </summary>
    public Optional<IPartialChannel> Channel { get; }

    /// <summary>
    /// The snowflake identifier of the channel this interaction was sent from, if applicable.
    /// </summary>
    public Optional<Snowflake> ChannelId { get; }

    /// <summary>
    /// The guild member object for the invoking user, if applicable.
    /// </summary>
    public Optional<IGuildMember> Member { get; }

    /// <summary>
    /// The user object for the invokign user, if invoked in a DM.
    /// </summary>
    public Optional<IUser> User { get; }

    /// <summary>
    /// The continuation token for responding to this interaction.
    /// </summary>
    public string Token { get; }

    /// <summary>
    /// Always 1.
    /// </summary>
    public int Version { get; }

    /// <summary>
    /// For components, the message they were attached to.
    /// </summary>
    public Optional<IMessage> Message { get; }

    /// <summary>
    /// The permissions the application has within the channel the interaction was sent from.
    /// </summary>
    public Optional<DiscordPermissions> AppPermissions { get; }

    /// <summary>
    /// The selected locale of the invoking user.
    /// </summary>
    public Optional<string> Locale { get; }

    /// <summary>
    /// The preferred locale of the guild, if invoked in a guild.
    /// </summary>
    public Optional<string> GuildLocale { get; }
}
