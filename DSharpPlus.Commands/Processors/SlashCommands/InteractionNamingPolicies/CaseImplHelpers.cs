using System;
using System.Globalization;

using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;

namespace DSharpPlus.Commands.Processors.SlashCommands.InteractionNamingPolicies;

internal static class CaseImplHelpers
{
    public static void LowercaseCore(ReadOnlySpan<char> raw, ArrayPoolBufferWriter<char> result, CultureInfo culture)
    {
        for (int i = 0; i < raw.Length; i++)
        {
            char character = raw[i];
            result.Write(char.ToLower(character, culture));
        }
    }

    public static void KebabCaseCore(ReadOnlySpan<char> raw, ArrayPoolBufferWriter<char> result, CultureInfo culture)
    {
        for (int i = 0; i < raw.Length; i++)
        {
            char character = raw[i];

            // camelCase, PascalCase
            if (i != 0 && char.IsUpper(character) && result.WrittenSpan[^1] is not ('-' or '_'))
            {
                result.Write('-');
            }

            result.Write(char.ToLower(character, culture));
        }
    }

    public static void SnakeCaseCore(ReadOnlySpan<char> raw, ArrayPoolBufferWriter<char> result, CultureInfo culture)
    {
        for (int i = 0; i < raw.Length; i++)
        {
            char character = raw[i];

            // camelCase, PascalCase
            if (i != 0 && char.IsUpper(character) && result.WrittenSpan[^1] is not ('-' or '_'))
            {
                result.Write('_');
            }

            result.Write(char.ToLower(character, culture));
        }
    }
}
