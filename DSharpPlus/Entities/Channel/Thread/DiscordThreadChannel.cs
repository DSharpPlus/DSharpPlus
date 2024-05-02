
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Exceptions;
using DSharpPlus.Net.Models;
using Newtonsoft.Json;

namespace DSharpPlus.Entities;
/// <summary>
/// Represents a Discord thread in a channel.
/// </summary>
public class DiscordThreadChannel : DiscordChannel
{
    /// <summary>
    /// Gets the ID of this thread's creator.
    /// </summary>
    [JsonProperty("owner_id", NullValueHandling = NullValueHandling.Ignore)]
    public ulong CreatorId { get; internal set; }

    /// <summary>
    /// Gets the approximate count of messages in a thread, capped to 50.
    /// </summary>
    [JsonProperty("message_count", NullValueHandling = NullValueHandling.Ignore)]
    public int? MessageCount { get; internal set; }

    /// <summary>
    /// Gets the approximate count of members in a thread, capped to 50.
    /// </summary>
    [JsonProperty("member_count", NullValueHandling = NullValueHandling.Ignore)]
    public int? MemberCount { get; internal set; }

    /// <summary>
    /// Represents the current member for this thread. This will have a value if the user has joined the thread.
    /// </summary>
    [JsonProperty("member", NullValueHandling = NullValueHandling.Ignore)]
    public DiscordThreadChannelMember CurrentMember { get; internal set; }

    /// <summary>
    /// Gets the approximate count of members in a thread, up to 50.
    /// </summary>
    [JsonProperty("thread_metadata", NullValueHandling = NullValueHandling.Ignore)]
    public DiscordThreadChannelMetadata ThreadMetadata { get; internal set; }

    /// <summary>
    /// Gets whether this thread has been newly created. This property is not populated when fetched by REST.
    /// </summary>
    [JsonProperty("newly_created", NullValueHandling = NullValueHandling.Ignore)]
    public bool IsNew { get; internal set; }

    /// <summary>
    /// Gets the tags applied to this forum post.
    /// </summary>
    // Performant? No. Ideally, you're not using this property often.
    public IReadOnlyList<DiscordForumTag> AppliedTags =>
        Parent is DiscordForumChannel parent
            ? parent.AvailableTags.Where(pt => _appliedTagIds.Contains(pt.Id)).ToArray()
            : [];

    [JsonProperty("applied_tags")]
    private readonly List<ulong> _appliedTagIds;

    #region Methods

    /// <summary>
    /// Makes the current user join the thread.
    /// </summary>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task JoinThreadAsync()
        => await Discord.ApiClient.JoinThreadAsync(Id);

    /// <summary>
    /// Makes the current user leave the thread.
    /// </summary>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task LeaveThreadAsync()
        => await Discord.ApiClient.LeaveThreadAsync(Id);

    /// <summary>
    /// Returns a full list of the thread members in this thread.
    /// Requires the <see cref="DiscordIntents.GuildMembers"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    /// <returns>A collection of all threads members in this thread.</returns>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<IReadOnlyList<DiscordThreadChannelMember>> ListJoinedMembersAsync()
        => await Discord.ApiClient.ListThreadMembersAsync(Id);

    /// <summary>
    /// Adds the given DiscordMember to this thread. Requires an not archived thread and send message permissions.
    /// </summary>
    /// <param name="member">The member to add to the thread.</param>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.SendMessages"/>.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task AddThreadMemberAsync(DiscordMember member)
    {
        if (ThreadMetadata.IsArchived)
        {
            throw new InvalidOperationException("You cannot add members to an archived thread.");
        }

        await Discord.ApiClient.AddThreadMemberAsync(Id, member.Id);
    }

    /// <summary>
    /// Removes the given DiscordMember from this thread. Requires an not archived thread and send message permissions.
    /// </summary>
    /// <param name="member">The member to remove from the thread.</param>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageThreads"/> permission, or is not the creator of the thread if it is private.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task RemoveThreadMemberAsync(DiscordMember member)
    {
        if (ThreadMetadata.IsArchived)
        {
            throw new InvalidOperationException("You cannot remove members from an archived thread.");
        }

        await Discord.ApiClient.RemoveThreadMemberAsync(Id, member.Id);
    }

    /// <summary>
    /// Modifies the current thread.
    /// </summary>
    /// <param name="action">Action to perform on this thread</param>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageChannels"/> permission.</exception>
    /// <exception cref="NotFoundException">Thrown when the channel does not exist.</exception>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task ModifyAsync(Action<ThreadChannelEditModel> action)
    {
        ThreadChannelEditModel mdl = new();
        action(mdl);
        await Discord.ApiClient.ModifyThreadChannelAsync(Id, mdl.Name, mdl.Position, mdl.Topic, mdl.Nsfw,
            mdl.Parent.HasValue ? mdl.Parent.Value?.Id : default(Optional<ulong?>), mdl.Bitrate, mdl.Userlimit, mdl.PerUserRateLimit, mdl.RtcRegion.IfPresent(r => r?.Id),
            mdl.QualityMode, mdl.Type, mdl.PermissionOverwrites, mdl.IsArchived, mdl.AutoArchiveDuration, mdl.Locked, mdl.AppliedTags, mdl.IsInvitable, mdl.AuditLogReason);

        // We set these *after* the rest request so that Discord can validate the properties. This is useful if the requirements ever change.
        if (!string.IsNullOrWhiteSpace(mdl.Name))
        {
            Name = mdl.Name;
        }

        if (mdl.PerUserRateLimit.HasValue)
        {
            PerUserRateLimit = mdl.PerUserRateLimit.Value;
        }

        if (mdl.IsArchived.HasValue)
        {
            ThreadMetadata.IsArchived = mdl.IsArchived.Value;
        }

        if (mdl.AutoArchiveDuration.HasValue)
        {
            ThreadMetadata.AutoArchiveDuration = mdl.AutoArchiveDuration.Value;
        }

        if (mdl.Locked.HasValue)
        {
            ThreadMetadata.IsLocked = mdl.Locked.Value;
        }
    }

    /// <summary>
    /// Returns a thread member object for the specified user if they are a member of the thread, returns a 404 response otherwise.
    /// </summary>
    /// <param name="member">The guild member to retrieve.</param>
    /// <exception cref="NotFoundException">Thrown when a GuildMember has not joined the channel thread.</exception>
    public async Task<DiscordThreadChannelMember> GetThreadMemberAsync(DiscordMember member)
        => await Discord.ApiClient.GetThreadMemberAsync(Id, member.Id);

    #endregion

    internal DiscordThreadChannel() { }
}
