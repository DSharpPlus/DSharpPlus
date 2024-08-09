// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

#pragma warning disable IDE0058

using DSharpPlus.Internal.Abstractions.Models;
using DSharpPlus.Internal.Models;
using DSharpPlus.Serialization;

using Microsoft.Extensions.DependencyInjection;

namespace DSharpPlus.Internal.Models.Extensions;

partial class ServiceCollectionExtensions
{
    private static void RegisterSerialization(IServiceCollection services)
    {
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
                options.AddModel<IApplicationIntegrationTypeConfiguration, ApplicationIntegrationTypeConfiguration>();
                options.AddModel<IInstallParameters, InstallParameters>();
                options.AddModel<IPartialApplication, PartialApplication>();
                options.AddModel<IAuditLog, AuditLog>();
                options.AddModel<IAuditLogChange, AuditLogChange>();
                options.AddModel<IAuditLogEntry, AuditLogEntry>();
                options.AddModel<IAuditLogEntryInfo, AuditLogEntryInfo>();
                options.AddModel<IAutoModerationAction, AutoModerationAction>();
                options.AddModel<IAutoModerationActionMetadata, AutoModerationActionMetadata>();
                options.AddModel<IAutoModerationRule, AutoModerationRule>();
                options.AddModel<IAutoModerationTriggerMetadata, AutoModerationTriggerMetadata>();
                options.AddModel<IBlockMessageActionMetadata, BlockMessageActionMetadata>();
                options.AddModel<IPartialAutoModerationRule, PartialAutoModerationRule>();
                options.AddModel<ISendAlertMessageActionMetadata, SendAlertMessageActionMetadata>();
                options.AddModel<ITimeoutActionMetadata, TimeoutActionMetadata>();
                options.AddModel<IChannel, Channel>();
                options.AddModel<IChannelOverwrite, ChannelOverwrite>();
                options.AddModel<IDefaultReaction, DefaultReaction>();
                options.AddModel<IFollowedChannel, FollowedChannel>();
                options.AddModel<IForumTag, ForumTag>();
                options.AddModel<IPartialChannel, PartialChannel>();
                options.AddModel<IPartialChannelOverwrite, PartialChannelOverwrite>();
                options.AddModel<IThreadMember, ThreadMember>();
                options.AddModel<IThreadMetadata, ThreadMetadata>();
                options.AddModel<IEmoji, Emoji>();
                options.AddModel<IPartialEmoji, PartialEmoji>();
                options.AddModel<IEntitlement, Entitlement>();
                options.AddModel<IPartialEntitlement, PartialEntitlement>();
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
                options.AddModel<IOnboardingPromptOption, OnboardingPromptOption>();
                options.AddModel<IPartialGuild, PartialGuild>();
                options.AddModel<IPartialGuildMember, PartialGuildMember>();
                options.AddModel<IPartialIntegration, PartialIntegration>();
                options.AddModel<IPartialRole, PartialRole>();
                options.AddModel<IRole, Role>();
                options.AddModel<IRoleTags, RoleTags>();
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
                options.AddModel<IModalCallbackData, ModalCallbackData>();
                options.AddModel<IModalInteractionData, ModalInteractionData>();
                options.AddModel<IResolvedData, ResolvedData>();
                options.AddModel<IInvite, Invite>();
                options.AddModel<IPartialInvite, PartialInvite>();
                options.AddModel<IActionRowComponent, ActionRowComponent>();
                options.AddModel<IButtonComponent, ButtonComponent>();
                options.AddModel<IChannelSelectComponent, ChannelSelectComponent>();
                options.AddModel<IDefaultSelectValue, DefaultSelectValue>();
                options.AddModel<IInteractiveComponent, InteractiveComponent>();
                options.AddModel<IMentionableSelectComponent, MentionableSelectComponent>();
                options.AddModel<IRoleSelectComponent, RoleSelectComponent>();
                options.AddModel<ISelectOption, SelectOption>();
                options.AddModel<IStringSelectComponent, StringSelectComponent>();
                options.AddModel<ITextInputComponent, TextInputComponent>();
                options.AddModel<IUserSelectComponent, UserSelectComponent>();
                options.AddModel<IAllowedMentions, AllowedMentions>();
                options.AddModel<IAttachment, Attachment>();
                options.AddModel<IChannelMention, ChannelMention>();
                options.AddModel<IEmbed, Embed>();
                options.AddModel<IEmbedAuthor, EmbedAuthor>();
                options.AddModel<IEmbedField, EmbedField>();
                options.AddModel<IEmbedFooter, EmbedFooter>();
                options.AddModel<IEmbedImage, EmbedImage>();
                options.AddModel<IEmbedProvider, EmbedProvider>();
                options.AddModel<IEmbedThumbnail, EmbedThumbnail>();
                options.AddModel<IEmbedVideo, EmbedVideo>();
                options.AddModel<IMessage, Message>();
                options.AddModel<IMessageActivity, MessageActivity>();
                options.AddModel<IMessageCall, MessageCall>();
                options.AddModel<IMessageInteractionMetadata, MessageInteractionMetadata>();
                options.AddModel<IMessageReference, MessageReference>();
                options.AddModel<IMessageSnapshot, MessageSnapshot>();
                options.AddModel<IPartialAttachment, PartialAttachment>();
                options.AddModel<IPartialMessage, PartialMessage>();
                options.AddModel<IReaction, Reaction>();
                options.AddModel<IReactionCountDetails, ReactionCountDetails>();
                options.AddModel<IRoleSubscriptionData, RoleSubscriptionData>();
                options.AddModel<ICreatePoll, CreatePoll>();
                options.AddModel<IPoll, Poll>();
                options.AddModel<IPollAnswer, PollAnswer>();
                options.AddModel<IPollAnswerCount, PollAnswerCount>();
                options.AddModel<IPollMedia, PollMedia>();
                options.AddModel<IPollResults, PollResults>();
                options.AddModel<IRoleConnectionMetadata, RoleConnectionMetadata>();
                options.AddModel<IPartialScheduledEvent, PartialScheduledEvent>();
                options.AddModel<IScheduledEvent, ScheduledEvent>();
                options.AddModel<IScheduledEventMetadata, ScheduledEventMetadata>();
                options.AddModel<IScheduledEventRecurrenceDay, ScheduledEventRecurrenceDay>();
                options.AddModel<IScheduledEventRecurrenceRule, ScheduledEventRecurrenceRule>();
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
                options.AddModel<IAvatarDecorationData, AvatarDecorationData>();
                options.AddModel<IConnection, Connection>();
                options.AddModel<IPartialUser, PartialUser>();
                options.AddModel<IUser, User>();
                options.AddModel<IVoiceRegion, VoiceRegion>();
                options.AddModel<IVoiceState, VoiceState>();
                options.AddModel<IPartialWebhook, PartialWebhook>();
                options.AddModel<IWebhook, Webhook>();
            }
        );
    }
}