using System;
using System.Threading.Tasks;
using DSharpPlus.CommandAll.Processors.SlashCommands;
using DSharpPlus.CommandAll.Processors.TextCommands;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace DSharpPlus.CommandAll.Converters
{
    public class EnumConverter : ISlashArgumentConverter<Enum>, ITextArgumentConverter<Enum>
    {
        public ApplicationCommandOptionType ArgumentType { get; init; } = ApplicationCommandOptionType.Integer;
        public bool RequiresText { get; init; } = true;

        public Task<Optional<Enum>> ConvertAsync(ConverterContext context, MessageCreateEventArgs eventArgs)
        {
            string value = context.As<TextConverterContext>().CurrentTextArgument;
            return Enum.IsDefined(context.Argument.Type, value)
                ? Task.FromResult(Optional.FromValue((Enum)Enum.ToObject(context.Argument.Type, value)))
                : Task.FromResult(Optional.FromNoValue<Enum>());
        }

        public Task<Optional<Enum>> ConvertAsync(ConverterContext context, InteractionCreateEventArgs eventArgs)
        {
            int value = (int)context.As<SlashConverterContext>().CurrentOption.Value;
            return Enum.IsDefined(context.Argument.Type, value)
                ? Task.FromResult(Optional.FromValue((Enum)Enum.ToObject(context.Argument.Type, value)))
                : Task.FromResult(Optional.FromNoValue<Enum>());
        }
    }
}
