namespace DSharpPlus.Tests.Commands.Processors.TextCommands.Parsing;

using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Commands.Processors.TextCommands.Parsing;
using DSharpPlus.Tests.Commands.Cases;
using NUnit.Framework;

public sealed class DefaultTextArgumentSplicerTests
{
    private static CommandsExtension _extension = null!;

    [OneTimeSetUp]
    public static async Task CreateExtensionAsync()
    {
        DiscordClient client = new(new DiscordConfiguration()
        {
            Token = "FakeToken",
        });

        _extension = client.UseCommands();
        await _extension.AddProcessorAsync(new TextCommandProcessor());
    }

    [TestCaseSource(typeof(UserInput), nameof(UserInput.ExpectedNormal), null)]
    public static void ParseNormalArguments(string input, string[] expectedArguments)
    {
        List<string> arguments = [];
        int position = 0;
        while (true)
        {
            string? argument = DefaultTextArgumentSplicer.Splice(_extension, input, ref position);
            if (argument is null)
            {
                break;
            }

            arguments.Add(argument);
        }

        Assert.That(arguments, Has.Count.EqualTo(expectedArguments.Length));
        Assert.That(arguments, Is.EqualTo(expectedArguments));
    }

    [TestCaseSource(typeof(UserInput), nameof(UserInput.ExpectedQuoted), null)]
    public static void ParseQuotedArguments(string input, string[] expectedArguments)
    {
        List<string> arguments = [];
        int position = 0;
        while (true)
        {
            string? argument = DefaultTextArgumentSplicer.Splice(_extension, input, ref position);
            if (argument is null)
            {
                break;
            }

            arguments.Add(argument);
        }

        Assert.That(arguments, Has.Count.EqualTo(expectedArguments.Length));
        Assert.That(arguments, Is.EqualTo(expectedArguments));
    }

    [TestCaseSource(typeof(UserInput), nameof(UserInput.ExpectedInlineCode), null)]
    public static void ParseInlineCodeArguments(string input, string[] expectedArguments)
    {
        List<string> arguments = [];
        int position = 0;
        while (true)
        {
            string? argument = DefaultTextArgumentSplicer.Splice(_extension, input, ref position);
            if (argument is null)
            {
                break;
            }

            arguments.Add(argument);
        }

        Assert.That(arguments, Has.Count.EqualTo(expectedArguments.Length));
        Assert.That(arguments, Is.EqualTo(expectedArguments));
    }

    [TestCaseSource(typeof(UserInput), nameof(UserInput.ExpectedCodeBlock), null)]
    public static void ParseCodeBlockArguments(string input, string[] expectedArguments)
    {
        List<string> arguments = [];
        int position = 0;
        while (true)
        {
            string? argument = DefaultTextArgumentSplicer.Splice(_extension, input, ref position);
            if (argument is null)
            {
                break;
            }

            arguments.Add(argument);
        }

        Assert.That(arguments, Has.Count.EqualTo(expectedArguments.Length));
        Assert.That(arguments, Is.EqualTo(expectedArguments));
    }

    [TestCaseSource(typeof(UserInput), nameof(UserInput.ExpectedEscaped), null)]
    public static void ParseEscapedArguments(string input, string[] expectedArguments)
    {
        List<string> arguments = [];
        int position = 0;
        while (true)
        {
            string? argument = DefaultTextArgumentSplicer.Splice(_extension, input, ref position);
            if (argument is null)
            {
                break;
            }

            arguments.Add(argument);
        }

        Assert.That(arguments, Has.Count.EqualTo(expectedArguments.Length));
        Assert.That(arguments, Is.EqualTo(expectedArguments));
    }

    [OneTimeTearDown]
    public static void DisposeExtension() => _extension.Dispose();
}
