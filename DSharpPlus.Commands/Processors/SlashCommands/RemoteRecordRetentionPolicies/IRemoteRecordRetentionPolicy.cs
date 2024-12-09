using System.Threading.Tasks;

using DSharpPlus.Entities;

namespace DSharpPlus.Commands.Processors.SlashCommands.RemoteRecordRetentionPolicies;

/// <summary>
/// Provides a means to customize when and which application commands get deleted from your bot.
/// </summary>
public interface IRemoteRecordRetentionPolicy
{
    /// <summary>
    /// Returns a value indicating whether the application command should be deleted or not.
    /// </summary>
    public Task<bool> CheckDeletionStatusAsync(DiscordApplicationCommand command);
}
