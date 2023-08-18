// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

using DSharpPlus.Core.Abstractions.Models;
using DSharpPlus.Core.Models.Converters;

using Remora.Rest.Extensions;

namespace DSharpPlus.Core.Models.Extensions;

/// <summary>
/// Provides segmented registration for Discord API object serialization.
/// </summary>
internal static class JsonSerializerOptionsExtensions
{
    /// <summary>
    /// Registers object converters for application commands.
    /// </summary>
    public static void RegisterApplicationCommands
    (
        this JsonSerializerOptions options
    )
    {
        options.AddDataObjectConverter<IApplicationCommand, ApplicationCommand>();
        options.AddDataObjectConverter<IApplicationCommandOption, ApplicationCommandOption>();
        options.AddDataObjectConverter<IApplicationCommandOptionChoice, ApplicationCommandOptionChoice>();
        options.AddDataObjectConverter<IApplicationCommandPermission, ApplicationCommandPermission>();
        options.AddDataObjectConverter<IApplicationCommandPermissions, ApplicationCommandPermissions>();
        options.AddDataObjectConverter<IPartialApplicationCommandPermissions, PartialApplicationCommandPermissions>();
    }

    /// <summary>
    /// Registers object converters for applications.
    /// </summary>
    public static void RegisterApplications
    (
        this JsonSerializerOptions options
    )
    {
        options.AddDataObjectConverter<IApplication, Application>();
        options.AddDataObjectConverter<IInstallParameters, InstallParameters>();
        options.AddDataObjectConverter<IPartialApplication, PartialApplication>();
    }

    /// <summary>
    /// Registers object converters for audit logs.
    /// </summary>
    public static void RegisterAuditLogs
    (
        this JsonSerializerOptions options
    )
    {
        options.AddDataObjectConverter<IAuditLog, AuditLog>();
        options.AddDataObjectConverter<IAuditLogEntry, AuditLogEntry>();
        options.AddDataObjectConverter<IAuditLogEntryInfo, AuditLogEntryInfo>();
    }

    /// <summary>
    /// Registers object converters for auto moderation.
    /// </summary>
    public static void RegisterAutoModeration
    (
        this JsonSerializerOptions options
    )
    {
        options.AddDataObjectConverter<IAutoModerationActionMetadata, AutoModerationActionMetadata>();
        options.AddDataObjectConverter<IAutoModerationRule, AutoModerationRule>();
        options.AddDataObjectConverter<IAutoModerationTriggerMetadata, AutoModerationTriggerMetadata>();
        options.AddDataObjectConverter<IBlockMessageActionMetadata, BlockMessageActionMetadata>();
        options.AddDataObjectConverter<IPartialAutoModerationRule, PartialAutoModerationRule>();
        options.AddDataObjectConverter<ISendAlertMessageActionMetadata, SendAlertMessageActionMetadata>();
        options.AddDataObjectConverter<ITimeoutActionMetadata, TimeoutActionMetadata>();
    }

    /// <summary>
    /// Registers object converters for channels.
    /// </summary>
    public static void RegisterChannels
    (
        this JsonSerializerOptions options
    )
    {
        options.AddDataObjectConverter<IAllowedMentions, AllowedMentions>();
        options.AddDataObjectConverter<IAttachment, Attachment>();
        options.AddDataObjectConverter<IChannel, Channel>();
        options.AddDataObjectConverter<IChannelMention, ChannelMention>();
        options.AddDataObjectConverter<IChannelOverwrite, ChannelOverwrite>();
        options.AddDataObjectConverter<IDefaultReaction, DefaultReaction>();
        options.AddDataObjectConverter<IEmbed, Embed>();
        options.AddDataObjectConverter<IEmbedAuthor, EmbedAuthor>();
        options.AddDataObjectConverter<IEmbedField, EmbedField>();
        options.AddDataObjectConverter<IEmbedFooter, EmbedFooter>();
        options.AddDataObjectConverter<IEmbedImage, EmbedImage>();
        options.AddDataObjectConverter<IEmbedProvider, EmbedProvider>();
        options.AddDataObjectConverter<IEmbedThumbnail, EmbedThumbnail>();
        options.AddDataObjectConverter<IEmbedVideo, EmbedVideo>();
        options.AddDataObjectConverter<IFollowedChannel, FollowedChannel>();
        options.AddDataObjectConverter<IForumTag, ForumTag>();
        options.AddDataObjectConverter<IMessage, Message>();
        options.AddDataObjectConverter<IMessageActivity, MessageActivity>();
        options.AddDataObjectConverter<IMessageReference, MessageReference>();
        options.AddDataObjectConverter<IPartialAttachment, PartialAttachment>();
        options.AddDataObjectConverter<IPartialChannel, PartialChannel>();
        options.AddDataObjectConverter<IPartialChannelOverwrite, PartialChannelOverwrite>();
        options.AddDataObjectConverter<IPartialMessage, PartialMessage>();
        options.AddDataObjectConverter<IReaction, Reaction>();
        options.AddDataObjectConverter<IRoleSubscriptionData, RoleSubscriptionData>();
        options.AddDataObjectConverter<IThreadMember, ThreadMember>();
        options.AddDataObjectConverter<IThreadMetadata, ThreadMetadata>();
    }

