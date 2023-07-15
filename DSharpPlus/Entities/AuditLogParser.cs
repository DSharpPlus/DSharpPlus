using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Net.Abstractions;
using DSharpPlus.Net.Serialization;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace DSharpPlus.Entities;

internal static class AuditLogParser
{
    internal static async Task<DiscordAuditLogEntry> ParseGuildUpdateAsync(DiscordGuild guild,
        AuditLogAction auditLogAction)
    {
        DiscordAuditLogEntry entry = new DiscordAuditLogGuildEntry {Target = guild};

        ulong before, after;

        DiscordAuditLogGuildEntry guildEntry = entry as DiscordAuditLogGuildEntry;
        foreach (AuditLogActionChange? change in auditLogAction.Changes)
        {
            switch (change.Key.ToLowerInvariant())
            {
                case "name":
                    guildEntry.NameChange = new PropertyChange<string>
                    {
                        Before = change.OldValueString, After = change.NewValueString
                    };
                    break;

                case "owner_id":
                    guildEntry.OwnerChange = new PropertyChange<DiscordMember>
                    {
                        Before = guild._members != null && guild._members.TryGetValue(
                            change.OldValueUlong,
                            out DiscordMember? oldMember)
                            ? oldMember
                            : await guild.GetMemberAsync(change.OldValueUlong),
                        After = guild._members != null && guild._members.TryGetValue(change.NewValueUlong,
                            out DiscordMember? newMember)
                            ? newMember
                            : await guild.GetMemberAsync(change.NewValueUlong)
                    };
                    break;

                case "icon_hash":
                    guildEntry.IconChange = new PropertyChange<string>
                    {
                        Before = change.OldValueString != null
                            ? $"https://cdn.discordapp.com/icons/{guild.Id}/{change.OldValueString}.webp"
                            : null,
                        After = change.OldValueString != null
                            ? $"https://cdn.discordapp.com/icons/{guild.Id}/{change.NewValueString}.webp"
                            : null
                    };
                    break;

                case "verification_level":
                    guildEntry.VerificationLevelChange = new PropertyChange<VerificationLevel>
                    {
                        Before = (VerificationLevel)(long)change.OldValue,
                        After = (VerificationLevel)(long)change.NewValue
                    };
                    break;

                case "afk_channel_id":
                    ulong.TryParse(change.NewValue as string, NumberStyles.Integer,
                        CultureInfo.InvariantCulture, out before);
                    ulong.TryParse(change.OldValue as string, NumberStyles.Integer,
                        CultureInfo.InvariantCulture, out after);

                    guildEntry.AfkChannelChange = new PropertyChange<DiscordChannel>
                    {
                        Before = guild.GetChannel(before) ?? new DiscordChannel
                        {
                            Id = before, Discord = guild.Discord, GuildId = guild.Id
                        },
                        After = guild.GetChannel(after) ?? new DiscordChannel
                        {
                            Id = before, Discord = guild.Discord, GuildId = guild.Id
                        }
                    };
                    break;

                case "widget_channel_id":
                    ulong.TryParse(change.NewValue as string, NumberStyles.Integer,
                        CultureInfo.InvariantCulture, out before);
                    ulong.TryParse(change.OldValue as string, NumberStyles.Integer,
                        CultureInfo.InvariantCulture, out after);

                    guildEntry.EmbedChannelChange = new PropertyChange<DiscordChannel>
                    {
                        Before = guild.GetChannel(before) ?? new DiscordChannel
                        {
                            Id = before, Discord = guild.Discord, GuildId = guild.Id
                        },
                        After = guild.GetChannel(after) ?? new DiscordChannel
                        {
                            Id = before, Discord = guild.Discord, GuildId = guild.Id
                        }
                    };
                    break;

                case "splash_hash":
                    guildEntry.SplashChange = new PropertyChange<string>
                    {
                        Before = change.OldValueString != null
                            ? $"https://cdn.discordapp.com/splashes/{guild.Id}/{change.OldValueString}.webp?size=2048"
                            : null,
                        After = change.NewValueString != null
                            ? $"https://cdn.discordapp.com/splashes/{guild.Id}/{change.NewValueString}.webp?size=2048"
                            : null
                    };
                    break;

                case "default_message_notifications":
                    guildEntry.NotificationSettingsChange = new PropertyChange<DefaultMessageNotifications>
                    {
                        Before = (DefaultMessageNotifications)(long)change.OldValue,
                        After = (DefaultMessageNotifications)(long)change.NewValue
                    };
                    break;

                case "system_channel_id":
                    ulong.TryParse(change.NewValue as string, NumberStyles.Integer,
                        CultureInfo.InvariantCulture, out before);
                    ulong.TryParse(change.OldValue as string, NumberStyles.Integer,
                        CultureInfo.InvariantCulture, out after);

                    guildEntry.SystemChannelChange = new PropertyChange<DiscordChannel>
                    {
                        Before = guild.GetChannel(before) ?? new DiscordChannel
                        {
                            Id = before, Discord = guild.Discord, GuildId = guild.Id
                        },
                        After = guild.GetChannel(after) ?? new DiscordChannel
                        {
                            Id = before, Discord = guild.Discord, GuildId = guild.Id
                        }
                    };
                    break;

                case "explicit_content_filter":
                    guildEntry.ExplicitContentFilterChange = new PropertyChange<ExplicitContentFilter>
                    {
                        Before = (ExplicitContentFilter)(long)change.OldValue,
                        After = (ExplicitContentFilter)(long)change.NewValue
                    };
                    break;

                case "mfa_level":
                    guildEntry.MfaLevelChange = new PropertyChange<MfaLevel>
                    {
                        Before = (MfaLevel)(long)change.OldValue,
                        After = (MfaLevel)(long)change.NewValue
                    };
                    break;

                case "region":
                    guildEntry.RegionChange = new PropertyChange<string>
                    {
                        Before = change.OldValueString, After = change.NewValueString
                    };
                    break;

                default:
                    guild.Discord.Logger.LogWarning(LoggerEvents.AuditLog,
                        "Unknown key in guild update: {Key} - this should be reported to library developers",
                        change.Key);
                    break;
            }
        }

        return entry;
    }

