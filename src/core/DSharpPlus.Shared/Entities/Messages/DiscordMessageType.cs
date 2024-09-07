// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

#pragma warning disable CA1027 // this is not a flags enum.

namespace DSharpPlus.Entities;

public enum DiscordMessageType
{
    Default,
    RecipientAdd,
    RecipientRemove,
    Call,
    ChannelNameChange,
    ChannelIconChange,
    ChannelPinnedMessage,
    UserJoin,
    GuildBoost,
    GuildBoostTier1,
    GuildBoostTier2,
    GuildBoostTier3,
    ChannelFollowAdd,
    GuildDiscoveryDisqualified = 14,
    GuildDiscoveryRequalified,
    GuildDiscoveryGracePeriodInitialWarning,
    GuildDiscoveryGracePeriodFinalWarning,
    ThreadCreated,
    Reply,
    ChatInputCommand,
    ThreadStarterMessage,
    GuildInviteReminder,
    ContextMenuCommand,
    AutoModerationAction,
    RoleSubscriptionPurchase,
    InteractionPremiumUpsell,
    StageStart,
    StageEnd,
    StageSpeaker,
    StageTopic = 31,
    GuildApplicationPremiumSubscription,
    GuildIncidentAlertModeEnabled = 36,
    GuildIncidentAlertModeDisabled,
    GuildIncidentReportRaid,
    GuildIncidentReportFalseAlarm,
    PurchaseNotification = 44,
    PollResult = 46
}
