using System;
using System.Buffers;
using System.Globalization;
using System.Text;

using CommunityToolkit.HighPerformance.Buffers;

using DSharpPlus.Commands.Trees;

namespace DSharpPlus.Commands.Processors.SlashCommands.InteractionNamingPolicies;

/// <summary>
/// Transforms parameter names into lowercase.
/// </summary>
public sealed class LowercaseNamingPolicy : IInteractionNamingPolicy
{
    /// <summary>
    /// Transforms the parameter name into it's lowercase equivalent.
    /// </summary>
    /// <inheritdoc />
    public string GetParameterName(CommandParameter parameter, CultureInfo culture, int arrayIndex)
    {
        if (string.IsNullOrWhiteSpace(parameter.Name))
        {
            throw new InvalidOperationException("Parameter name cannot be null or empty.");
        }

        StringBuilder stringBuilder = new(TransformText(parameter.Name, culture));
        if (arrayIndex > -1)
        {
            stringBuilder.Append(arrayIndex.ToString(culture));
        }

        return stringBuilder.ToString();
    }

    /// <inheritdoc />
    public string TransformText(ReadOnlySpan<char> text, CultureInfo culture)
    {
        ArrayPoolBufferWriter<char> writer = new(32);

        CaseImplHelpers.LowercaseCore(text, writer, culture);
        ((IMemoryOwner<char>)writer).Memory.Span.Replace('-', '_');

        return new string(writer.WrittenSpan);
    }
}
