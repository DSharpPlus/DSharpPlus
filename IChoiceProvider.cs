using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace DSharpPlus.SlashCommands
{
    public interface IChoiceProvider
    {
        Task<IEnumerable<DiscordApplicationCommandOptionChoice>> Provider();
    }
}