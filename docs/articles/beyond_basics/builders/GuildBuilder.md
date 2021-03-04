---
uid: beyond_basics_builder_guildbuilder
title: Guild Builder
---

## Background
Before the guild builder was put into place, we had one large method for creating a guild and 2 methods for modifying.  This
was becoming a major code smell and it was hard to maintain and add more params onto it. Now we support either sending a prebuilt 
builder OR an action of a builder.  

## Using the Message Builder
The API Documentation for the guild builder can be found at @DSharpPlus.Entities.DiscordGuildCreateBuilder and @DSharpPlus.Entities.DiscordGuildModifyBuilder but here we'll go over some of the concepts of using the
guild builder:

### Creating a Guild
When Creating a guild, you can create it by pre-building the builder then calling CreateAsync 
```cs
var guild = await new DiscordGuildCreateBuilder()
    .WithName("My Awesome Guild Name")
    .WithDefaultMessageNotificationLevel(DefaultMessageNotifications.MentionsOnly)
    .WithExplicitContentFilterLevel(ExplicitContentFilter.MembersWithoutRoles)
    .WithVerificationLevel(VerificationLevel.Low)
    .WithRoles(new DiscordGuildCreateBuilder.GuildBuilderRole[] {
            new DiscordGuildCreateBuilder.GuildBuilderRole { 
                Id = Convert.ToUInt64(1), 
                Mentionable = true, 
                Name = "Everyone", 
                Permissions = Permissions.Administrator 
            },
            new DiscordGuildCreateBuilder.GuildBuilderRole { 
                Id = Convert.ToUInt64(2), 
                Mentionable = true, 
                Name = "Role 1" 
            },
            new DiscordGuildCreateBuilder.GuildBuilderRole { 
                Id = Convert.ToUInt64(3), 
                Mentionable = true, 
                Name = "Role 2" 
            },
    })
    .WithChannels(new DiscordGuildCreateBuilder.GuildBuilderChannel[] {
            new DiscordGuildCreateBuilder.GuildBuilderChannel { 
                Id = Convert.ToUInt64(4), 
                Name = "test General", 
                Type = ChannelType.Category, 
                PermissionOverwrites = new DiscordGuildCreateBuilder.GuildBuilderChannel.ChannelOverwrite[] { } 
            },
            new DiscordGuildCreateBuilder.GuildBuilderChannel { 
                Id = Convert.ToUInt64(5), 
                Name = "test text General", 
                Type = ChannelType.Text, 
                ParentId = Convert.ToUInt64(4), 
                Nsfw = true, 
                Topic = "Some dumb topic", 
                PermissionOverwrites = new DiscordGuildCreateBuilder.GuildBuilderChannel.ChannelOverwrite[] {
                    new DiscordGuildCreateBuilder.GuildBuilderChannel.ChannelOverwrite { 
                        Id = Convert.ToUInt64(2), 
                        DenyPermissions = Permissions.All, 
                        AllowPermissions = Permissions.None 
                    },
                    new DiscordGuildCreateBuilder.GuildBuilderChannel.ChannelOverwrite { 
                        Id = Convert.ToUInt64(3), 
                        AllowPermissions = Permissions.All, 
                        DenyPermissions = Permissions.None 
                    }
                }
            }
        })
    .CreateAsync(ctx.Client);
```
OR u can create the guild by using an action

```cs
var guild = await ctx.Client.CreateGuildAsync(x => {
    x.WithName("My Awesome Guild Name")
    .WithDefaultMessageNotificationLevel(DefaultMessageNotifications.MentionsOnly)
    .WithExplicitContentFilterLevel(ExplicitContentFilter.MembersWithoutRoles)
    .WithVerificationLevel(VerificationLevel.Low)
    .WithRoles(new DiscordGuildCreateBuilder.GuildBuilderRole[] {
            new DiscordGuildCreateBuilder.GuildBuilderRole { 
                Id = Convert.ToUInt64(1), 
                Mentionable = true, 
                Name = "Everyone", 
                Permissions = Permissions.Administrator 
            },
            new DiscordGuildCreateBuilder.GuildBuilderRole { 
                Id = Convert.ToUInt64(2), 
                Mentionable = true, 
                Name = "Role 1" 
            },
            new DiscordGuildCreateBuilder.GuildBuilderRole { 
                Id = Convert.ToUInt64(3), 
                Mentionable = true, 
                Name = "Role 2" 
            },
    })
    .WithChannels(new DiscordGuildCreateBuilder.GuildBuilderChannel[] {
            new DiscordGuildCreateBuilder.GuildBuilderChannel { 
                Id = Convert.ToUInt64(4), 
                Name = "test General", 
                Type = ChannelType.Category, 
                PermissionOverwrites = new DiscordGuildCreateBuilder.GuildBuilderChannel.ChannelOverwrite[] { } 
            },
            new DiscordGuildCreateBuilder.GuildBuilderChannel { 
                Id = Convert.ToUInt64(5), 
                Name = "test text General", 
                Type = ChannelType.Text, 
                ParentId = Convert.ToUInt64(4), 
                Nsfw = true, 
                Topic = "Some dumb topic", 
                PermissionOverwrites = new DiscordGuildCreateBuilder.GuildBuilderChannel.ChannelOverwrite[] {
                    new DiscordGuildCreateBuilder.GuildBuilderChannel.ChannelOverwrite { 
                        Id = Convert.ToUInt64(2), 
                        DenyPermissions = Permissions.All, 
                        AllowPermissions = Permissions.None 
                    },
                    new DiscordGuildCreateBuilder.GuildBuilderChannel.ChannelOverwrite { 
                        Id = Convert.ToUInt64(3), 
                        AllowPermissions = Permissions.All, 
                        DenyPermissions = Permissions.None 
                    }
                }
            }
        });
});
```

### Modifing a Guild

When modifing a guild, you can create it by pre-building the builder then calling ModifyAsync 
```cs 
await new DiscordGuildModifyBuilder()
    .WithName("My Awesome Test Guild")
    .WithAuditLogReason("Cause i can")
    .WithNewOwener(ctx.Member)
    .ModifyAsync(ctx.Guild);
```

OR u can modify the guild by using an action

```cs
await ctx.Guild.ModifyAsync(g =>
{
    g.WithName("My Awesome Test Guild")
    .WithAuditLogReason("Cause i can")
    .WithNewOwener(ctx.Member);
});
```