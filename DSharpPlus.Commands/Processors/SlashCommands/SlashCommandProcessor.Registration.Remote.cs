using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using DSharpPlus.Entities;
using DSharpPlus.Net.Models;

namespace DSharpPlus.Commands.Processors.SlashCommands;

public sealed partial class SlashCommandProcessor
{
    private async Task<IReadOnlyList<DiscordApplicationCommand>> VerifyAndUpdateRemoteCommandsAsync
    (
        IReadOnlyList<DiscordApplicationCommand> local,
        IReadOnlyList<DiscordApplicationCommand> remoteCommands
    )
    {
        int added = 0, edited = 0, unchanged = 0, deleted = 0;
        List<DiscordApplicationCommand> updated = [];
        List<DiscordApplicationCommand> remoteTracking = new(remoteCommands);

        foreach (DiscordApplicationCommand command in local)
        {
            DiscordApplicationCommand remote;

            if ((remote = remoteTracking.SingleOrDefault(x => x.Name == command.Name)) is not null)
            {
                if (command.WeakEquals(remote))
                {
                    unchanged++;
                    updated.Add(command);
                    remoteTracking.Remove(remote);
                    continue;
                }
                else
                {
                    edited++;
                    updated.Add(await ModifyGlobalCommandAsync(remote.Id, command));
                    remoteTracking.Remove(remote);
                    continue;
                }
            }
            else
            {
                added++;
                updated.Add(await CreateGlobalCommandAsync(command));
            }
        }

        deleted = remoteTracking.Count;

        foreach (DiscordApplicationCommand toDelete in remoteTracking)
        {
            if (await this.Configuration.RemoteRecordRetentionPolicy.CheckDeletionStatusAsync(toDelete))
            {
                await DeleteGlobalCommandAsync(toDelete);
            }
        }

        if (added != 0 || edited != 0 || deleted != 0)
        {
            SlashLogging.detectedCommandRecordChanges(this.logger, unchanged, added, edited, deleted, null);
        }
        else
        {
            SlashLogging.commandRecordsUnchanged(this.logger, null);
        }

        return updated;
    }

    private async ValueTask<DiscordApplicationCommand> CreateGlobalCommandAsync(DiscordApplicationCommand command)
    {
        return this.extension.DebugGuildId == 0
            ? await this.extension.Client.CreateGlobalApplicationCommandAsync(command)
            : await this.extension.Client.CreateGuildApplicationCommandAsync(this.extension.DebugGuildId, command);
    }

#pragma warning disable IDE0046
    private async ValueTask<DiscordApplicationCommand> ModifyGlobalCommandAsync(ulong id, DiscordApplicationCommand command)
    {
        if (this.extension.DebugGuildId == 0)
        {
            return await this.extension.Client.EditGlobalApplicationCommandAsync(id, x => CopyToEditModel(command, x));
        }
        else
        {
            return await this.extension.Client.EditGuildApplicationCommandAsync
            (
                this.extension.DebugGuildId,
                id, 
                x => CopyToEditModel(command, x)
            );
        }
    }
#pragma warning restore IDE0046

    private async ValueTask DeleteGlobalCommandAsync(DiscordApplicationCommand command)
    {
        if (this.extension.DebugGuildId == 0)
        {
            await this.extension.Client.DeleteGlobalApplicationCommandAsync(command.Id);
        }
        else
        {
            await this.extension.Client.DeleteGuildApplicationCommandAsync(this.extension.DebugGuildId, command.Id);
        }
    }

    private static void CopyToEditModel(DiscordApplicationCommand command, ApplicationCommandEditModel editModel)
    {
        editModel.AllowDMUsage = command.AllowDMUsage.HasValue
                    ? new(command.AllowDMUsage.Value)
                    : Optional.FromNoValue<bool>();
        editModel.DefaultMemberPermissions = command.DefaultMemberPermissions;
        editModel.Description = command.Description;
        editModel.NameLocalizations = command.NameLocalizations;
        editModel.DescriptionLocalizations = command.DescriptionLocalizations;
        editModel.IntegrationTypes = command.IntegrationTypes is not null
            ? new(command.IntegrationTypes)
            : Optional.FromNoValue<IEnumerable<DiscordApplicationIntegrationType>>();
        editModel.AllowedContexts = command.Contexts is not null
            ? new(command.Contexts)
            : Optional.FromNoValue<IEnumerable<DiscordInteractionContextType>>();
        editModel.NSFW = command.NSFW;
        editModel.Options = command.Options is not null
            ? new(command.Options)
            : Optional.FromNoValue<IReadOnlyList<DiscordApplicationCommandOption>>();
    }
}
