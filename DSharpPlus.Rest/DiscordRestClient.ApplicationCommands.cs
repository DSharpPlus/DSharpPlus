// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2023 DSharpPlus Contributors
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.Net.Models;

namespace DSharpPlus;

public sealed partial class DiscordRestClient
{
    /// <summary>
    /// Gets all the global application commands for this application.
    /// </summary>
    /// <returns>A list of global application commands.</returns>
    public Task<IReadOnlyList<DiscordApplicationCommand>> GetGlobalApplicationCommandsAsync()
        => ApiClient.GetGlobalApplicationCommandsAsync(CurrentApplication.Id);

    /// <summary>
    /// Overwrites the existing global application commands. New commands are automatically created and missing commands are automatically deleted.
    /// </summary>
    /// <param name="commands">The list of commands to overwrite with.</param>
    /// <returns>The list of global commands.</returns>
    public Task<IReadOnlyList<DiscordApplicationCommand>> BulkOverwriteGlobalApplicationCommandsAsync(IEnumerable<DiscordApplicationCommand> commands)
        => ApiClient.BulkOverwriteGlobalApplicationCommandsAsync(CurrentApplication.Id, commands);

    /// <summary>
    /// Creates or overwrites a global application command.
    /// </summary>
    /// <param name="command">The command to create.</param>
    /// <returns>The created command.</returns>
    public Task<DiscordApplicationCommand> CreateGlobalApplicationCommandAsync(DiscordApplicationCommand command)
        => ApiClient.CreateGlobalApplicationCommandAsync(CurrentApplication.Id, command);

    /// <summary>
    /// Gets a global application command by its ID.
    /// </summary>
    /// <param name="commandId">The ID of the command to get.</param>
    /// <returns>The command with the ID.</returns>
    public Task<DiscordApplicationCommand> GetGlobalApplicationCommandAsync(ulong commandId)
        => ApiClient.GetGlobalApplicationCommandAsync(CurrentApplication.Id, commandId);

    /// <summary>
    /// Edits a global application command.
    /// </summary>
    /// <param name="commandId">The ID of the command to edit.</param>
    /// <param name="action">Action to perform.</param>
    /// <returns>The edited command.</returns>
    public Task<DiscordApplicationCommand> EditGlobalApplicationCommandAsync(ulong commandId, Action<ApplicationCommandEditModel> action)
    {
        ApplicationCommandEditModel applicationCommandEditModel = new();
        action(applicationCommandEditModel);
        return ApiClient.EditGlobalApplicationCommandAsync(
            CurrentApplication.Id,
            commandId,
            applicationCommandEditModel.Name,
            applicationCommandEditModel.Description,
            applicationCommandEditModel.Options,
            applicationCommandEditModel.DefaultPermission,
            default,
            default,
            applicationCommandEditModel.AllowDMUsage,
            applicationCommandEditModel.DefaultMemberPermissions
        );
    }

    /// <summary>
    /// Deletes a global application command.
    /// </summary>
    /// <param name="commandId">The ID of the command to delete.</param>
    public Task DeleteGlobalApplicationCommandAsync(ulong commandId)
        => ApiClient.DeleteGlobalApplicationCommandAsync(CurrentApplication.Id, commandId);

    /// <summary>
    /// Gets all the application commands for a guild.
    /// </summary>
    /// <param name="guildId">The ID of the guild to get application commands for.</param>
    /// <returns>A list of application commands in the guild.</returns>
    public Task<IReadOnlyList<DiscordApplicationCommand>> GetGuildApplicationCommandsAsync(ulong guildId)
        => ApiClient.GetGuildApplicationCommandsAsync(CurrentApplication.Id, guildId);

    /// <summary>
    /// Overwrites the existing application commands in a guild. New commands are automatically created and missing commands are automatically deleted.
    /// </summary>
    /// <param name="guildId">The ID of the guild.</param>
    /// <param name="commands">The list of commands to overwrite with.</param>
    /// <returns>The list of guild commands.</returns>
    public Task<IReadOnlyList<DiscordApplicationCommand>> BulkOverwriteGuildApplicationCommandsAsync(ulong guildId, IEnumerable<DiscordApplicationCommand> commands)
        => ApiClient.BulkOverwriteGuildApplicationCommandsAsync(CurrentApplication.Id, guildId, commands);

