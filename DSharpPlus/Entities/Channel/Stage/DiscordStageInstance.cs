using System;
using System.Threading.Tasks;
using DSharpPlus.Exceptions;
using Newtonsoft.Json;

namespace DSharpPlus.Entities;

/// <summary>
/// Represents a discord stage instance.
/// </summary>
public sealed class DiscordStageInstance : SnowflakeObject
{
    /// <summary>
    /// Gets the guild this stage instance is in.
    /// </summary>
    [JsonIgnore]
    public DiscordGuild Guild
        => this.Discord._guilds.TryGetValue(this.GuildId, out DiscordGuild? guild) ? guild : null;

    /// <summary>
    /// Gets the id of the guild this stage instance is in.
    /// </summary>
    [JsonProperty("guild_id")]
    public ulong GuildId { get; internal set; }

    /// <summary>
    /// Gets the channel this stage instance is in.
    /// </summary>
    [JsonIgnore]
    public DiscordChannel Channel
        => (this.Discord as DiscordClient)?.InternalGetCachedChannel(this.ChannelId) ?? null;

    /// <summary>
    /// Gets the id of the channel this stage instance is in.
    /// </summary>
    [JsonProperty("channel_id")]
    public ulong ChannelId { get; internal set; }

    /// <summary>
    /// Gets the topic of this stage instance.
    /// </summary>
    [JsonProperty("topic")]
    public string Topic { get; internal set; }

    /// <summary>
    /// Gets the privacy level of this stage instance.
    /// </summary>
    [JsonProperty("privacy_level")]
    public PrivacyLevel PrivacyLevel { get; internal set; }

    /// <summary>
    /// Gets whether or not stage discovery is disabled.
    /// </summary>
    [JsonProperty("discoverable_disabled")]
    public bool DiscoverableDisabled { get; internal set; }

    /// <summary>
    /// Become speaker of current stage.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.MoveMembers"/> permission</exception>
    public async Task BecomeSpeakerAsync()
        => await this.Discord.ApiClient.BecomeStageInstanceSpeakerAsync(this.GuildId, this.Id, null);

    /// <summary>
    /// Request to become a speaker in the stage instance.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.RequestToSpeak"/> permission</exception>
    public async Task SendSpeakerRequestAsync() => await this.Discord.ApiClient.BecomeStageInstanceSpeakerAsync(this.GuildId, this.Id, null, DateTime.Now);

    /// <summary>
    /// Invite a member to become a speaker in the state instance.
    /// </summary>
    /// <param name="member">The member to invite to speak on stage.</param>
    /// <returns></returns>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.MoveMembers"/> permission</exception>
    public async Task InviteToSpeakAsync(DiscordMember member) => await this.Discord.ApiClient.BecomeStageInstanceSpeakerAsync(this.GuildId, this.Id, member.Id, null, suppress: false);
}
