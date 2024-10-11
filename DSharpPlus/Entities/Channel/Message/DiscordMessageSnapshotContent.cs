using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace DSharpPlus.Entities;

/// <summary>
/// Represents a Discord text message snapshot.
/// </summary>
public class DiscordMessageSnapshotContent
{
    internal DiscordMessageSnapshotContent()
    {

    }

    internal DiscordMessageSnapshotContent(DiscordMessageSnapshotContent other)
        : this()
    {
        this.attachments = new List<DiscordAttachment>(other.attachments);
        this.embeds = new List<DiscordEmbed>(other.embeds);

        if (other.mentionedChannels is not null)
        {
            this.mentionedChannels = new List<DiscordChannel>(other.mentionedChannels);
        }

        if (other.mentionedRoles is not null)
        {
            this.mentionedRoles = new List<DiscordRole>(other.mentionedRoles);
        }

        if (other.mentionedRoleIds is not null)
        {
            this.mentionedRoleIds = new List<ulong>(other.mentionedRoleIds);
        }

        this.mentionedUsers = new List<DiscordUser>(other.mentionedUsers);
        this.stickers = new List<DiscordMessageSticker>(other.stickers);

        this.Content = other.Content;
        this.EditedTimestamp = other.EditedTimestamp;
        this.MessageType = other.MessageType;
        this.Timestamp = other.Timestamp;
        this.Components = other.Components;
    }

    /// <summary>
    /// Gets the components this message was sent with.
    /// </summary>
    [JsonProperty("components", NullValueHandling = NullValueHandling.Ignore)]
    public IReadOnlyList<DiscordComponent>? Components { get; internal set; }

    /// <summary>
    /// Gets the action rows this message was sent with - components holding buttons, selects and the likes.
    /// </summary>
    public IReadOnlyList<DiscordActionRowComponent>? ComponentActionRows
        => this.Components?.Where(x => x is DiscordActionRowComponent).Cast<DiscordActionRowComponent>().ToList();

    /// <summary>
    /// Gets the message's content.
    /// </summary>
    [JsonProperty("content", NullValueHandling = NullValueHandling.Ignore)]
    public string Content { get; internal set; } = "";

    /// <summary>
    /// Gets the message's creation timestamp.
    /// </summary>
    [JsonProperty("timestamp", NullValueHandling = NullValueHandling.Ignore)]
    public DateTimeOffset Timestamp { get; set; }

    /// <summary>
    /// Gets the message's edit timestamp. Will be null if the message was not edited.
    /// </summary>
    [JsonProperty("edited_timestamp", NullValueHandling = NullValueHandling.Ignore)]
    public DateTimeOffset? EditedTimestamp { get; internal set; }

    /// <summary>
    /// Gets whether this message was edited.
    /// </summary>
    [JsonIgnore]
    public bool IsEdited => this.EditedTimestamp is not null;

    /// <summary>
    /// Gets users or members mentioned by this message.
    /// </summary>
    [JsonIgnore]
    public IReadOnlyList<DiscordUser> MentionedUsers
        => this.mentionedUsers;

    [JsonProperty("mentions", NullValueHandling = NullValueHandling.Ignore)]
    internal List<DiscordUser> mentionedUsers = [];

    // TODO this will probably throw an exception in DMs since it tries to wrap around a null List...
    // this is probably low priority but need to find out a clean way to solve it...
    /// <summary>
    /// Gets roles mentioned by this message.
    /// </summary>
    [JsonIgnore]
    public IReadOnlyList<DiscordRole> MentionedRoles
        => this.mentionedRoles;

    [JsonIgnore]
    internal List<DiscordRole> mentionedRoles = [];

    [JsonProperty("mention_roles")]
    internal List<ulong> mentionedRoleIds = [];

    /// <summary>
    /// Gets channels mentioned by this message.
    /// </summary>
    [JsonIgnore]
    public IReadOnlyList<DiscordChannel> MentionedChannels
        => this.mentionedChannels;