    /// <summary>
    /// Creates or overwrites a guild application command.
    /// </summary>
    /// <param name="guildId">The ID of the guild to create the application command in.</param>
    /// <param name="command">The command to create.</param>
    /// <returns>The created command.</returns>
    public Task<DiscordApplicationCommand> CreateGuildApplicationCommandAsync(ulong guildId, DiscordApplicationCommand command)
        => ApiClient.CreateGuildApplicationCommandAsync(CurrentApplication.Id, guildId, command);

    /// <summary>
    /// Gets a application command in a guild by its ID.
    /// </summary>
    /// <param name="guildId">The ID of the guild the application command is in.</param>
    /// <param name="commandId">The ID of the command to get.</param>
    /// <returns>The command with the ID.</returns>
    public Task<DiscordApplicationCommand> GetGuildApplicationCommandAsync(ulong guildId, ulong commandId)
        => ApiClient.GetGuildApplicationCommandAsync(CurrentApplication.Id, guildId, commandId);

    /// <summary>
    /// Edits a application command in a guild.
    /// </summary>
    /// <param name="guildId">The ID of the guild the application command is in.</param>
    /// <param name="commandId">The ID of the command to edit.</param>
    /// <param name="action">Action to perform.</param>
    /// <returns>The edited command.</returns>
    public Task<DiscordApplicationCommand> EditGuildApplicationCommandAsync(ulong guildId, ulong commandId, Action<ApplicationCommandEditModel> action)
    {
        ApplicationCommandEditModel applicationCommandEditModel = new();
        action(applicationCommandEditModel);
        return ApiClient.EditGuildApplicationCommandAsync(
            CurrentApplication.Id,
            guildId,
            commandId,
            applicationCommandEditModel.Name,
            applicationCommandEditModel.Description,
            applicationCommandEditModel.Options,
            applicationCommandEditModel.DefaultPermission,
            default,
            default,
            applicationCommandEditModel.AllowDMUsage,
            applicationCommandEditModel.DefaultMemberPermissions
        );
    }

    /// <summary>
    /// Deletes a application command in a guild.
    /// </summary>
    /// <param name="guildId">The ID of the guild to delete the application command in.</param>
    /// <param name="commandId">The ID of the command.</param>
    public Task DeleteGuildApplicationCommandAsync(ulong guildId, ulong commandId)
        => ApiClient.DeleteGuildApplicationCommandAsync(CurrentApplication.Id, guildId, commandId);

    /// <summary>
    /// Creates a response to an interaction.
    /// </summary>
    /// <param name="interactionId">The ID of the interaction.</param>
    /// <param name="interactionToken">The token of the interaction</param>
    /// <param name="type">The type of the response.</param>
    /// <param name="builder">The data, if any, to send.</param>
    public Task CreateInteractionResponseAsync(ulong interactionId, string interactionToken, InteractionResponseType type, DiscordInteractionResponseBuilder? builder = null)
        => ApiClient.CreateInteractionResponseAsync(interactionId, interactionToken, type, builder);

    /// <summary>
    /// Gets the original interaction response.
    /// </summary>
    /// <returns>The original message that was sent. This <b>does not work on ephemeral messages.</b></returns>
    public Task<DiscordMessage> GetOriginalInteractionResponseAsync(string interactionToken)
        => ApiClient.GetOriginalInteractionResponseAsync(CurrentApplication.Id, interactionToken);

    /// <summary>
    /// Edits the original interaction response.
    /// </summary>
    /// <param name="interactionToken">The token of the interaction.</param>
    /// <param name="builder">The webhook builder.</param>
    /// <param name="attachments">Attached files to keep.</param>
    /// <returns>The <see cref="DiscordMessage"/> edited.</returns>
    public Task<DiscordMessage> EditOriginalInteractionResponseAsync(string interactionToken, DiscordWebhookBuilder builder, IEnumerable<DiscordAttachment>? attachments = default)
    {
        builder.Validate(isInteractionResponse: true);
        return ApiClient.EditOriginalInteractionResponseAsync(CurrentApplication.Id, interactionToken, builder, attachments);
    }

    /// <summary>
    /// Deletes the original interaction response.
    /// <param name="interactionToken">The token of the interaction.</param>
    /// </summary>
    public Task DeleteOriginalInteractionResponseAsync(string interactionToken)
        => ApiClient.DeleteOriginalInteractionResponseAsync(CurrentApplication.Id, interactionToken);