    internal static DiscordAuditLogEntry ParseChannelEntry(DiscordGuild guild, AuditLogAction auditLogAction)
    {
        DiscordAuditLogEntry entry = new DiscordAuditLogChannelEntry
        {
            Target = guild.GetChannel(auditLogAction.TargetId.Value) ?? new DiscordChannel
            {
                Id = auditLogAction.TargetId.Value, Discord = guild.Discord, GuildId = guild.Id
            }
        };

        ulong ulongBefore, ulongAfter;
        bool boolBefore, boolAfter;
        DiscordAuditLogChannelEntry? channelEntry = entry as DiscordAuditLogChannelEntry;
        foreach (AuditLogActionChange? change in auditLogAction.Changes)
        {
            switch (change.Key.ToLowerInvariant())
            {
                case "name":
                    channelEntry.NameChange = new PropertyChange<string>
                    {
                        Before = change.OldValue != null ? change.OldValueString : null,
                        After = change.NewValue != null ? change.NewValueString : null
                    };
                    break;

                case "type":
                    boolBefore = ulong.TryParse(change.NewValue as string, NumberStyles.Integer,
                        CultureInfo.InvariantCulture, out ulongBefore);
                    boolAfter = ulong.TryParse(change.OldValue as string, NumberStyles.Integer,
                        CultureInfo.InvariantCulture, out ulongAfter);

                    channelEntry.TypeChange = new PropertyChange<ChannelType?>
                    {
                        Before = boolBefore ? (ChannelType?)ulongBefore : null,
                        After = boolAfter ? (ChannelType?)ulongAfter : null
                    };
                    break;

                case "permission_overwrites":
                    IEnumerable<DiscordOverwrite>? olds = change.OldValues?.OfType<JObject>()?
                        .Select(jObject => jObject.ToDiscordObject<DiscordOverwrite>())?
                        .Select(overwrite =>
                        {
                            overwrite.Discord = guild.Discord;
                            return overwrite;
                        });

                    IEnumerable<DiscordOverwrite>? news = change.NewValues?.OfType<JObject>()?
                        .Select(jObject => jObject.ToDiscordObject<DiscordOverwrite>())?
                        .Select(overwrite =>
                        {
                            overwrite.Discord = guild.Discord;
                            return overwrite;
                        });

                    channelEntry.OverwriteChange = new PropertyChange<IReadOnlyList<DiscordOverwrite>>
                    {
                        Before = olds != null
                            ? new ReadOnlyCollection<DiscordOverwrite>(new List<DiscordOverwrite>(olds))
                            : null,
                        After = news != null
                            ? new ReadOnlyCollection<DiscordOverwrite>(new List<DiscordOverwrite>(news))
                            : null
                    };
                    break;

                case "topic":
                    channelEntry.TopicChange = new PropertyChange<string>
                    {
                        Before = change.OldValueString, After = change.NewValueString
                    };
                    break;

                case "nsfw":
                    channelEntry.NsfwChange = new PropertyChange<bool?>
                    {
                        Before = (bool?)change.OldValue, After = (bool?)change.NewValue
                    };
                    break;

                case "bitrate":
                    channelEntry.BitrateChange = new PropertyChange<int?>
                    {
                        Before = (int?)(long?)change.OldValue, After = (int?)(long?)change.NewValue
                    };
                    break;

                case "rate_limit_per_user":
                    channelEntry.PerUserRateLimitChange = new PropertyChange<int?>
                    {
                        Before = (int?)(long?)change.OldValue, After = (int?)(long?)change.NewValue
                    };
                    break;

                default:
                    guild.Discord.Logger.LogWarning(LoggerEvents.AuditLog,
                        "Unknown key in channel update: {Key} - this should be reported to library developers",
                        change.Key);
                    break;
            }
        }

        return entry;
    }

