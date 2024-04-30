namespace DSharpPlus.Tests.Commands.Cases;
using System.Collections.Generic;
using NUnit.Framework;

public static class UserInput
{
    public static readonly List<TestCaseData> ExpectedNormal =
    [
        new TestCaseData("Hello", new[] { "Hello" }),
        new TestCaseData("Hello World", new[] { "Hello", "World" }),
        new TestCaseData("Hello World Hello World", new[] { "Hello", "World", "Hello", "World" }),
        new TestCaseData("Jeff Bezos has 121 BILLION dollars. The population of earth is 7 billion people. He could give every person 1 BILLION dollars and end poverty, and he would still have 114 billion dollars left over but he would do it. This is what capitalist greed looks like!", new[] { "Jeff", "Bezos", "has", "121", "BILLION", "dollars.", "The", "population", "of", "earth", "is", "7", "billion", "people.", "He", "could", "give", "every", "person", "1", "BILLION", "dollars", "and", "end", "poverty,", "and", "he", "would", "still", "have", "114", "billion", "dollars", "left", "over", "but", "he", "would", "do", "it.", "This", "is", "what", "capitalist", "greed", "looks", "like!" })
    ];

    public static readonly List<TestCaseData> ExpectedQuoted =
    [
        new TestCaseData("'Hello'", new[] { "Hello" }),
        new TestCaseData("'Hello World'", new[] { "Hello World" }),
        new TestCaseData("'Hello World' 'Hello World'", new[] { "Hello World", "Hello World" }),
        new TestCaseData("'Hello World' Hello World", new[] { "Hello World", "Hello", "World" }),
        new TestCaseData("\"Hello 'world'\"", new[] { "Hello 'world'" }),
        new TestCaseData("\"'Hello world'\"", new[] { "'Hello world'" }),
        new TestCaseData("I'm so sick of all these people who think they're gamers. No, you're not. Most of you are not even close to being gamers. I see these people saying \"I put well over 100hrs in this game and it's great!\" That's nothing, most of us can easily put 300+ in all of our games. I see people who only have the Nintendo switch and claim to be gamers. Come talk to me when you pick up a PS4 controller then we'll be friends.", new[] { "I'm", "so", "sick", "of", "all", "these", "people", "who", "think", "they're", "gamers.", "No,", "you're", "not.", "Most", "of", "you", "are", "not", "even", "close", "to", "being", "gamers.", "I", "see", "these", "people", "saying", "I put well over 100hrs in this game and it's great!", "That's", "nothing,", "most", "of", "us", "can", "easily", "put", "300+", "in", "all", "of", "our", "games.", "I", "see", "people", "who", "only", "have", "the", "Nintendo", "switch", "and", "claim", "to", "be", "gamers.", "Come", "talk", "to", "me", "when", "you", "pick", "up", "a", "PS4", "controller", "then", "we'll", "be", "friends." })
    ];

    public static readonly List<TestCaseData> ExpectedInlineCode =
    [
        new TestCaseData("`Hello`", new[] { "`Hello`" }),
        new TestCaseData("`Hello World`", new[] { "`Hello World`" }),
        new TestCaseData("`Hello World` `Hello World`", new[] { "`Hello World`", "`Hello World`" }),
        new TestCaseData("`Hello World` Hello World", new[] { "`Hello World`", "Hello", "World" }),
        new TestCaseData("`ɴᴏᴡ ᴘʟᴀʏɪɴɢ: Who asked (Feat: No one) ───────────⚪────── ◄◄⠀▐▐ ⠀►► 5:12/ 7:𝟻𝟼 ───○ 🔊⠀ ᴴᴰ ⚙️`", new[] { "`ɴᴏᴡ ᴘʟᴀʏɪɴɢ: Who asked (Feat: No one) ───────────⚪────── ◄◄⠀▐▐ ⠀►► 5:12/ 7:𝟻𝟼 ───○ 🔊⠀ ᴴᴰ ⚙️`" })
    ];

    public static readonly List<TestCaseData> ExpectedCodeBlock =
    [
        new TestCaseData("```Hello```", new[] { "```Hello```" }),
        new TestCaseData("```Hello World```", new[] { "```Hello World```" }),
        new TestCaseData("```\nHello world\n```", new[] { "```\nHello world\n```" }),
        new TestCaseData("```Hello``````Hello World```", new[] { "```Hello```", "```Hello World```" }),
    ];

    public static readonly List<TestCaseData> ExpectedEscaped =
    [
        new TestCaseData("Hello\\ World", new[] { "Hello World" }),
        new TestCaseData("'Hello\\' World'", new[] { "Hello' World" }),
        new TestCaseData("\\'Hello 'World'", new[] { "'Hello", "World" }),
    ];
}
