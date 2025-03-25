using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
using DSharpPlus.Net.Serialization;
using Newtonsoft.Json.Linq;

namespace DSharpPlus.Tools.Benchmarks.Cases;

public class TextCommandExecutionBenchmarks
{
    private static readonly DiscordMessage message = DiscordJson.ToDiscordObject<DiscordMessage>(JToken.Parse("""{"type":0,"tts":false,"timestamp":"2024-10-02T01:04:48.604-05:00","pinned":false,"nonce":"1290917384118337536","mentions":[],"mention_roles":[],"mention_everyone":false,"member":{"roles":["402444174223343617","1252238597004853379","573612339924959270","379397810476417064","973818122211590154","1204470581274353674","750044518849445909","1059190345688158358","1057954598683414619","1163786594138992670"],"premium_since":null,"pending":false,"nick":null,"mute":false,"joined_at":"2024-09-26T23:50:07.584-05:00","flags":11,"deaf":false,"communication_disabled_until":null,"banner":null,"avatar":null},"id":"1290917384726515742","flags":0,"embeds":[],"edited_timestamp":null,"content":"!enum Monday","components":[],"channel_id":"379379415475552276","author":{"username":"oolunar","public_flags":4194560,"id":"336733686529654798","global_name":"Lunar","discriminator":"0","clan":{"tag":"Moon","identity_guild_id":"832354798153236510","identity_enabled":true,"badge":"f09dccb8074d3c1a3bccadff9ceee10b"},"avatar_decoration_data":null,"avatar":"cb52688afd66f14e8a433396cd84c7c7"},"attachments":[],"guild_id":"379378609942560770"}"""));
    private static MessageCreatedEventArgs notFoundCommand;
    private static MessageCreatedEventArgs command0Args;
    private static MessageCreatedEventArgs command1Args;
    private static MessageCreatedEventArgs command2Args;
    private static MessageCreatedEventArgs command3Args;
    private static MessageCreatedEventArgs command4Args;
    private static MessageCreatedEventArgs command5Args;
    private static MessageCreatedEventArgs command6Args;

    [GlobalSetup]
    public void Setup()
    {
        bool isConnected = DiscordData.IsConnected;
        DiscordData.SetupStaticVariablesAsync().GetAwaiter().GetResult();
        if (!isConnected)
        {
            notFoundCommand = TextCommandUtilities.CreateFakeMessageEventArgsAsync(message, "!not found", DiscordData.Client, DiscordData.Client.CurrentUser, DiscordData.Channel, DiscordData.Guild).GetAwaiter().GetResult();
            command0Args = TextCommandUtilities.CreateFakeMessageEventArgsAsync(message, "!none", DiscordData.Client, DiscordData.Client.CurrentUser, DiscordData.Channel, DiscordData.Guild).GetAwaiter().GetResult();
            command1Args = TextCommandUtilities.CreateFakeMessageEventArgsAsync(message, "!one 1", DiscordData.Client, DiscordData.Client.CurrentUser, DiscordData.Channel, DiscordData.Guild).GetAwaiter().GetResult();
            command2Args = TextCommandUtilities.CreateFakeMessageEventArgsAsync(message, "!two 1 2", DiscordData.Client, DiscordData.Client.CurrentUser, DiscordData.Channel, DiscordData.Guild).GetAwaiter().GetResult();
            command3Args = TextCommandUtilities.CreateFakeMessageEventArgsAsync(message, "!three 1 2 3", DiscordData.Client, DiscordData.Client.CurrentUser, DiscordData.Channel, DiscordData.Guild).GetAwaiter().GetResult();
            command4Args = TextCommandUtilities.CreateFakeMessageEventArgsAsync(message, "!four 1 2 3 4", DiscordData.Client, DiscordData.Client.CurrentUser, DiscordData.Channel, DiscordData.Guild).GetAwaiter().GetResult();
            command5Args = TextCommandUtilities.CreateFakeMessageEventArgsAsync(message, "!five 1 2 3 4 5", DiscordData.Client, DiscordData.Client.CurrentUser, DiscordData.Channel, DiscordData.Guild).GetAwaiter().GetResult();
            command6Args = TextCommandUtilities.CreateFakeMessageEventArgsAsync(message, "!six 1 2 3 4 5 6", DiscordData.Client, DiscordData.Client.CurrentUser, DiscordData.Channel, DiscordData.Guild).GetAwaiter().GetResult();
        }
    }

