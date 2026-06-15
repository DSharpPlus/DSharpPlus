using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

using DSharpPlus.Entities;
using DSharpPlus.Net;

namespace DSharpPlus;

/// <summary>
/// Various Discord-related utilities.
/// </summary>
public static partial class Utilities
{
    /// <summary>
    /// Gets the version of the library
    /// </summary>
    private static string VersionHeader { get; set; }

    /// <summary>
    /// Gets the library version.
    /// </summary>
    public static string Version { get; }

    internal static UTF8Encoding UTF8 { get; } = new UTF8Encoding(false);

    static Utilities()
    {
        Assembly a = typeof(DiscordClient).GetTypeInfo().Assembly;

        Version = "";
        AssemblyInformationalVersionAttribute? iv = a.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
        if (iv != null)
        {
            Version = iv.InformationalVersion;
        }
        else
        {
            Version? v = a.GetName().Version;
            Version = v.ToString(3);
        }

        VersionHeader = $"DiscordBot (https://github.com/DSharpPlus/DSharpPlus, v{Version})";
    }

    internal static string GetApiBaseUri()
        => Endpoints.BASE_URI;

    internal static string GetUserAgent()
        => VersionHeader;

    internal static IEnumerable<ulong> GetUserMentions(DiscordMessage message)
    {
        Regex regex = UserMentionRegex();
        MatchCollection matches = regex.Matches(message.Content);
        foreach (Match match in matches.Cast<Match>())
        {
            yield return ulong.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
        }
    }

    internal static IEnumerable<ulong> GetRoleMentions(DiscordMessage message)
    {
        Regex regex = RoleMentionRegex();
        MatchCollection matches = regex.Matches(message.Content);
        foreach (Match match in matches.Cast<Match>())
        {
            yield return ulong.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
        }
    }

    internal static IEnumerable<ulong> GetChannelMentions(DiscordMessage message) => GetChannelMentions(message.Content);

    internal static IEnumerable<ulong> GetChannelMentions(string messageContent)
    {
        Regex regex = ChannelMentionRegex();
        MatchCollection matches = regex.Matches(messageContent);
        foreach (Match match in matches.Cast<Match>())
        {
            yield return ulong.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
        }
    }

    internal static bool IsValidSlashCommandName(string name)
    {
        Regex regex = SlashCommandNameRegex();
        return regex.IsMatch(name);
    }

    internal static bool HasMessageIntents(DiscordIntents intents)
        => (intents.HasIntent(DiscordIntents.GuildMessages) && intents.HasIntent(DiscordIntents.MessageContents)) || intents.HasIntent(DiscordIntents.DirectMessages);

    internal static bool HasReactionIntents(DiscordIntents intents)
        => intents.HasIntent(DiscordIntents.GuildMessageReactions) || intents.HasIntent(DiscordIntents.DirectMessageReactions);

    internal static bool HasTypingIntents(DiscordIntents intents)
        => intents.HasIntent(DiscordIntents.GuildMessageTyping) || intents.HasIntent(DiscordIntents.DirectMessageTyping);

    internal static bool IsTextableChannel(DiscordChannel channel)
        => channel.Type switch
        {
            DiscordChannelType.Text => true,
            DiscordChannelType.Voice => true,
            DiscordChannelType.Group => true,
            DiscordChannelType.Private => true,
            DiscordChannelType.PublicThread => true,
            DiscordChannelType.PrivateThread => true,
            DiscordChannelType.NewsThread => true,
            DiscordChannelType.News => true,
            DiscordChannelType.Stage => true,
            _ => false,
        };
    
    /// <summary>
    /// Converts a stream to a base64-encoded data URL string.
    /// </summary>
    /// <param name="stream">The optional stream to convert.</param>
    /// <returns>
    /// An optional base64-encoded data URL string. If the stream has no value, returns an optional with no value.
    /// If the stream has a value but it is null, returns an optional containing null.
    /// Otherwise, returns the base64-encoded data URL string.
    /// </returns>
    internal static Optional<string?> ConvertStreamToBase64(Optional<Stream?> stream)
    {
        if (stream.HasValue && stream.Value is not null)
        {
            using InlineMediaTool imgtool = new(stream.Value);
            return imgtool.GetBase64();
        }

        return stream.HasValue
            ? Optional.FromValue<string?>(null)
            : Optional.FromNoValue<string?>();
    }

    // https://discord.com/developers/docs/topics/gateway#sharding-sharding-formula
    /// <summary>
    /// Gets a shard id from a guild id and total shard count.
    /// </summary>
    /// <param name="guildId">The guild id the shard is on.</param>
    /// <param name="shardCount">The total amount of shards.</param>
    /// <returns>The shard id.</returns>
    public static int GetShardId(ulong guildId, int shardCount)
        => (int)((guildId >> 22) % (ulong)shardCount);

    /// <summary>
    /// Computes a timestamp from a given snowflake.
    /// </summary>
    /// <param name="snowflake">Snowflake to compute a timestamp from.</param>
    /// <returns>Computed timestamp.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateTimeOffset GetSnowflakeTime(this ulong snowflake)
        => DiscordClient.discordEpoch.AddMilliseconds(snowflake >> 22);

    /// <summary>
    /// Creates a snowflake from a given <see cref="DateTimeOffset"/>. This can be used to provide "timestamps" for methods
    /// like <see cref="DiscordChannel.GetMessagesAfterAsync"/>.
    /// </summary>
    /// <param name="dateTimeOffset">DateTimeOffset to create a snowflake from.</param>
    /// <returns>Returns a snowflake representing the given date and time.</returns>
    public static ulong CreateSnowflake(DateTimeOffset dateTimeOffset)
    {
        long diff = dateTimeOffset.ToUnixTimeMilliseconds() - DiscordClient.discordEpoch.ToUnixTimeMilliseconds();
        return (ulong)diff << 22;
    }

    [GeneratedRegex("<@(\\d+)>", RegexOptions.ECMAScript)]
    private static partial Regex UserMentionRegex();

    [GeneratedRegex("<#(\\d+)>", RegexOptions.ECMAScript)]
    private static partial Regex ChannelMentionRegex();

    [GeneratedRegex("<@&(\\d+)>", RegexOptions.ECMAScript)]
    private static partial Regex RoleMentionRegex();

    [GeneratedRegex("^[\\w-]{1,32}$")]
    private static partial Regex SlashCommandNameRegex();
}