    internal static DiscordAuditLogEntry ParseOverwriteEntry(DiscordGuild guild,
        AuditLogAction auditLogAction)
    {
        DiscordAuditLogEntry entry = new DiscordAuditLogOverwriteEntry
        {
            Target = guild
                .GetChannel(auditLogAction.TargetId.Value)?
                .PermissionOverwrites
                .FirstOrDefault(xo => xo.Id == auditLogAction.Options.Id),
            Channel = guild.GetChannel(auditLogAction.TargetId.Value)
        };

        ulong ulongBefore, ulongAfter;
        bool boolBefore, boolAfter;
        DiscordAuditLogOverwriteEntry? overwriteEntry = entry as DiscordAuditLogOverwriteEntry;
        foreach (AuditLogActionChange? xc in auditLogAction.Changes)
        {
            switch (xc.Key.ToLowerInvariant())
            {
                case "deny":
                    boolBefore = ulong.TryParse(xc.OldValue as string, NumberStyles.Integer,
                        CultureInfo.InvariantCulture, out ulongBefore);
                    boolAfter = ulong.TryParse(xc.OldValue as string, NumberStyles.Integer,
                        CultureInfo.InvariantCulture, out ulongAfter);

                    overwriteEntry.DenyChange = new()
                    {
                        Before = boolBefore ? (Permissions?)ulongBefore : null,
                        After = boolAfter ? (Permissions?)ulongAfter : null
                    };
                    break;

                case "allow":
                    boolBefore = ulong.TryParse(xc.OldValue as string, NumberStyles.Integer,
                        CultureInfo.InvariantCulture, out ulongBefore);
                    boolAfter = ulong.TryParse(xc.OldValue as string, NumberStyles.Integer,
                        CultureInfo.InvariantCulture, out ulongAfter);

                    overwriteEntry.AllowChange = new PropertyChange<Permissions?>
                    {
                        Before = boolBefore ? (Permissions?)ulongBefore : null,
                        After = boolAfter ? (Permissions?)ulongAfter : null
                    };
                    break;

                case "type":
                    overwriteEntry.TypeChange = new PropertyChange<string>
                    {
                        Before = xc.OldValueString, After = xc.NewValueString
                    };
                    break;

                case "id":
                    boolBefore = ulong.TryParse(xc.OldValue as string, NumberStyles.Integer,
                        CultureInfo.InvariantCulture, out ulongBefore);
                    boolAfter = ulong.TryParse(xc.NewValue as string, NumberStyles.Integer,
                        CultureInfo.InvariantCulture, out ulongAfter);

                    overwriteEntry.TargetIdChange = new PropertyChange<ulong?>
                    {
                        Before = boolBefore ? (ulong?)ulongBefore : null,
                        After = boolAfter ? (ulong?)ulongAfter : null
                    };
                    break;

                default:
                    guild.Discord.Logger.LogWarning(LoggerEvents.AuditLog,
                        "Unknown key in overwrite update: {Key} - this should be reported to library developers",
                        xc.Key);
                    break;
            }
        }

        return entry;
    }

