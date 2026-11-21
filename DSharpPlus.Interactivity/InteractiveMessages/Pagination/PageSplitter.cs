using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace DSharpPlus.Interactivity.InteractiveMessages.Pagination;

internal static class PageSplitter
{
    public static IReadOnlyList<Page> GeneratePages(string input, PageSplitType splitType = PageSplitType.CharacterWise)
    {
        if (string.IsNullOrEmpty(input))
        {
            throw new ArgumentException("You must provide a string that is not null or empty!");
        }

        List<Page> result = [];
        List<string> split = [];

        switch (splitType)
        {
            case PageSplitType.CharacterWise:

                split = SplitStringToSentences(input, 500);
                break;

            case PageSplitType.LineWise:

                StringBuilder builder = new();
                int lines = 0;

                foreach (ReadOnlySpan<char> line in input.EnumerateLines())
                {
                    builder.Append(line);
                    builder.AppendLine();
                    lines++;

                    if (lines == 15)
                    {
                        lines = 0;
                        split.Add(builder.ToString().Trim());
                        builder.Clear();
                    }
                }
                
                if (lines != 0)
                {
                    split.Add(builder.ToString().Trim());
                }

                break;
        }

        int page = 0;

        foreach (string s in split)
        {
            result.Add(new Page(null, s, page, split.Count));
            page++;
        }

        return result;
    }

    private static List<string> SplitStringToSentences(string value, int characterCount)
    {
        List<string> slices = [];
        ReadOnlySpan<char> toSplit = value.AsSpan();

        while (toSplit.Length > characterCount * 1.2)
        {
            StringBuilder stringBuilder = new();

            stringBuilder.Append(toSplit[..characterCount]);
            int i;

            for (i = characterCount; i < toSplit.Length; i++)
            {
                char character = toSplit[i];

                if (char.IsLetterOrDigit(character))
                {
                    stringBuilder.Append(character);
                    continue;
                }

                if (i > (characterCount * 1.2) && char.GetUnicodeCategory(character) == UnicodeCategory.SpaceSeparator)
                {
                    break;
                }

                switch (char.GetUnicodeCategory(character))
                {
                    case UnicodeCategory.LineSeparator:
                    case UnicodeCategory.ParagraphSeparator:

                        i++;
                        goto endOfLoop;

                    case UnicodeCategory.OtherPunctuation:

                        if 
                        (
                            toSplit.Length >= i + 1 
                            && char.GetUnicodeCategory(toSplit[i + 1]) is UnicodeCategory.SpaceSeparator 
                                or UnicodeCategory.LineSeparator 
                                or UnicodeCategory.ParagraphSeparator
                        )
                        {
                            stringBuilder.Append(character);
                            i++;
                            goto endOfLoop;
                        }

                        stringBuilder.Append(character);
                        continue;
                }

                stringBuilder.Append(character);
            }
        
        endOfLoop:

            slices.Add(stringBuilder.ToString().Trim());
            toSplit = toSplit[i..];
        }

        slices.Add(toSplit.ToString().Trim());

        return slices;
    }
}
