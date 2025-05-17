using System;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DSharpPlus.Entities;

/// <summary>
/// Represents a Discord permission overwrite builder.
/// </summary>
public sealed record DiscordOverwriteBuilder
{
    /// <summary>
    /// Gets or sets the allowed permissions for this overwrite.
    /// </summary>
    public DiscordPermissions Allowed { get; set; }

    /// <summary>
    /// Gets or sets the denied permissions for this overwrite.
    /// </summary>
    public DiscordPermissions Denied { get; set; }

    /// <summary>
    /// The id of the target for this overwrite.
    /// </summary>
    public ulong TargetId { get; set; }

    /// <summary>
    /// Gets the type of this overwrite's target.
    /// </summary>
    public DiscordOverwriteType Type { get; set; }

    /// <summary>
    /// Creates a new Discord permission overwrite builder. This class can be used to construct permission overwrites for guild channels, used when creating channels.
    /// </summary>
    public DiscordOverwriteBuilder() { }

    /// <summary>
    /// Creates a new Discord permission overwrite builder for a member. This class can be used to construct permission overwrites for guild channels, used when creating channels.
    /// </summary>
    public DiscordOverwriteBuilder(DiscordMember member)
    {
        this.TargetId = member.Id;
        this.Type = DiscordOverwriteType.Member;
    }

    /// <summary>
    /// Creates a new Discord permission overwrite builder for a role. This class can be used to construct permission overwrites for guild channels, used when creating channels.
    /// </summary>
    public DiscordOverwriteBuilder(DiscordRole role)
    {
        this.TargetId = role.Id;
        this.Type = DiscordOverwriteType.Role;
    }

    /// <summary>
    /// Allows a permission for this overwrite.
    /// </summary>
    /// <param name="permission">Permission or permission set to allow for this overwrite.</param>
    /// <returns>This builder.</returns>
    public DiscordOverwriteBuilder Allow(DiscordPermissions permission)
    {
        this.Allowed |= permission;
        return this;
    }

    /// <summary>
    /// Denies a permission for this overwrite.
    /// </summary>
    /// <param name="permission">Permission or permission set to deny for this overwrite.</param>
    /// <returns>This builder.</returns>
    public DiscordOverwriteBuilder Deny(DiscordPermissions permission)
    {
        this.Denied |= permission;
        return this;
    }

    /// <summary>
    /// Attempts to get the entity representing the target of this overwrite.
    /// </summary>
    /// <param name="guild">The server to which the target belongs.</param>
    /// <returns>Entity representing the target of this overwrite, or null if the target id is not set.</returns>
    public async ValueTask<SnowflakeObject?> GetTargetAsync(DiscordGuild guild) => this.Type switch
    {
        DiscordOverwriteType.Member => await guild.GetMemberAsync(this.TargetId),
        DiscordOverwriteType.Role => await guild.GetRoleAsync(this.TargetId),
        _ => null
    };

    /// <summary>
    /// Populates this builder with data from another overwrite object.
    /// </summary>
    /// <param name="other">Overwrite from which data will be used.</param>
    /// <returns>This builder.</returns>
    public static DiscordOverwriteBuilder From(DiscordOverwrite other) => new()
    {
        Allowed = other.Allowed,
        Denied = other.Denied,
        TargetId = other.Id,
        Type = other.Type
    };

    /// <summary>
    /// Builds this DiscordOverwrite.
    /// </summary>
    /// <returns>Use this object for creation of new overwrites.</returns>
    internal DiscordRestOverwrite Build()
    {
        return this.TargetId is 0 ? throw new InvalidOperationException("The target id must be set.") : new()
        {
            Allow = this.Allowed,
            Deny = this.Denied,
            Id = this.TargetId,
            Type = this.Type,
        };
    }
}

internal struct DiscordRestOverwrite
{
    [JsonProperty("allow", NullValueHandling = NullValueHandling.Ignore)]
    internal DiscordPermissions Allow { get; set; }

    [JsonProperty("deny", NullValueHandling = NullValueHandling.Ignore)]
    internal DiscordPermissions Deny { get; set; }

    [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
    internal ulong Id { get; set; }

    [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
    internal DiscordOverwriteType Type { get; set; }
}
