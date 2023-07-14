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
        foreach (AuditLogActionChange? auditLogActionChange in auditLogAction.Changes)
        {
            switch (auditLogActionChange.Key.ToLowerInvariant())
            {
                case "name":
                    guildEntry.NameChange = new PropertyChange<string>
                    {
                        Before = auditLogActionChange.OldValueString, After = auditLogActionChange.NewValueString
                    };
                    break;

                case "owner_id":
                    guildEntry.OwnerChange = new PropertyChange<DiscordMember>
                    {
                        Before = guild._members != null && guild._members.TryGetValue(
                            auditLogActionChange.OldValueUlong,
                            out DiscordMember? oldMember)
                            ? oldMember
                            : await guild.GetMemberAsync(auditLogActionChange.OldValueUlong),
                        After = guild._members != null && guild._members.TryGetValue(auditLogActionChange.NewValueUlong,
                            out DiscordMember? newMember)
                            ? newMember
                            : await guild.GetMemberAsync(auditLogActionChange.NewValueUlong)
                    };
                    break;

                case "icon_hash":
                    guildEntry.IconChange = new PropertyChange<string>
                    {
                        Before = auditLogActionChange.OldValueString != null
                            ? $"https://cdn.discordapp.com/icons/{guild.Id}/{auditLogActionChange.OldValueString}.webp"
                            : null,
                        After = auditLogActionChange.OldValueString != null
                            ? $"https://cdn.discordapp.com/icons/{guild.Id}/{auditLogActionChange.NewValueString}.webp"
                            : null
                    };
                    break;

                case "verification_level":
                    guildEntry.VerificationLevelChange = new PropertyChange<VerificationLevel>
                    {
                        Before = (VerificationLevel)(long)auditLogActionChange.OldValue,
                        After = (VerificationLevel)(long)auditLogActionChange.NewValue
                    };
                    break;

                case "afk_channel_id":
                    ulong.TryParse(auditLogActionChange.NewValue as string, NumberStyles.Integer,
                        CultureInfo.InvariantCulture, out before);
                    ulong.TryParse(auditLogActionChange.OldValue as string, NumberStyles.Integer,
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
                    ulong.TryParse(auditLogActionChange.NewValue as string, NumberStyles.Integer,
                        CultureInfo.InvariantCulture, out before);
                    ulong.TryParse(auditLogActionChange.OldValue as string, NumberStyles.Integer,
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
                        Before = auditLogActionChange.OldValueString != null
                            ? $"https://cdn.discordapp.com/splashes/{guild.Id}/{auditLogActionChange.OldValueString}.webp?size=2048"
                            : null,
                        After = auditLogActionChange.NewValueString != null
                            ? $"https://cdn.discordapp.com/splashes/{guild.Id}/{auditLogActionChange.NewValueString}.webp?size=2048"
                            : null
                    };
                    break;

                case "default_message_notifications":
                    guildEntry.NotificationSettingsChange = new PropertyChange<DefaultMessageNotifications>
                    {
                        Before = (DefaultMessageNotifications)(long)auditLogActionChange.OldValue,
                        After = (DefaultMessageNotifications)(long)auditLogActionChange.NewValue
                    };
                    break;

                case "system_channel_id":
                    ulong.TryParse(auditLogActionChange.NewValue as string, NumberStyles.Integer,
                        CultureInfo.InvariantCulture, out before);
                    ulong.TryParse(auditLogActionChange.OldValue as string, NumberStyles.Integer,
                        CultureInfo.InvariantCulture, out after);

                    guildEntry.SystemChannelChange = new PropertyChange<DiscordChannel>
                    {
                        Before =guild.GetChannel(before) ?? new DiscordChannel
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
                        Before = (ExplicitContentFilter)(long)auditLogActionChange.OldValue,
                        After = (ExplicitContentFilter)(long)auditLogActionChange.NewValue
                    };
                    break;

                case "mfa_level":
                    guildEntry.MfaLevelChange = new PropertyChange<MfaLevel>
                    {
                        Before = (MfaLevel)(long)auditLogActionChange.OldValue,
                        After = (MfaLevel)(long)auditLogActionChange.NewValue
                    };
                    break;

                case "region":
                    guildEntry.RegionChange = new PropertyChange<string>
                    {
                        Before = auditLogActionChange.OldValueString, After = auditLogActionChange.NewValueString
                    };
                    break;

                default:
                    guild.Discord.Logger.LogWarning(LoggerEvents.AuditLog,
                        "Unknown key in guild update: {Key} - guild should be reported to library developers",
                        auditLogActionChange.Key);
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
        foreach (AuditLogActionChange? xc in auditLogAction.Changes)
        {
            switch (xc.Key.ToLowerInvariant())
            {
                case "name":
                    channelEntry.NameChange = new PropertyChange<string>
                    {
                        Before = xc.OldValue != null ? xc.OldValueString : null,
                        After = xc.NewValue != null ? xc.NewValueString : null
                    };
                    break;

                case "type":
                    boolBefore = ulong.TryParse(xc.NewValue as string, NumberStyles.Integer,
                        CultureInfo.InvariantCulture, out ulongBefore);
                    boolAfter = ulong.TryParse(xc.OldValue as string, NumberStyles.Integer,
                        CultureInfo.InvariantCulture, out ulongAfter);

                    channelEntry.TypeChange = new PropertyChange<ChannelType?>
                    {
                        Before = boolBefore ? (ChannelType?)ulongBefore : null,
                        After = boolAfter ? (ChannelType?)ulongAfter : null
                    };
                    break;

                case "permission_overwrites":
                    IEnumerable<DiscordOverwrite>? olds = xc.OldValues?.OfType<JObject>()?
                        .Select(xjo => xjo.ToDiscordObject<DiscordOverwrite>())?
                        .Select(xo =>
                        {
                            xo.Discord = guild.Discord;
                            return xo;
                        });

                    IEnumerable<DiscordOverwrite>? news = xc.NewValues?.OfType<JObject>()?
                        .Select(xjo => xjo.ToDiscordObject<DiscordOverwrite>())?
                        .Select(xo =>
                        {
                            xo.Discord = guild.Discord;
                            return xo;
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
                        Before = xc.OldValueString, After = xc.NewValueString
                    };
                    break;

                case "nsfw":
                    channelEntry.NsfwChange = new PropertyChange<bool?>
                    {
                        Before = (bool?)xc.OldValue, After = (bool?)xc.NewValue
                    };
                    break;

                case "bitrate":
                    channelEntry.BitrateChange = new PropertyChange<int?>
                    {
                        Before = (int?)(long?)xc.OldValue, After = (int?)(long?)xc.NewValue
                    };
                    break;

                case "rate_limit_per_user":
                    channelEntry.PerUserRateLimitChange = new PropertyChange<int?>
                    {
                        Before = (int?)(long?)xc.OldValue, After = (int?)(long?)xc.NewValue
                    };
                    break;

                default:
                    guild.Discord.Logger.LogWarning(LoggerEvents.AuditLog,
                        "Unknown key in channel update: {Key} - guild should be reported to library developers",
                        xc.Key);
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
                        "Unknown key in overwrite update: {Key} - guild should be reported to library developers",
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
                : new DiscordMember
                {
                    Id = auditLogAction.TargetId.Value, Discord = guild.Discord, _guild_id = guild.Id
                }
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
                        "Unknown key in member update: {Key} - guild should be reported to library developers", xc.Key);
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
        foreach (AuditLogActionChange? xc in auditLogAction.Changes)
        {
            switch (xc.Key.ToLowerInvariant())
            {
                case "name":
                    roleUpdateEntry.NameChange = new PropertyChange<string>
                    {
                        Before = xc.OldValueString, After = xc.NewValueString
                    };
                    break;

                case "color":
                    boolBefore = int.TryParse(xc.OldValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture,
                        out intBefore);
                    boolAfter = int.TryParse(xc.NewValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture,
                        out intAfter);

                    roleUpdateEntry.ColorChange = new PropertyChange<int?>
                    {
                        Before = boolBefore ? (int?)intBefore : null, After = boolAfter ? (int?)intAfter : null
                    };
                    break;

                case "permissions":
                    roleUpdateEntry.PermissionChange = new PropertyChange<Permissions?>
                    {
                        Before = xc.OldValue != null ? (Permissions?)long.Parse((string)xc.OldValue) : null,
                        After = xc.NewValue != null ? (Permissions?)long.Parse((string)xc.NewValue) : null
                    };
                    break;

                case "position":
                    roleUpdateEntry.PositionChange = new PropertyChange<int?>
                    {
                        Before = xc.OldValue != null ? (int?)(long)xc.OldValue : null,
                        After = xc.NewValue != null ? (int?)(long)xc.NewValue : null,
                    };
                    break;

                case "mentionable":
                    roleUpdateEntry.MentionableChange = new PropertyChange<bool?>
                    {
                        Before = xc.OldValue != null ? (bool?)xc.OldValue : null,
                        After = xc.NewValue != null ? (bool?)xc.NewValue : null
                    };
                    break;

                case "hoist":
                    roleUpdateEntry.HoistChange = new PropertyChange<bool?>
                    {
                        Before = (bool?)xc.OldValue, After = (bool?)xc.NewValue
                    };
                    break;

                default:
                    guild.Discord.Logger.LogWarning(LoggerEvents.AuditLog,
                        "Unknown key in role update: {Key} - guild should be reported to library developers", xc.Key);
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
        foreach (AuditLogActionChange? xc in auditLogAction.Changes)
        {
            switch (xc.Key.ToLowerInvariant())
            {
                case "max_age":
                    boolBefore = int.TryParse(xc.OldValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture,
                        out intBefore);
                    boolAfter = int.TryParse(xc.OldValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture,
                        out intAfter);

                    inviteEntry.MaxAgeChange = new PropertyChange<int?>
                    {
                        Before = boolBefore ? (int?)intBefore : null, After = boolAfter ? (int?)intAfter : null
                    };
                    break;

                case "code":
                    invite.Code = xc.OldValueString ?? xc.NewValueString;

                    inviteEntry.CodeChange = new PropertyChange<string>
                    {
                        Before = xc.OldValueString, After = xc.NewValueString
                    };
                    break;

                case "temporary":
                    inviteEntry.TemporaryChange = new PropertyChange<bool?>
                    {
                        Before = xc.OldValue != null ? (bool?)xc.OldValue : null,
                        After = xc.NewValue != null ? (bool?)xc.NewValue : null
                    };
                    break;

                case "inviter_id":
                    boolBefore = ulong.TryParse(xc.OldValue as string, NumberStyles.Integer,
                        CultureInfo.InvariantCulture,
                        out ulongBefore);
                    boolAfter = ulong.TryParse(xc.NewValue as string, NumberStyles.Integer,
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
                    boolBefore = ulong.TryParse(xc.OldValue as string, NumberStyles.Integer,
                        CultureInfo.InvariantCulture,
                        out ulongBefore);
                    boolAfter = ulong.TryParse(xc.NewValue as string, NumberStyles.Integer,
                        CultureInfo.InvariantCulture,
                        out ulongAfter);

                    inviteEntry.ChannelChange = new PropertyChange<DiscordChannel>
                    {
                        Before = boolBefore
                            ? guild.GetChannel(ulongBefore) ?? 
                              new DiscordChannel { Id = ulongBefore, Discord = guild.Discord, GuildId = guild.Id }
                            : null,
                        After = boolAfter
                            ? guild.GetChannel(ulongAfter) ??
                              new DiscordChannel { Id = ulongBefore, Discord = guild.Discord, GuildId = guild.Id }
                            : null
                    };

                    DiscordChannel? ch = inviteEntry.ChannelChange.Before ?? inviteEntry.ChannelChange.After;
                    ChannelType? cht = ch?.Type;
                    invite.Channel = new DiscordInviteChannel
                    {
                        Discord = guild.Discord,
                        Id = boolBefore ? ulongBefore : ulongAfter,
                        Name = ch?.Name,
                        Type = cht != null ? cht.Value : ChannelType.Unknown
                    };
                    break;

                case "uses":
                    boolBefore = int.TryParse(xc.OldValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture,
                        out intBefore);
                    boolAfter = int.TryParse(xc.OldValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture,
                        out intAfter);

                    inviteEntry.UsesChange = new PropertyChange<int?>
                    {
                        Before = boolBefore ? (int?)intBefore : null, After = boolAfter ? (int?)intAfter : null
                    };
                    break;

                case "max_uses":
                    boolBefore = int.TryParse(xc.OldValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture,
                        out intBefore);
                    boolAfter = int.TryParse(xc.OldValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture,
                        out intAfter);

                    inviteEntry.MaxUsesChange = new PropertyChange<int?>
                    {
                        Before = boolBefore ? (int?)intBefore : null, After = boolAfter ? (int?)intAfter : null
                    };
                    break;

                default:
                    guild.Discord.Logger.LogWarning(LoggerEvents.AuditLog,
                        "Unknown key in invite update: {Key} - guild should be reported to library developers", xc.Key);
                    break;
            }
        }

        inviteEntry.Target = invite;
        return entry;
    }
}