    /// <summary>
    /// Registers object converters for emojis.
    /// </summary>
    public static void RegisterEmojis
    (
        this JsonSerializerOptions options
    )
    {
        options.AddDataObjectConverter<IEmoji, Emoji>();
        options.AddDataObjectConverter<IPartialEmoji, PartialEmoji>();
    }

    /// <summary>
    /// Registers object converters for guilds.
    /// </summary>
    public static void RegisterGuilds
    (
        this JsonSerializerOptions options
    )
    {
        options.AddDataObjectConverter<IBan, Ban>();
        options.AddDataObjectConverter<IGuild, Guild>();
        options.AddDataObjectConverter<IGuildMember, GuildMember>();
        options.AddDataObjectConverter<IGuildPreview, GuildPreview>();
        options.AddDataObjectConverter<IGuildWidget, GuildWidget>();
        options.AddDataObjectConverter<IGuildWidgetSettings, GuildWidgetSettings>();
        options.AddDataObjectConverter<IIntegration, Integration>();
        options.AddDataObjectConverter<IIntegrationAccount, IntegrationAccount>();
        options.AddDataObjectConverter<IIntegrationApplication, IntegrationApplication>();
        options.AddDataObjectConverter<IOnboarding, Onboarding>();
        options.AddDataObjectConverter<IOnboardingPrompt, OnboardingPrompt>();
        options.AddDataObjectConverter<IPartialGuild, PartialGuild>();
        options.AddDataObjectConverter<IPartialGuildMember, PartialGuildMember>();
        options.AddDataObjectConverter<IPartialIntegration, PartialIntegration>();
        options.AddDataObjectConverter<IPartialRole, PartialRole>();
        options.AddDataObjectConverter<IRole, Role>();

        options.AddDataObjectConverter<IRoleTags, RoleTags>()
            .WithPropertyConverter
            (
                o => o.AvailableForPurchase,
                new NullBooleanJsonConverter()
            )
            .WithPropertyConverter
            (
                o => o.GuildConnections,
                new NullBooleanJsonConverter()
            )
            .WithPropertyConverter
            (
                o => o.PremiumSubscriber,
                new NullBooleanJsonConverter()
            );

        options.TypeInfoResolverChain.Add
        (
            new DefaultJsonTypeInfoResolver
            {
                Modifiers =
                {
                    (JsonTypeInfo type) =>
                    {
                        if (type.Type != typeof(RoleTags))
                        {
                            return;
                        }

                        foreach (JsonPropertyInfo property in type.Properties)
                        {
                            if (property.PropertyType == typeof(bool))
                            {
                                property.IsRequired = false;
                            }
                        }
                    }
                }
            }
        );

        options.AddDataObjectConverter<IWelcomeScreen, WelcomeScreen>();
        options.AddDataObjectConverter<IWelcomeScreenChannel, WelcomeScreenChannel>();
    }

    /// <summary>
    /// Registers object converters for guild templates.
    /// </summary>
    public static void RegisterGuildTemplates
    (
        this JsonSerializerOptions options
    )
        => options.AddDataObjectConverter<ITemplate, Template>();

    /// <summary>
    /// Registers object converters for interactions.
    /// </summary>
    public static void RegisterInteractions
    (
        this JsonSerializerOptions options
    )
    {
        options.AddDataObjectConverter<IApplicationCommandInteractionData, ApplicationCommandInteractionData>();
        options.AddDataObjectConverter<IApplicationCommandInteractionDataOption, ApplicationCommandInteractionDataOption>();
        options.AddDataObjectConverter<IAutocompleteCallbackData, AutocompleteCallbackData>();
        options.AddDataObjectConverter<IInteraction, Interaction>();
        options.AddDataObjectConverter<IInteractionResponse, InteractionResponse>();
        options.AddDataObjectConverter<IMessageCallbackData, MessageCallbackData>();
        options.AddDataObjectConverter<IMessageComponentInteractionData, MessageComponentInteractionData>();
        options.AddDataObjectConverter<IMessageInteraction, MessageInteraction>();
        options.AddDataObjectConverter<IModalCallbackData, ModalCallbackData>();
        options.AddDataObjectConverter<IModalInteractionData, ModalInteractionData>();
        options.AddDataObjectConverter<IResolvedData, ResolvedData>();
    }

