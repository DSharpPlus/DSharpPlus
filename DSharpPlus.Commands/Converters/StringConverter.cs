using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using DSharpPlus.Commands.ArgumentModifiers;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Entities;

namespace DSharpPlus.Commands.Converters;

public class StringConverter : ISlashArgumentConverter<string>, ITextArgumentConverter<string>
{
    public DiscordApplicationCommandOptionType ParameterType => DiscordApplicationCommandOptionType.String;
    public ConverterInputType RequiresText => ConverterInputType.Always;
    public string ReadableName => "Text";

    public Task<Optional<string>> ConvertAsync(ConverterContext context)
    {
        string argument = context.Argument?.ToString() ?? "";
        foreach (Attribute attribute in context.Parameter.Attributes)
        {
            if (attribute is RemainingTextAttribute && context is TextConverterContext textConverterContext)
            {
                return Task.FromResult(Optional.FromValue(textConverterContext.RawArguments[textConverterContext.CurrentArgumentIndex..].TrimStart()));
            }
            else if (attribute is FromCodeAttribute codeAttribute)
            {
                return TryGetCodeBlock(argument, codeAttribute.CodeType, out string? code)
                    ? Task.FromResult(Optional.FromValue(code))
                    : Task.FromResult(Optional.FromNoValue<string>());
            }
        }

        return Task.FromResult(Optional.FromValue(argument));
    }

    private static bool TryGetCodeBlock(string input, CodeType expectedCodeType, [NotNullWhen(true)] out string? code)
    {
        code = null;
        ReadOnlySpan<char> inputSpan = input.AsSpan();
        if (inputSpan.Length > 6 && inputSpan.StartsWith("```") && inputSpan.EndsWith("```") && expectedCodeType.HasFlag(CodeType.Codeblock))
        {
            int index = inputSpan.IndexOf('\n');
            if (index == -1 || !FromCodeAttribute.CodeBlockLanguages.Contains(inputSpan[3..index].ToString()))
            {
                code = input[3..^3];
                return true;
            }

            code = input[(index + 1)..^3];
            return true;
        }
        else if (inputSpan.Length > 4 && inputSpan.StartsWith("``") && inputSpan.EndsWith("``") && expectedCodeType.HasFlag(CodeType.Inline))
        {
            code = input[2..^2];
        }
        else if (inputSpan.Length > 2 && inputSpan.StartsWith("`") && inputSpan.EndsWith("`") && expectedCodeType.HasFlag(CodeType.Inline))
        {
            code = input[1..^1];
        }

        return code is not null;
    }
}