    internal static DiscordAuditLogEntry ParseMemberUpdateEntry(DiscordGuild guild,
        AuditLogAction auditLogAction)
    {
        DiscordAuditLogEntry entry = new DiscordAuditLogMemberUpdateEntry
        {
            Target = guild._members.TryGetValue(auditLogAction.TargetId.Value, out DiscordMember? roleUpdMember)
                ? roleUpdMember
                : new DiscordMember {Id = auditLogAction.TargetId.Value, Discord = guild.Discord, _guild_id = guild.Id}
        };

        DiscordAuditLogMemberUpdateEntry? memberUpdateEntry = entry as DiscordAuditLogMemberUpdateEntry;
        foreach (AuditLogActionChange? xc in auditLogAction.Changes)
        {
            switch (xc.Key.ToLowerInvariant())
            {
                case "nick":
                    memberUpdateEntry.NicknameChange = new PropertyChange<string>
                    {
                        Before = xc.OldValueString, After = xc.NewValueString
                    };
                    break;

                case "deaf":
                    memberUpdateEntry.DeafenChange = new PropertyChange<bool?>
                    {
                        Before = (bool?)xc.OldValue, After = (bool?)xc.NewValue
                    };
                    break;

                case "mute":
                    memberUpdateEntry.MuteChange = new PropertyChange<bool?>
                    {
                        Before = (bool?)xc.OldValue, After = (bool?)xc.NewValue
                    };
                    break;

                case "communication_disabled_until":
                    memberUpdateEntry.TimeoutChange = new PropertyChange<DateTime?>
                    {
                        Before = xc.OldValue != null ? (DateTime)xc.OldValue : null,
                        After = xc.NewValue != null ? (DateTime)xc.NewValue : null
                    };
                    break;

                case "$add":
                    memberUpdateEntry.AddedRoles =
                        new ReadOnlyCollection<DiscordRole>(xc.NewValues.Select(xo => (ulong)xo["id"])
                            .Select(guild.GetRole).ToList());
                    break;

                case "$remove":
                    memberUpdateEntry.RemovedRoles =
                        new ReadOnlyCollection<DiscordRole>(xc.NewValues.Select(xo => (ulong)xo["id"])
                            .Select(guild.GetRole).ToList());
                    break;

                default:
                    guild.Discord.Logger.LogWarning(LoggerEvents.AuditLog,
                        "Unknown key in member update: {Key} - this should be reported to library developers", xc.Key);
                    break;
            }
        }

        return entry;
    }

    internal static DiscordAuditLogEntry ParseRoleUpdateEntry(DiscordGuild guild,
        AuditLogAction auditLogAction)
    {
        DiscordAuditLogEntry entry = new DiscordAuditLogRoleUpdateEntry
        {
            Target = guild.GetRole(auditLogAction.TargetId.Value) ??
                     new DiscordRole {Id = auditLogAction.TargetId.Value, Discord = guild.Discord}
        };
        bool boolBefore, boolAfter;
        int intBefore, intAfter;
        DiscordAuditLogRoleUpdateEntry? roleUpdateEntry = entry as DiscordAuditLogRoleUpdateEntry;
        foreach (AuditLogActionChange? change in auditLogAction.Changes)
        {
            switch (change.Key.ToLowerInvariant())
            {
                case "name":
                    roleUpdateEntry.NameChange = new PropertyChange<string>
                    {
                        Before = change.OldValueString, After = change.NewValueString
                    };
                    break;

                case "color":
                    boolBefore = int.TryParse(change.OldValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture,
                        out intBefore);
                    boolAfter = int.TryParse(change.NewValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture,
                        out intAfter);

                    roleUpdateEntry.ColorChange = new PropertyChange<int?>
                    {
                        Before = boolBefore ? (int?)intBefore : null, After = boolAfter ? (int?)intAfter : null
                    };
                    break;

                case "permissions":
                    roleUpdateEntry.PermissionChange = new PropertyChange<Permissions?>
                    {
                        Before = change.OldValue != null ? (Permissions?)long.Parse((string)change.OldValue) : null,
                        After = change.NewValue != null ? (Permissions?)long.Parse((string)change.NewValue) : null
                    };
                    break;

                case "position":
                    roleUpdateEntry.PositionChange = new PropertyChange<int?>
                    {
                        Before = change.OldValue != null ? (int?)(long)change.OldValue : null,
                        After = change.NewValue != null ? (int?)(long)change.NewValue : null,
                    };
                    break;

                case "mentionable":
                    roleUpdateEntry.MentionableChange = new PropertyChange<bool?>
                    {
                        Before = change.OldValue != null ? (bool?)change.OldValue : null,
                        After = change.NewValue != null ? (bool?)change.NewValue : null
                    };
                    break;

                case "hoist":
                    roleUpdateEntry.HoistChange = new PropertyChange<bool?>
                    {
                        Before = (bool?)change.OldValue, After = (bool?)change.NewValue
                    };
                    break;

                default:
                    guild.Discord.Logger.LogWarning(LoggerEvents.AuditLog,
                        "Unknown key in role update: {Key} - this should be reported to library developers", change.Key);
                    break;
            }
        }

        return entry;
    }