    [Benchmark, ArgumentsSource(nameof(GetDiscordClient))]
    public async ValueTask CommandNotFoundAsync(DiscordClient client) => await DiscordData.TextCommandProcessor.ExecuteTextCommandAsync(client, notFoundCommand);

    [Benchmark, ArgumentsSource(nameof(GetDiscordClient))]
    public async ValueTask Command0ArgsAsync(DiscordClient client) => await DiscordData.TextCommandProcessor.ExecuteTextCommandAsync(client, command0Args);

    [Benchmark, ArgumentsSource(nameof(GetDiscordClient))]
    public async ValueTask Command1ArgsAsync(DiscordClient client) => await DiscordData.TextCommandProcessor.ExecuteTextCommandAsync(client, command1Args);

    [Benchmark, ArgumentsSource(nameof(GetDiscordClient))]
    public async ValueTask Command2ArgsAsync(DiscordClient client) => await DiscordData.TextCommandProcessor.ExecuteTextCommandAsync(client, command2Args);

    [Benchmark, ArgumentsSource(nameof(GetDiscordClient))]
    public async ValueTask Command3ArgsAsync(DiscordClient client) => await DiscordData.TextCommandProcessor.ExecuteTextCommandAsync(client, command3Args);

    [Benchmark, ArgumentsSource(nameof(GetDiscordClient))]
    public async ValueTask Command4ArgsAsync(DiscordClient client) => await DiscordData.TextCommandProcessor.ExecuteTextCommandAsync(client, command4Args);

    [Benchmark, ArgumentsSource(nameof(GetDiscordClient))]
    public async ValueTask Command5ArgsAsync(DiscordClient client) => await DiscordData.TextCommandProcessor.ExecuteTextCommandAsync(client, command5Args);

    [Benchmark, ArgumentsSource(nameof(GetDiscordClient))]
    public async ValueTask Command6ArgsAsync(DiscordClient client) => await DiscordData.TextCommandProcessor.ExecuteTextCommandAsync(client, command6Args);

    public IEnumerable<object> GetDiscordClient()
    {
        Setup();

        yield return DiscordData.Client;
    }
}

internal static partial class TextCommandUtilities
{
    [GeneratedRegex(@"<@&(\d+)>", RegexOptions.Compiled)] private static partial Regex _roleMentionRegex();
    [GeneratedRegex(@"<@!?(\d+)>", RegexOptions.Compiled)] private static partial Regex _userMentionRegex();
    [GeneratedRegex(@"<#(\d+)>", RegexOptions.Compiled)] private static partial Regex _channelMentionRegex();

