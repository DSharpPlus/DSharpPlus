using System;
using System.Globalization;
using System.Threading.Tasks;
using DSharpPlus.CommandAll.Processors.SlashCommands;
using DSharpPlus.CommandAll.Processors.TextCommands;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace DSharpPlus.CommandAll.Converters
{
    public class EnumConverter : ISlashArgumentConverter<Enum>, ITextArgumentConverter<Enum>
    {
        public ApplicationCommandOptionType ParameterType { get; init; } = ApplicationCommandOptionType.Integer;
        public bool RequiresText { get; init; } = true;

        public Task<Optional<Enum>> ConvertAsync(ConverterContext context, MessageCreateEventArgs eventArgs)
        {
            string value = context.As<TextConverterContext>().Argument;
            return Task.FromResult(Enum.TryParse(context.Parameter.Type, value, true, out object? result)
                ? Optional.FromValue((Enum)result)
                : Optional.FromNoValue<Enum>());
        }

        public Task<Optional<Enum>> ConvertAsync(ConverterContext context, InteractionCreateEventArgs eventArgs)
        {
            SlashConverterContext slashConverterContext = context.As<SlashConverterContext>();
            object value = Convert.ChangeType(slashConverterContext.Argument.Value, Enum.GetUnderlyingType(context.Parameter.Type), CultureInfo.InvariantCulture);
            return Enum.IsDefined(context.Parameter.Type, value)
                ? Task.FromResult(Optional.FromValue((Enum)Enum.ToObject(context.Parameter.Type, value)))
                : Task.FromResult(Optional.FromNoValue<Enum>());
        }
    }
}