    internal static DiscordAuditLogEntry ParseInviteUpdateEntry(DiscordGuild guild, AuditLogAction auditLogAction)
    {
        DiscordAuditLogEntry entry = new DiscordAuditLogInviteEntry();

        DiscordInvite invite = new()
        {
            Discord = guild.Discord,
            Guild = new DiscordInviteGuild
            {
                Discord = guild.Discord, Id = guild.Id, Name = guild.Name, SplashHash = guild.SplashHash
            }
        };

        bool boolBefore, boolAfter;
        ulong ulongBefore, ulongAfter;
        int intBefore, intAfter;
        DiscordAuditLogInviteEntry? inviteEntry = entry as DiscordAuditLogInviteEntry;
        foreach (AuditLogActionChange? change in auditLogAction.Changes)
        {
            switch (change.Key.ToLowerInvariant())
            {
                case "max_age":
                    boolBefore = int.TryParse(change.OldValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture,
                        out intBefore);
                    boolAfter = int.TryParse(change.OldValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture,
                        out intAfter);

                    inviteEntry.MaxAgeChange = new PropertyChange<int?>
                    {
                        Before = boolBefore ? (int?)intBefore : null, After = boolAfter ? (int?)intAfter : null
                    };
                    break;

                case "code":
                    invite.Code = change.OldValueString ?? change.NewValueString;

                    inviteEntry.CodeChange = new PropertyChange<string>
                    {
                        Before = change.OldValueString, After = change.NewValueString
                    };
                    break;

                case "temporary":
                    inviteEntry.TemporaryChange = new PropertyChange<bool?>
                    {
                        Before = change.OldValue != null ? (bool?)change.OldValue : null,
                        After = change.NewValue != null ? (bool?)change.NewValue : null
                    };
                    break;

                case "inviter_id":
                    boolBefore = ulong.TryParse(change.OldValue as string, NumberStyles.Integer,
                        CultureInfo.InvariantCulture,
                        out ulongBefore);
                    boolAfter = ulong.TryParse(change.NewValue as string, NumberStyles.Integer,
                        CultureInfo.InvariantCulture,
                        out ulongAfter);

                    inviteEntry.InviterChange = new PropertyChange<DiscordMember>
                    {
                        Before = guild._members.TryGetValue(ulongBefore, out DiscordMember? propBeforeMember)
                            ? propBeforeMember
                            : new DiscordMember {Id = ulongBefore, Discord = guild.Discord, _guild_id = guild.Id},
                        After = guild._members.TryGetValue(ulongAfter, out DiscordMember? propAfterMember)
                            ? propAfterMember
                            : new DiscordMember {Id = ulongBefore, Discord = guild.Discord, _guild_id = guild.Id}
                    };
                    break;

                case "channel_id":
                    boolBefore = ulong.TryParse(change.OldValue as string, NumberStyles.Integer,
                        CultureInfo.InvariantCulture,
                        out ulongBefore);
                    boolAfter = ulong.TryParse(change.NewValue as string, NumberStyles.Integer,
                        CultureInfo.InvariantCulture,
                        out ulongAfter);

                    inviteEntry.ChannelChange = new PropertyChange<DiscordChannel>
                    {
                        Before = boolBefore
                            ? guild.GetChannel(ulongBefore) ??
                              new DiscordChannel {Id = ulongBefore, Discord = guild.Discord, GuildId = guild.Id}
                            : null,
                        After = boolAfter
                            ? guild.GetChannel(ulongAfter) ??
                              new DiscordChannel {Id = ulongBefore, Discord = guild.Discord, GuildId = guild.Id}
                            : null
                    };

                    DiscordChannel? channel = inviteEntry.ChannelChange.Before ?? inviteEntry.ChannelChange.After;
                    ChannelType? channelType = channel?.Type;
                    invite.Channel = new DiscordInviteChannel
                    {
                        Discord = guild.Discord,
                        Id = boolBefore ? ulongBefore : ulongAfter,
                        Name = channel?.Name,
                        Type = channelType != null ? channelType.Value : ChannelType.Unknown
                    };
                    break;

                case "uses":
                    boolBefore = int.TryParse(change.OldValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture,
                        out intBefore);
                    boolAfter = int.TryParse(change.OldValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture,
                        out intAfter);

                    inviteEntry.UsesChange = new PropertyChange<int?>
                    {
                        Before = boolBefore ? (int?)intBefore : null, After = boolAfter ? (int?)intAfter : null
                    };
                    break;

                case "max_uses":
                    boolBefore = int.TryParse(change.OldValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture,
                        out intBefore);
                    boolAfter = int.TryParse(change.OldValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture,
                        out intAfter);

                    inviteEntry.MaxUsesChange = new PropertyChange<int?>
                    {
                        Before = boolBefore ? (int?)intBefore : null, After = boolAfter ? (int?)intAfter : null
                    };
                    break;

                default:
                    guild.Discord.Logger.LogWarning(LoggerEvents.AuditLog,
                        "Unknown key in invite update: {Key} - this should be reported to library developers", change.Key);
                    break;
            }
        }

        inviteEntry.Target = invite;
        return entry;
    }