    [UnsafeAccessor(UnsafeAccessorKind.Constructor)] private static extern MessageCreatedEventArgs _messageCreateEventArgsConstructor();
    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_Message")] private static extern void _messageCreateEventArgsMessageSetter(MessageCreatedEventArgs messageCreateEventArgs, DiscordMessage message);
    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_MentionedUsers")] private static extern void _messageCreateEventArgsMentionedUsersSetter(MessageCreatedEventArgs messageCreateEventArgs, IReadOnlyList<DiscordUser> mentionedUsers);
    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_MentionedRoles")] private static extern void _messageCreateEventArgsMentionedRolesSetter(MessageCreatedEventArgs messageCreateEventArgs, IReadOnlyList<DiscordRole> mentionedRoles);
    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_MentionedChannels")] private static extern void _messageCreateEventArgsMentionedChannelsSetter(MessageCreatedEventArgs messageCreateEventArgs, IReadOnlyList<DiscordChannel> mentionedChannels);
    [UnsafeAccessor(UnsafeAccessorKind.Constructor)] private static extern DiscordMessage _messageConstructor();
    [UnsafeAccessor(UnsafeAccessorKind.Constructor)] private static extern DiscordMessage _messageConstructor(DiscordMessage message);
    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_Content")] private static extern void _messageContentSetter(DiscordMessage message, string content);
    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_Channel")] private static extern void _messageChannelSetter(DiscordMessage message, DiscordChannel channel);
    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_ChannelId")] private static extern void _messageChannelIdSetter(DiscordMessage message, ulong channelId);
    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_guildId")] private static extern void _messageGuildIdSetter(DiscordMessage message, ulong? guildId);
    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_Author")] private static extern void _messageAuthorSetter(DiscordMessage message, DiscordUser author);
    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "mentionedUsers")] private static extern ref List<DiscordUser> _messageMentionedUsersSetter(DiscordMessage message);
    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "mentionedRoles")] private static extern ref List<DiscordRole> _messageMentionedRolesSetter(DiscordMessage message);
    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "mentionedChannels")] private static extern ref List<DiscordChannel> _messageMentionedChannelsSetter(DiscordMessage message);

    public static DiscordMessage Copy(this DiscordMessage message) => _messageConstructor(message);

    public static async Task<MessageCreatedEventArgs> CreateFakeMessageEventArgsAsync(DiscordMessage message, string content, DiscordClient client, DiscordUser user, DiscordChannel channel, DiscordGuild guild)
    {
        ArgumentNullException.ThrowIfNull(message, nameof(message));
        ArgumentNullException.ThrowIfNull(content, nameof(content));
        ArgumentNullException.ThrowIfNull(client, nameof(client));
        ArgumentNullException.ThrowIfNull(user, nameof(user));
        ArgumentNullException.ThrowIfNull(channel, nameof(channel));
        ArgumentNullException.ThrowIfNull(guild, nameof(guild));

        // Create a copy of the message to modify to prevent modifying the original message.
        // By modifying the original message, we could potentially cause issues with the message cache.
        DiscordMessage messageCopy = message.Copy();

        // Modify the copied message to contain the contents that the user wants.
        await messageCopy.ModifyMessagePropertiesAsync(content, client, user, channel, guild);

        // Create the message created event args.
        MessageCreatedEventArgs messageCreateEventArgs = _messageCreateEventArgsConstructor();
        _messageCreateEventArgsMessageSetter(messageCreateEventArgs, messageCopy);
        _messageCreateEventArgsMentionedUsersSetter(messageCreateEventArgs, messageCopy.MentionedUsers);
        _messageCreateEventArgsMentionedRolesSetter(messageCreateEventArgs, messageCopy.MentionedRoles);
        _messageCreateEventArgsMentionedChannelsSetter(messageCreateEventArgs, messageCopy.MentionedChannels);

        // Return the message created event args, which can be used for argument conversion or command execution.
        return messageCreateEventArgs;
    }

    /// <summary>
    /// Modifies the internal properties of a message, changing the content, author, channel, and mentions.
    /// </summary>
    /// <param name="message">The message to modify.</param>
    /// <param name="content">The new content of the message.</param>
    /// <param name="client">The client to use for fetching users and channels.</param>
    /// <param name="user">The new author of the message.</param>
    /// <param name="channel">The new channel of the message.</param>
    /// <param name="guild">The guild to use for fetching roles.</param>
    public static async Task ModifyMessagePropertiesAsync(this DiscordMessage message, string content, DiscordClient client, DiscordUser user, DiscordChannel channel, DiscordGuild? guild = null)
    {
        List<DiscordRole> roleMentions = [];
        List<DiscordUser> userMentions = [];
        List<DiscordChannel> channelMentions = [];
        if (guild is not null)
        {
            MatchCollection roleMatches = _roleMentionRegex().Matches(content);
            foreach (Match match in roleMatches.Cast<Match>())
            {
                if (ulong.TryParse(match.Groups[1].Value, out ulong roleId) && await guild.GetRoleAsync(roleId) is DiscordRole role)
                {
                    roleMentions.Add(role);
                }
            }

            MatchCollection userMatches = _userMentionRegex().Matches(content);
            foreach (Match match in userMatches.Cast<Match>())
            {
                if (ulong.TryParse(match.Groups[1].Value, out ulong userId))
                {
                    userMentions.Add(await client.GetUserAsync(userId));
                }
            }
        }

        MatchCollection channelMatches = _channelMentionRegex().Matches(content);
        foreach (Match match in channelMatches.Cast<Match>())
        {
            if (ulong.TryParse(match.Groups[1].Value, out ulong channelId))
            {
                DiscordChannel? mentionedChannel;
                try
                {
                    mentionedChannel = await client.GetChannelAsync(channelId);
                }
                catch (DiscordException)
                {
                    continue;
                }

                channelMentions.Add(mentionedChannel);
            }
        }

        _messageContentSetter(message, content);
        _messageChannelSetter(message, channel);
        _messageChannelIdSetter(message, channel.Id);
        _messageGuildIdSetter(message, channel.GuildId);
        _messageAuthorSetter(message, user);
        _messageMentionedUsersSetter(message) = userMentions;
        _messageMentionedRolesSetter(message) = roleMentions;
        _messageMentionedChannelsSetter(message) = channelMentions;
    }
}
