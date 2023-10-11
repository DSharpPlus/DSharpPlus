using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandAll.Converters.Meta;
using DSharpPlus.CommandAll.Processors.SlashCommands.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace DSharpPlus.CommandAll.Converters
{
    public class StringConverter
    {
        [Converter<string>, SlashConverter(ApplicationCommandOptionType.String)]
        public static Task<IOptional> ConvertAsync(ConverterContext context)
        {
            if (context.EventArgs is MessageCreateEventArgs messageCreateEventArgs)
            {
                return Task.FromResult<IOptional>(Optional.FromValue(messageCreateEventArgs.Message.Content));
            }
            else if (context.EventArgs is InteractionCreateEventArgs eventArgs
                && eventArgs.Interaction.Type == InteractionType.ApplicationCommand)
            {
                return Task.FromResult<IOptional>(Optional.FromValue(eventArgs.Interaction.Data.Options.ElementAt(context.Command.Arguments.IndexOf(context.Argument)).Value.ToString()!));
            }

            return Task.FromResult<IOptional>(Optional.FromNoValue<string>());
        }
    }
}