    internal static DiscordAuditLogEntry ParseWebhookUpdateEntry
    (
        DiscordGuild guild,
        AuditLogAction auditLogAction,
        Dictionary<ulong, DiscordWebhook> webhooks
    )
    {
        DiscordAuditLogEntry entry = new DiscordAuditLogWebhookEntry
        {
            Target = webhooks.TryGetValue(auditLogAction.TargetId.Value, out DiscordWebhook? webhook)
                ? webhook
                : new DiscordWebhook {Id = auditLogAction.TargetId.Value, Discord = guild.Discord}
        };

        ulong ulongBefore, ulongAfter;
        bool boolBefore, boolAfter;
        int intBefore, intAfter;
        DiscordAuditLogWebhookEntry? webhookEntry = entry as DiscordAuditLogWebhookEntry;
        foreach (AuditLogActionChange actionChange in auditLogAction.Changes)
        {
            switch (actionChange.Key.ToLowerInvariant())
            {
                case "name":
                    webhookEntry.NameChange = new PropertyChange<string>
                    {
                        Before = actionChange.OldValueString, After = actionChange.NewValueString
                    };
                    break;

                case "channel_id":
                    boolBefore = ulong.TryParse(actionChange.OldValue as string, NumberStyles.Integer,
                        CultureInfo.InvariantCulture, out ulongBefore);
                    boolAfter = ulong.TryParse(actionChange.NewValue as string, NumberStyles.Integer,
                        CultureInfo.InvariantCulture, out ulongAfter);

                    webhookEntry.ChannelChange = new PropertyChange<DiscordChannel>
                    {
                        Before =
                            boolBefore
                                ? guild.GetChannel(ulongBefore) ?? new DiscordChannel
                                {
                                    Id = ulongBefore, Discord = guild.Discord, GuildId = guild.Id
                                }
                                : null,
                        After = boolAfter
                            ? guild.GetChannel(ulongAfter) ??
                              new DiscordChannel {Id = ulongBefore, Discord = guild.Discord, GuildId = guild.Id}
                            : null
                    };
                    break;

                case "type": // ???
                    boolBefore = int.TryParse(actionChange.OldValue as string, NumberStyles.Integer,
                        CultureInfo.InvariantCulture, out intBefore);
                    boolAfter = int.TryParse(actionChange.NewValue as string, NumberStyles.Integer,
                        CultureInfo.InvariantCulture, out intAfter);

                    webhookEntry.TypeChange = new PropertyChange<int?>
                    {
                        Before = boolBefore ? (int?)intBefore : null, After = boolAfter ? (int?)intAfter : null
                    };
                    break;

                case "avatar_hash":
                    webhookEntry.AvatarHashChange = new PropertyChange<string>
                    {
                        Before = actionChange.OldValueString, After = actionChange.NewValueString
                    };
                    break;

                case "application_id"
                    : //Why the fuck does discord send this as a string if it's supposed to be a snowflake
                    webhookEntry.ApplicationIdChange = new PropertyChange<ulong?>
                    {
                        Before =
                            actionChange.OldValue != null ? Convert.ToUInt64(actionChange.OldValueString) : null,
                        After = actionChange.NewValue != null ? Convert.ToUInt64(actionChange.NewValueString) : null
                    };
                    break;


                default:
                    guild.Discord.Logger.LogWarning(LoggerEvents.AuditLog,
                        "Unknown key in webhook update: {Key} - this should be reported to library developers",
                        actionChange.Key);
                    break;
            }
        }

        return entry;
    }

