using System;
using System.Text.RegularExpressions;

namespace DSharpPlus.CommandAll.Processors.TextCommands.Parsing
{
    public delegate int TextArgumentSplicer(CommandAllExtension extension, string text, int startAt, out ReadOnlySpan<char> argument);

    public partial class DefaultTextArgumentSplicer
    {
        [GeneratedRegex("""(?<!\\)((?:(["'«»‘“„‟]).*?[^\\]\2)|(```(?:.*?[\n]?)*?```)|(`.*`)|(\S+))""")]
        private static partial Regex _argumentMatcherRegex();

        public static int Splice(CommandAllExtension extension, string text, int startAt, out ReadOnlySpan<char> argument)
        {
            // We do this for no parameter overloads such as HelloWorldAsync(CommandContext context)
            if (string.IsNullOrWhiteSpace(text))
            {
                argument = [];
                return -1;
            }

            MatchCollection matches = _argumentMatcherRegex().Matches(text, startAt);
            if (matches.Count == 0)
            {
                argument = [];
                return -1;
            }

            ReadOnlySpan<char> _quoteCharacters = ['"', '\'', '«', '»', '‘', '“', '„', '‟'];
            Match match = matches[0];
            if (_quoteCharacters.Contains(match.ValueSpan[0]) && _quoteCharacters.Contains(match.ValueSpan[^1]) && match.ValueSpan[0] == match.ValueSpan[^1])
            {
                argument = match.ValueSpan[1..^1];
                return startAt + match.Length;
            }

            argument = match.ValueSpan;
            return startAt + match.Length;
        }
    }
}
