// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Text.Json;

using DSharpPlus.Core.Abstractions.Models;
using DSharpPlus.Core.Models.Serialization.Converters;
using DSharpPlus.Core.Models.Serialization.Resolvers;
using DSharpPlus.Serialization;

using Microsoft.Extensions.DependencyInjection;

namespace DSharpPlus.Core.Models.Extensions;

/// <summary>
/// Provides extensions on IServiceCollection to register our JSON serialization of Discord models.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers converters for Discord's API models.
    /// </summary>
    /// <param name="services">The service collection to register into.</param>
    /// <param name="name">The name under which the serialization options should be accessible.</param>
    /// <returns>The same service collection for chaining.</returns>
    public static IServiceCollection RegisterDiscordModelSerialization
    (
        this IServiceCollection services,
        string? name = "dsharpplus"
    )
    {
        services.Configure<JsonSerializerOptions>
        (
            name,
            options =>
            {
                options.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;

                options.Converters.Add(new AuditLogChangeConverter());
                options.Converters.Add(new AutoModerationActionConverter());
                options.Converters.Add(new DiscordPermissionConverter());
                options.Converters.Add(new MessageComponentConverter());

                options.TypeInfoResolverChain.Add(OptionalTypeInfoResolver.Default);
                options.TypeInfoResolverChain.Add(NullBooleanTypeInfoResolver.Default);
            }
        );

        services.Configure<SerializationOptions>
        (
            options =>
            {
                options.AddModel<IApplicationCommand, ApplicationCommand>();
                options.AddModel<IApplicationCommandOption, ApplicationCommandOption>();
                options.AddModel<IApplicationCommandOptionChoice, ApplicationCommandOptionChoice>();
                options.AddModel<IApplicationCommandPermission, ApplicationCommandPermission>();
                options.AddModel<IApplicationCommandPermissions, ApplicationCommandPermissions>();
                options.AddModel<IPartialApplicationCommandPermissions, PartialApplicationCommandPermissions>();

                options.AddModel<IApplication, Application>();
                options.AddModel<IInstallParameters, InstallParameters>();
                options.AddModel<IPartialApplication, PartialApplication>();

                options.AddModel<IAutoModerationActionMetadata, AutoModerationActionMetadata>();
                options.AddModel<IAutoModerationRule, AutoModerationRule>();
                options.AddModel<IAutoModerationTriggerMetadata, AutoModerationTriggerMetadata>();
                options.AddModel<IBlockMessageActionMetadata, BlockMessageActionMetadata>();
                options.AddModel<IPartialAutoModerationRule, PartialAutoModerationRule>();
                options.AddModel<ISendAlertMessageActionMetadata, SendAlertMessageActionMetadata>();
                options.AddModel<ITimeoutActionMetadata, TimeoutActionMetadata>();

                options.AddModel<IAllowedMentions, AllowedMentions>();
                options.AddModel<IAttachment, Attachment>();
                options.AddModel<IChannel, Channel>();
                options.AddModel<IChannelMention, ChannelMention>();
                options.AddModel<IChannelOverwrite, ChannelOverwrite>();
                options.AddModel<IDefaultReaction, DefaultReaction>();
                options.AddModel<IEmbed, Embed>();
                options.AddModel<IEmbedAuthor, EmbedAuthor>();
                options.AddModel<IEmbedField, EmbedField>();
                options.AddModel<IEmbedFooter, EmbedFooter>();
                options.AddModel<IEmbedImage, EmbedImage>();
                options.AddModel<IEmbedProvider, EmbedProvider>();
                options.AddModel<IEmbedThumbnail, EmbedThumbnail>();
                options.AddModel<IEmbedVideo, EmbedVideo>();
                options.AddModel<IFollowedChannel, FollowedChannel>();
                options.AddModel<IForumTag, ForumTag>();
                options.AddModel<IMessage, Message>();
                options.AddModel<IMessageActivity, MessageActivity>();
                options.AddModel<IMessageReference, MessageReference>();
                options.AddModel<IPartialAttachment, PartialAttachment>();
                options.AddModel<IPartialChannel, PartialChannel>();
                options.AddModel<IPartialChannelOverwrite, PartialChannelOverwrite>();
                options.AddModel<IPartialMessage, PartialMessage>();
                options.AddModel<IReaction, Reaction>();
                options.AddModel<IRoleSubscriptionData, RoleSubscriptionData>();
                options.AddModel<IThreadMember, ThreadMember>();
                options.AddModel<IThreadMetadata, ThreadMetadata>();

                options.AddModel<IEmoji, Emoji>();
                options.AddModel<IPartialEmoji, PartialEmoji>();

                options.AddModel<IEntitlement, Entitlement>();

                options.AddModel<IBan, Ban>();
                options.AddModel<IGuild, Guild>();
                options.AddModel<IGuildMember, GuildMember>();
                options.AddModel<IGuildPreview, GuildPreview>();
                options.AddModel<IGuildWidget, GuildWidget>();
                options.AddModel<IGuildWidgetSettings, GuildWidgetSettings>();
                options.AddModel<IIntegration, Integration>();
                options.AddModel<IIntegrationAccount, IntegrationAccount>();
                options.AddModel<IIntegrationApplication, IntegrationApplication>();
                options.AddModel<IOnboarding, Onboarding>();
                options.AddModel<IOnboardingPrompt, OnboardingPrompt>();
                options.AddModel<IPartialGuild, PartialGuild>();
                options.AddModel<IPartialGuildMember, PartialGuildMember>();
                options.AddModel<IPartialIntegration, PartialIntegration>();
                options.AddModel<IPartialRole, PartialRole>();
                options.AddModel<IRole, Role>();
                options.AddModel<IWelcomeScreen, WelcomeScreen>();
                options.AddModel<IWelcomeScreenChannel, WelcomeScreenChannel>();

                options.AddModel<ITemplate, Template>();

                options.AddModel<IApplicationCommandInteractionData, ApplicationCommandInteractionData>();
                options.AddModel<IApplicationCommandInteractionDataOption, ApplicationCommandInteractionDataOption>();
                options.AddModel<IAutocompleteCallbackData, AutocompleteCallbackData>();
                options.AddModel<IInteraction, Interaction>();
                options.AddModel<IInteractionResponse, InteractionResponse>();
                options.AddModel<IMessageCallbackData, MessageCallbackData>();
                options.AddModel<IMessageComponentInteractionData, MessageComponentInteractionData>();
                options.AddModel<IMessageInteraction, MessageInteraction>();
                options.AddModel<IModalCallbackData, ModalCallbackData>();
                options.AddModel<IModalInteractionData, ModalInteractionData>();
                options.AddModel<IResolvedData, ResolvedData>();

                options.AddModel<IInvite, Invite>();
                options.AddModel<IPartialInvite, PartialInvite>();

                options.AddModel<IActionRowComponent, ActionRowComponent>();
                options.AddModel<IButtonComponent, ButtonComponent>();
                options.AddModel<IChannelSelectComponent, ChannelSelectComponent>();
                options.AddModel<IMentionableSelectComponent, MentionableSelectComponent>();
                options.AddModel<IRoleSelectComponent, RoleSelectComponent>();
                options.AddModel<ISelectOption, SelectOption>();
                options.AddModel<IStringSelectComponent, StringSelectComponent>();
                options.AddModel<ITextInputComponent, TextInputComponent>();
                options.AddModel<IUserSelectComponent, UserSelectComponent>();

                options.AddModel<IRoleConnectionMetadata, RoleConnectionMetadata>();

                options.AddModel<IPartialScheduledEvent, PartialScheduledEvent>();
                options.AddModel<IScheduledEvent, ScheduledEvent>();
                options.AddModel<IScheduledEventUser, ScheduledEventUser>();

                options.AddModel<ISku, Sku>();

                options.AddModel<IPartialStageInstance, PartialStageInstance>();
                options.AddModel<IStageInstance, StageInstance>();

                options.AddModel<IPartialSticker, PartialSticker>();
                options.AddModel<ISticker, Sticker>();
                options.AddModel<IStickerItem, StickerItem>();
                options.AddModel<IStickerPack, StickerPack>();

                options.AddModel<ITeam, Team>();
                options.AddModel<ITeamMember, TeamMember>();

                options.AddModel<IApplicationRoleConnection, ApplicationRoleConnection>();
                options.AddModel<IConnection, Connection>();
                options.AddModel<IPartialUser, PartialUser>();
                options.AddModel<IUser, User>();

                options.AddModel<IVoiceRegion, VoiceRegion>();
                options.AddModel<IVoiceState, VoiceState>();

                options.AddModel<IPartialWebhook, PartialWebhook>();
                options.AddModel<IWebhook, Webhook>();
            }
        );

        return services;
    }
}
