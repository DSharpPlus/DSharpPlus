namespace DSharpPlus.CommandAll.Processors.TextCommands.Parsing;

using System;
using System.Collections.Generic;
using System.Linq;

public delegate string? TextArgumentSplicer(CommandAllExtension extension, string text, ref int startAt);

public class DefaultTextArgumentSplicer
{
    [Flags]
    private enum State
    {
        None = 0,
        Quoted = 1 << 0,
        InlineCode = 1 << 1,
        TripleCode = 1 << 2,
        Escape = 1 << 3
    }

    public static string? Splice(CommandAllExtension extension, string text, ref int startAt)
    {
        // We do this for no parameter overloads such as HelloWorldAsync(CommandContext context)
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        char[] quoteCharacters = extension.GetProcessor<TextCommandProcessor>().Configuration.QuoteCharacters;
        bool inBacktick = false;
        bool inTripleBacktick = false;
        bool inQuote = false;
        bool inEscape = false;
        List<int> removeIndices = new(text.Length - startAt);

        int i = startAt;
        for (; i < text.Length; i++)
        {
            if (!char.IsWhiteSpace(text[i]))
            {
                break;
            }
        }

        startAt = i;

        int endPosition = -1;
        int startPosition = startAt;
        for (i = startPosition; i < text.Length; i++)
        {
            if (char.IsWhiteSpace(text[i]) && !inQuote && !inTripleBacktick && !inBacktick && !inEscape)
            {
                endPosition = i;
            }

            if (text[i] == '\\' && text.Length > i + 1)
            {
                if (!inEscape && !inBacktick && !inTripleBacktick)
                {
                    inEscape = true;
                    if (text.IndexOf("\\`", i) == i || quoteCharacters.Any(c => text.IndexOf($"\\{c}", i) == i) || text.IndexOf("\\\\", i) == i || (text.Length >= i && char.IsWhiteSpace(text[i + 1])))
                    {
                        removeIndices.Add(i - startPosition);
                    }

                    i++;
                }
                else if ((inBacktick || inTripleBacktick) && text.IndexOf("\\`", i) == i)
                {
                    inEscape = true;
                    removeIndices.Add(i - startPosition);
                    i++;
                }
            }

            if (text[i] == '`' && !inEscape)
            {
                bool tripleBacktick = text.IndexOf("```", i) == i;
                if (inTripleBacktick && tripleBacktick)
                {
                    inTripleBacktick = false;
                    i += 2;
                }
                else if (!inBacktick && tripleBacktick)
                {
                    inTripleBacktick = true;
                    i += 2;
                }

                if (inBacktick && !tripleBacktick)
                {
                    inBacktick = false;
                }
                else if (!inTripleBacktick && tripleBacktick)
                {
                    inBacktick = true;
                }
            }

            if (quoteCharacters.Contains(text[i]) && !inEscape && !inBacktick && !inTripleBacktick)
            {
                removeIndices.Add(i - startPosition);

                inQuote = !inQuote;
            }

            if (inEscape)
            {
                inEscape = false;
            }

            if (endPosition != -1)
            {
                startAt = endPosition;
                return startPosition != endPosition ? CleanupString(text[startPosition..endPosition], removeIndices) : null;
            }
        }

        startAt = text.Length;
        return startAt != startPosition ? CleanupString(text[startPosition..], removeIndices) : null;
    }

    private static string CleanupString(string s, IList<int> indices)
    {
        if (!indices.Any())
        {
            return s;
        }

        int li = indices.Last();
        int ll = 1;
        for (int x = indices.Count - 2; x >= 0; x--)
        {
            if (li - indices[x] == ll)
            {
                ll++;
                continue;
            }

            s = s.Remove(li - ll + 1, ll);
            li = indices[x];
            ll = 1;
        }

        return s.Remove(li - ll + 1, ll);
    }
}
