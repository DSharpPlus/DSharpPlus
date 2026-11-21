using System.Collections.Generic;
using System.Linq;

using DSharpPlus.Interactivity.InteractiveMessages.Pagination;

using NUnit.Framework;

namespace DSharpPlus.Tests.Interactivity;

public class PageSplitterTests
{
    [TestCaseSource(typeof(PageSplitterTestData), nameof(PageSplitterTestData.lineWiseSplitting), null)]
    public static void SplitByLine(string input, string[] expectedArguments)
    {
        IReadOnlyList<Page> rawPages = PageSplitter.GeneratePages(input, PageSplitType.LineWise);
        string[] pages = [..rawPages.Select(x => x.Content)];

        Assert.That(pages, Has.Length.EqualTo(expectedArguments.Length));
        Assert.That(pages, Is.EqualTo(expectedArguments).IgnoreLineEndingFormat);
    }

    [TestCaseSource(typeof(PageSplitterTestData), nameof(PageSplitterTestData.characterWiseSplitting), null)]
    public static void SplitByLength(string input, string[] expectedArguments)
    {
        IReadOnlyList<Page> rawPages = PageSplitter.GeneratePages(input, PageSplitType.CharacterWise);
        string[] pages = [..rawPages.Select(x => x.Content)];

        Assert.That(pages, Has.Length.EqualTo(expectedArguments.Length));
        Assert.That(pages, Is.EqualTo(expectedArguments).IgnoreLineEndingFormat);
    }
}
