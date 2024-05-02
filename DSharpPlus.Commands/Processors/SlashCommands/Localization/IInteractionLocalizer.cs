using System.Collections.Generic;
using System.Threading.Tasks;

namespace DSharpPlus.Commands.Processors.SlashCommands.Localization;

public interface IInteractionLocalizer
{
    public ValueTask<IReadOnlyDictionary<DiscordLocale, string>> TranslateAsync(string fullSymbolName);
}
