using System;
using System.Linq;
using System.Text;

namespace DSharpPlus.Commands.Processors.TextCommands.Parsing;

public delegate string? TextArgumentSplicer(CommandsExtension extension, string text, ref int startAt);

public class DefaultTextArgumentSplicer
{
    private enum TextState
    {
        None,
        InBacktick,
        InTripleBacktick,
        InQuote
    }

    public static string? Splice(CommandsExtension extension, string text, ref int startAt)
    {
        // We do this for no parameter overloads such as HelloWorldAsync(CommandContext context)
        if (string.IsNullOrWhiteSpace(text) || startAt >= text.Length)
        {
            return null;
        }

        int i;
        char quotedCharacter = default;
        TextState state = TextState.None;
        StringBuilder result = new();
        ReadOnlySpan<char> textSpan = text.AsSpan();
        char[] quoteCharacters = extension.GetProcessor<TextCommandProcessor>().Configuration.QuoteCharacters;
        for (i = startAt; i < textSpan.Length; i++)
        {
            char character = textSpan[i];
            if (state == TextState.None)
            {
                if (IsEscaped(textSpan, i))
                {
                    result.Append(textSpan[++i]);
                    continue;
                }
                else if (char.IsWhiteSpace(character))
                {
                    // Skip beginning whitespace
                    if (result.Length == 0)
                    {
                        continue;
                    }

                    // End of argument
                    break;
                }
                else if (IsQuoted(textSpan, i, quoteCharacters))
                {
                    state = TextState.InQuote;
                    quotedCharacter = character;
                    continue;
                }
                else if (character == '`')
                {
                    if (IsTripleBacktick(textSpan, i))
                    {
                        i += 2;
                        result.Append("```");
                        state = TextState.InTripleBacktick;
                        continue;
                    }

                    state = TextState.InBacktick;
                }
            }
            else if (state == TextState.InTripleBacktick && IsTripleBacktick(textSpan, i))
            {
                i += 3;
                result.Append("```");
                break;
            }
            else if (state == TextState.InBacktick && character == '`')
            {
                state = TextState.None;
            }
            else if (state == TextState.InQuote)
            {
                if (IsEscaped(textSpan, i))
                {
                    result.Append(textSpan[++i]);
                    continue;
                }
                else if (character == quotedCharacter)
                {
                    state = TextState.None;
                    i++;
                    break;
                }
            }

            result.Append(character);
        }

        if (state == TextState.InQuote)
        {
            // Prepend the quoted character
            result.Insert(0, quotedCharacter);
        }

        if (result.Length == 0)
        {
            return null;
        }

        startAt = i;
        return result.ToString();
    }

    private static bool IsTripleBacktick(ReadOnlySpan<char> text, int index) => index + 2 < text.Length && text[index] == '`' && text[index + 1] == '`' && text[index + 2] == '`';
    private static bool IsEscaped(ReadOnlySpan<char> text, int index) => index + 1 < text.Length && text[index] == '\\';
    private static bool IsQuoted(ReadOnlySpan<char> text, int index, char[] quoteCharacters) => quoteCharacters.Contains(text[index]) && (index == 0 || char.IsWhiteSpace(text[index - 1]));
}
