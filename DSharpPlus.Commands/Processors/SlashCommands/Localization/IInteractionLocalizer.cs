namespace DSharpPlus.Commands.Processors.SlashCommands.Localization;

using System.Collections.Generic;
using System.Threading.Tasks;

public interface IInteractionLocalizer
{
    public ValueTask<IReadOnlyDictionary<DiscordLocale, string>> TranslateAsync(string fullSymbolName);
}