    internal static DiscordAuditLogEntry ParseStickerUpdateEntry(DiscordGuild guild, AuditLogAction auditLogAction)
    {
        DiscordAuditLogEntry entry = new DiscordAuditLogStickerEntry
        {
            Target = guild._stickers.TryGetValue(auditLogAction.TargetId.Value, out DiscordMessageSticker? sticker)
                ? sticker
                : new DiscordMessageSticker {Id = auditLogAction.TargetId.Value, Discord = guild.Discord}
        };

        ulong ulongBefore, ulongAfter;
        long longBefore, longAfter;
        bool boolBefore, boolAfter;
        DiscordAuditLogStickerEntry? stickerEntry = entry as DiscordAuditLogStickerEntry;
        foreach (AuditLogActionChange actionChange in auditLogAction.Changes)
        {
            switch (actionChange.Key.ToLowerInvariant())
            {
                case "name":
                    stickerEntry.NameChange = new PropertyChange<string>
                    {
                        Before = actionChange.OldValueString, After = actionChange.NewValueString
                    };
                    break;
                case "description":
                    stickerEntry.DescriptionChange = new PropertyChange<string>
                    {
                        Before = actionChange.OldValueString, After = actionChange.NewValueString
                    };
                    break;
                case "tags":
                    stickerEntry.TagsChange = new PropertyChange<string>
                    {
                        Before = actionChange.OldValueString, After = actionChange.NewValueString
                    };
                    break;
                case "guild_id":
                    stickerEntry.GuildIdChange = new PropertyChange<ulong?>
                    {
                        Before = ulong.TryParse(actionChange.OldValueString, out ulong oldGuildId) ? oldGuildId : null,
                        After = ulong.TryParse(actionChange.NewValueString, out ulong newGuildId) ? newGuildId : null
                    };
                    break;
                case "available":
                    stickerEntry.AvailabilityChange = new PropertyChange<bool?>
                    {
                        Before = (bool?)actionChange.OldValue, After = (bool?)actionChange.NewValue,
                    };
                    break;
                case "asset":
                    stickerEntry.AssetChange = new PropertyChange<string>
                    {
                        Before = actionChange.OldValueString, After = actionChange.NewValueString
                    };
                    break;
                case "id":
                    stickerEntry.IdChange = new PropertyChange<ulong?>
                    {
                        Before = ulong.TryParse(actionChange.OldValueString, out ulong oid) ? oid : null,
                        After = ulong.TryParse(actionChange.NewValueString, out ulong nid) ? nid : null
                    };
                    break;
                case "type":
                    boolBefore = long.TryParse(actionChange.OldValue as string, NumberStyles.Integer,
                        CultureInfo.InvariantCulture, out longBefore);
                    boolAfter = long.TryParse(actionChange.NewValue as string, NumberStyles.Integer,
                        CultureInfo.InvariantCulture, out longAfter);
                    stickerEntry.TypeChange = new PropertyChange<StickerType?>
                    {
                        Before = boolBefore ? (StickerType?)longBefore : null,
                        After = boolAfter ? (StickerType?)longAfter : null
                    };
                    break;
                case "format_type":
                    boolBefore = long.TryParse(actionChange.OldValue as string, NumberStyles.Integer,
                        CultureInfo.InvariantCulture, out longBefore);
                    boolAfter = long.TryParse(actionChange.NewValue as string, NumberStyles.Integer,
                        CultureInfo.InvariantCulture, out longAfter);
                    stickerEntry.FormatChange = new PropertyChange<StickerFormat?>
                    {
                        Before = boolBefore ? (StickerFormat?)longBefore : null,
                        After = boolAfter ? (StickerFormat?)longAfter : null
                    };
                    break;

                default:
                    guild.Discord.Logger.LogWarning(LoggerEvents.AuditLog,
                        "Unknown key in sticker update: {Key} - this should be reported to library developers",
                        actionChange.Key);
                    break;
            }
        }

        return entry;
    }
}
