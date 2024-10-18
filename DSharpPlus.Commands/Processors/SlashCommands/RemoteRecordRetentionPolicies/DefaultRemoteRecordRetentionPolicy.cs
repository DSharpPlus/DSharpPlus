using System.Threading.Tasks;

using DSharpPlus.Entities;

namespace DSharpPlus.Commands.Processors.SlashCommands.RemoteRecordRetentionPolicies;

internal sealed class DefaultRemoteRecordRetentionPolicy : IRemoteRecordRetentionPolicy
{
    public Task<bool> CheckDeletionStatusAsync(DiscordApplicationCommand command)
        => Task.FromResult(command.Type != DiscordApplicationCommandType.ActivityEntryPoint);
}