    /// <summary>
    /// Creates a follow up message to an interaction.
    /// </summary>
    /// <param name="interactionToken">The token of the interaction.</param>
    /// <param name="builder">The webhook builder.</param>
    /// <returns>The <see cref="DiscordMessage"/> created.</returns>
    public Task<DiscordMessage> CreateFollowupMessageAsync(string interactionToken, DiscordFollowupMessageBuilder builder)
    {
        builder.Validate();
        return ApiClient.CreateFollowupMessageAsync(CurrentApplication.Id, interactionToken, builder);
    }

    /// <summary>
    /// Edits a follow up message.
    /// </summary>
    /// <param name="interactionToken">The token of the interaction.</param>
    /// <param name="messageId">The ID of the follow up message.</param>
    /// <param name="builder">The webhook builder.</param>
    /// <param name="attachments">Attached files to keep.</param>
    /// <returns>The <see cref="DiscordMessage"/> edited.</returns>
    public Task<DiscordMessage> EditFollowupMessageAsync(string interactionToken, ulong messageId, DiscordWebhookBuilder builder, IEnumerable<DiscordAttachment>? attachments = default)
    {
        builder.Validate(isFollowup: true);
        return ApiClient.EditFollowupMessageAsync(CurrentApplication.Id, interactionToken, messageId, builder, attachments);
    }

    /// <summary>
    /// Deletes a follow up message.
    /// </summary>
    /// <param name="interactionToken">The token of the interaction.</param>
    /// <param name="messageId">The ID of the follow up message.</param>
    public Task DeleteFollowupMessageAsync(string interactionToken, ulong messageId)
        => ApiClient.DeleteFollowupMessageAsync(CurrentApplication.Id, interactionToken, messageId);

    /// <summary>
    /// Gets all application command permissions in a guild.
    /// </summary>
    /// <param name="guildId">The guild ID.</param>
    /// <returns>A list of permissions.</returns>
    public Task<IReadOnlyList<DiscordGuildApplicationCommandPermissions>> GetGuildApplicationCommandsPermissionsAsync(ulong guildId)
        => ApiClient.GetGuildApplicationCommandPermissionsAsync(CurrentApplication.Id, guildId);

    /// <summary>
    /// Gets permissions for a application command in a guild.
    /// </summary>
    /// <param name="guildId">The guild ID.</param>
    /// <param name="commandId">The ID of the command to get them for.</param>
    /// <returns>The permissions.</returns>
    public Task<DiscordGuildApplicationCommandPermissions> GetGuildApplicationCommandPermissionsAsync(ulong guildId, ulong commandId)
        => ApiClient.GetApplicationCommandPermissionsAsync(CurrentApplication.Id, guildId, commandId);

    /// <summary>
    /// Edits permissions for a application command in a guild.
    /// </summary>
    /// <param name="guildId">The guild ID.</param>
    /// <param name="commandId">The ID of the command to edit permissions for.</param>
    /// <param name="permissions">The list of permissions to use.</param>
    /// <returns>The edited permissions.</returns>
    public Task<DiscordGuildApplicationCommandPermissions> EditApplicationCommandPermissionsAsync(ulong guildId, ulong commandId, IEnumerable<DiscordApplicationCommandPermission> permissions)
        => ApiClient.EditApplicationCommandPermissionsAsync(CurrentApplication.Id, guildId, commandId, permissions);

    /// <summary>
    /// Batch edits permissions for a application command in a guild.
    /// </summary>
    /// <param name="guildId">The guild ID.</param>
    /// <param name="permissions">The list of permissions to use.</param>
    /// <returns>A list of edited permissions.</returns>
    public Task<IReadOnlyList<DiscordGuildApplicationCommandPermissions>> BatchEditApplicationCommandPermissionsAsync(ulong guildId, IEnumerable<DiscordGuildApplicationCommandPermissions> permissions)
        => ApiClient.BatchEditApplicationCommandPermissionsAsync(CurrentApplication.Id, guildId, permissions);

    public Task<DiscordMessage> GetFollowupMessageAsync(string interactionToken, ulong messageId)
        => ApiClient.GetFollowupMessageAsync(CurrentApplication.Id, interactionToken, messageId);
}
