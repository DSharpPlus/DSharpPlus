// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

#pragma warning disable IDE0058

using DSharpPlus.Internal.Abstractions.Rest.Payloads;
using DSharpPlus.Internal.Rest.Payloads;
using DSharpPlus.Serialization;

using Microsoft.Extensions.DependencyInjection;

namespace DSharpPlus.Internal.Rest.Extensions;

partial class ServiceCollectionExtensions
{
    private static void RegisterSerialization(IServiceCollection services)
    {
        services.Configure<SerializationOptions>
        (
            options =>
            {
                options.AddModel<ICreateGlobalApplicationCommandPayload, CreateGlobalApplicationCommandPayload>();
                options.AddModel<ICreateGuildApplicationCommandPayload, CreateGuildApplicationCommandPayload>();
                options.AddModel<IEditGlobalApplicationCommandPayload, EditGlobalApplicationCommandPayload>();
                options.AddModel<IEditGuildApplicationCommandPayload, EditGuildApplicationCommandPayload>();
                options.AddModel<IEditCurrentApplicationPayload, EditCurrentApplicationPayload>();
                options.AddModel<ICreateAutoModerationRulePayload, CreateAutoModerationRulePayload>();
                options.AddModel<IModifyAutoModerationRulePayload, ModifyAutoModerationRulePayload>();
                options.AddModel<ICreateChannelInvitePayload, CreateChannelInvitePayload>();
                options.AddModel<IEditChannelPermissionsPayload, EditChannelPermissionsPayload>();
                options.AddModel<IFollowAnnouncementChannelPayload, FollowAnnouncementChannelPayload>();
                options.AddModel<IForumAndMediaThreadMessage, ForumAndMediaThreadMessage>();
                options.AddModel<IGroupDMAddRecipientPayload, GroupDMAddRecipientPayload>();
                options.AddModel<IModifyGroupDMPayload, ModifyGroupDMPayload>();
                options.AddModel<IModifyGuildChannelPayload, ModifyGuildChannelPayload>();
                options.AddModel<IModifyThreadChannelPayload, ModifyThreadChannelPayload>();
                options.AddModel<IStartThreadFromMessagePayload, StartThreadFromMessagePayload>();
                options.AddModel<IStartThreadInForumOrMediaChannelPayload, StartThreadInForumOrMediaChannelPayload>();
                options.AddModel<IStartThreadWithoutMessagePayload, StartThreadWithoutMessagePayload>();
                options.AddModel<ICreateApplicationEmojiPayload, CreateApplicationEmojiPayload>();
                options.AddModel<ICreateGuildEmojiPayload, CreateGuildEmojiPayload>();
                options.AddModel<IModifyApplicationEmojiPayload, ModifyApplicationEmojiPayload>();
                options.AddModel<IModifyGuildEmojiPayload, ModifyGuildEmojiPayload>();
                options.AddModel<ICreateTestEntitlementPayload, CreateTestEntitlementPayload>();
                options.AddModel<IAddGuildMemberPayload, AddGuildMemberPayload>();
                options.AddModel<IBeginGuildPrunePayload, BeginGuildPrunePayload>();
                options.AddModel<IBulkGuildBanPayload, BulkGuildBanPayload>();
                options.AddModel<ICreateGuildChannelPayload, CreateGuildChannelPayload>();
                options.AddModel<ICreateGuildPayload, CreateGuildPayload>();
                options.AddModel<ICreateGuildRolePayload, CreateGuildRolePayload>();
                options.AddModel<IModifyCurrentMemberPayload, ModifyCurrentMemberPayload>();
                options.AddModel<IModifyCurrentUserVoiceStatePayload, ModifyCurrentUserVoiceStatePayload>();
                options.AddModel<IModifyGuildChannelPositionsPayload, ModifyGuildChannelPositionsPayload>();
                options.AddModel<IModifyGuildMemberPayload, ModifyGuildMemberPayload>();
                options.AddModel<IModifyGuildMfaLevelPayload, ModifyGuildMfaLevelPayload>();
                options.AddModel<IModifyGuildOnboardingPayload, ModifyGuildOnboardingPayload>();
                options.AddModel<IModifyGuildPayload, ModifyGuildPayload>();
                options.AddModel<IModifyGuildRolePayload, ModifyGuildRolePayload>();
                options.AddModel<IModifyGuildRolePositionsPayload, ModifyGuildRolePositionsPayload>();
                options.AddModel<IModifyGuildWelcomeScreenPayload, ModifyGuildWelcomeScreenPayload>();
                options.AddModel<IModifyUserVoiceStatePayload, ModifyUserVoiceStatePayload>();
                options.AddModel<ICreateGuildFromGuildTemplatePayload, CreateGuildFromGuildTemplatePayload>();
                options.AddModel<ICreateGuildTemplatePayload, CreateGuildTemplatePayload>();
                options.AddModel<IModifyGuildTemplatePayload, ModifyGuildTemplatePayload>();
                options.AddModel<ICreateFollowupMessagePayload, CreateFollowupMessagePayload>();
                options.AddModel<IEditFollowupMessagePayload, EditFollowupMessagePayload>();
                options.AddModel<IEditInteractionResponsePayload, EditInteractionResponsePayload>();
                options.AddModel<IBulkDeleteMessagesPayload, BulkDeleteMessagesPayload>();
                options.AddModel<ICreateMessagePayload, CreateMessagePayload>();
                options.AddModel<IEditMessagePayload, EditMessagePayload>();
                options.AddModel<ICreateGuildScheduledEventPayload, CreateGuildScheduledEventPayload>();
                options.AddModel<IModifyGuildScheduledEventPayload, ModifyGuildScheduledEventPayload>();
                options.AddModel<ICreateStageInstancePayload, CreateStageInstancePayload>();
                options.AddModel<IModifyStageInstancePayload, ModifyStageInstancePayload>();
                options.AddModel<ICreateGuildStickerPayload, CreateGuildStickerPayload>();
                options.AddModel<IModifyGuildStickerPayload, ModifyGuildStickerPayload>();
                options.AddModel<ICreateDmPayload, CreateDmPayload>();
                options.AddModel<ICreateGroupDmPayload, CreateGroupDmPayload>();
                options.AddModel<IModifyCurrentUserPayload, ModifyCurrentUserPayload>();
                options.AddModel<IUpdateCurrentUserApplicationRoleConnectionPayload, UpdateCurrentUserApplicationRoleConnectionPayload>();
                options.AddModel<ICreateWebhookPayload, CreateWebhookPayload>();
                options.AddModel<IEditWebhookMessagePayload, EditWebhookMessagePayload>();
                options.AddModel<IExecuteWebhookPayload, ExecuteWebhookPayload>();
                options.AddModel<IModifyWebhookPayload, ModifyWebhookPayload>();
                options.AddModel<IModifyWebhookWithTokenPayload, ModifyWebhookWithTokenPayload>();
            }
        );
    }
}