    [JsonIgnore]
    internal List<DiscordChannel> mentionedChannels = [];

    /// <summary>
    /// Gets files attached to this message.
    /// </summary>
    [JsonIgnore]
    public IReadOnlyList<DiscordAttachment> Attachments
        => this.attachments;

    [JsonProperty("attachments", NullValueHandling = NullValueHandling.Ignore)]
    internal List<DiscordAttachment> attachments = [];

    /// <summary>
    /// Gets embeds attached to this message.
    /// </summary>
    [JsonIgnore]
    public IReadOnlyList<DiscordEmbed> Embeds
        => this.embeds;

    [JsonProperty("embeds", NullValueHandling = NullValueHandling.Ignore)]
    internal List<DiscordEmbed> embeds = [];

    /// <summary>
    /// Gets the type of the message.
    /// </summary>
    [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
    public DiscordMessageType? MessageType { get; internal set; }

    /// <summary>
    /// Gets the bitwise flags for this message.
    /// </summary>
    [JsonProperty("flags", NullValueHandling = NullValueHandling.Ignore)]
    public DiscordMessageFlags? Flags { get; internal set; }

    /// <summary>
    /// Gets stickers for this message.
    /// </summary>
    [JsonIgnore]
    public IReadOnlyList<DiscordMessageSticker>? Stickers
        => this.stickers;

    [JsonProperty("sticker_items", NullValueHandling = NullValueHandling.Ignore)]
    internal List<DiscordMessageSticker> stickers = [];

    internal ulong? guildId { get; set; }

    private IMention[] GetMentions()
    {
        List<IMention> mentions = [];

        if ((this.mentionedUsers?.Count ?? 0) > 0)
        {
            mentions.AddRange(this.mentionedUsers!.Select(m => (IMention)new UserMention(m)));
        }

        if ((this.mentionedRoleIds?.Count ?? 0) > 0)
        {
            mentions.AddRange(this.mentionedRoleIds!.Select(r => (IMention)new RoleMention(r)));
        }

        return [.. mentions];
    }

    internal void PopulateMentions()
    {
        this.mentionedUsers ??= [];
        this.mentionedRoles ??= [];
        this.mentionedChannels ??= [];

        // Create a Hashset that will replace 'this.mentionedUsers'.
        HashSet<DiscordUser> mentionedUsers = new(new DiscordUserComparer());

        foreach (DiscordUser usr in this.mentionedUsers)
        {
            mentionedUsers.Add(usr);
        }

        // Replace 'this.mentionedUsers'.
        this.mentionedUsers = [.. mentionedUsers];

        if (!string.IsNullOrWhiteSpace(this.Content))
        {
            this.mentionedChannels = this.mentionedChannels.Union(Utilities.GetChannelMentions(this.Content).Select(x => new DiscordChannel() { Id = x })).ToList();
            this.mentionedRoles = this.mentionedRoles.Union(this.mentionedRoleIds.Select(x => new DiscordRole() { Id = x })).ToList();
        }
    }

    /// <summary>
    /// Searches the components on this message for an aggregate of all components of a certain type.
    /// </summary>
    public IReadOnlyList<T> FilterComponents<T>()
        where T : DiscordComponent
    {
        List<T> components = [];

        foreach (DiscordComponent component in this.Components)
        {
            if (component is DiscordActionRowComponent actionRowComponent)
            {
                foreach (DiscordComponent subComponent in actionRowComponent.Components)
                {
                    if (subComponent is T filteredComponent)
                    {
                        components.Add(filteredComponent);
                    }
                }
            }
            else if (component is T filteredComponent)
            {
                components.Add(filteredComponent);
            }
        }

        return components;
    }

    /// <summary>
    /// Returns a string representation of this message.
    /// </summary>
    /// <returns>String representation of this message.</returns>
    public override string ToString() => $"Message Snapshot; Attachment count: {this.attachments.Count}; Embed count: {this.embeds.Count}; Contents: {this.Content}";
}