    /// <summary>
    /// Registers object converters for invites.
    /// </summary>
    public static void RegisterInvites
    (
        this JsonSerializerOptions options
    )
    {
        options.AddDataObjectConverter<IInvite, Invite>();
        options.AddDataObjectConverter<IPartialInvite, PartialInvite>();
    }

    /// <summary>
    /// Registers object converters for message components.
    /// </summary>
    public static void RegisterMessageComponents
    (
        this JsonSerializerOptions options
    )
    {
        options.AddDataObjectConverter<IActionRowComponent, ActionRowComponent>();
        options.AddDataObjectConverter<IButtonComponent, ButtonComponent>();
        options.AddDataObjectConverter<IChannelSelectComponent, ChannelSelectComponent>();
        options.AddDataObjectConverter<IMentionableSelectComponent, MentionableSelectComponent>();
        options.AddDataObjectConverter<IRoleSelectComponent, RoleSelectComponent>();
        options.AddDataObjectConverter<ISelectOption, SelectOption>();
        options.AddDataObjectConverter<IStringSelectComponent, StringSelectComponent>();
        options.AddDataObjectConverter<ITextInputComponent, TextInputComponent>();
        options.AddDataObjectConverter<IUserSelectComponent, UserSelectComponent>();
    }

    /// <summary>
    /// Registers object converters for role connections.
    /// </summary>
    public static void RegisterRoleConnections
    (
        this JsonSerializerOptions options
    )
        => options.AddDataObjectConverter<IRoleConnectionMetadata, RoleConnectionMetadata>();

    /// <summary>
    /// Registers object converters for scheduled events.
    /// </summary>
    public static void RegisterScheduledEvents
    (
        this JsonSerializerOptions options
    )
    {
        options.AddDataObjectConverter<IPartialScheduledEvent, PartialScheduledEvent>();
        options.AddDataObjectConverter<IScheduledEvent, ScheduledEvent>();
        options.AddDataObjectConverter<IScheduledEventUser, ScheduledEventUser>();
    }

    /// <summary>
    /// Registers object converters for stage instances.
    /// </summary>
    public static void RegisterStageInstances
    (
        this JsonSerializerOptions options
    )
    {
        options.AddDataObjectConverter<IPartialStageInstance, PartialStageInstance>();
        options.AddDataObjectConverter<IStageInstance, StageInstance>();
    }

    /// <summary>
    /// Registers object converters for stickers.
    /// </summary>
    public static void RegisterStickers
    (
        this JsonSerializerOptions options
    )
    {
        options.AddDataObjectConverter<IPartialSticker, PartialSticker>();
        options.AddDataObjectConverter<ISticker, Sticker>();
        options.AddDataObjectConverter<IStickerItem, StickerItem>();
        options.AddDataObjectConverter<IStickerPack, StickerPack>();
    }

    /// <summary>
    /// Registers object converters for teams.
    /// </summary>
    public static void RegisterTeams
    (
        this JsonSerializerOptions options
    )
    {
        options.AddDataObjectConverter<ITeam, Team>();
        options.AddDataObjectConverter<ITeamMember, TeamMember>();
    }

    /// <summary>
    /// Registers object converters for users.
    /// </summary>
    public static void RegisterUsers
    (
        this JsonSerializerOptions options
    )
    {
        options.AddDataObjectConverter<IApplicationRoleConnection, ApplicationRoleConnection>();
        options.AddDataObjectConverter<IConnection, Connection>();
        options.AddDataObjectConverter<IPartialUser, PartialUser>();
        options.AddDataObjectConverter<IUser, User>();
    }

    /// <summary>
    /// Registers object converters for voice.
    /// </summary>
    public static void RegisterVoice
    (
        this JsonSerializerOptions options
    )
    {
        options.AddDataObjectConverter<IVoiceRegion, VoiceRegion>();
        options.AddDataObjectConverter<IVoiceState, VoiceState>();
    }

    /// <summary>
    /// Registers object converters for webhooks.
    /// </summary>
    public static void RegisterWebhooks
    (
        this JsonSerializerOptions options
    )
    {
        options.AddDataObjectConverter<IPartialWebhook, PartialWebhook>();
        options.AddDataObjectConverter<IWebhook, Webhook>();
    }
}
