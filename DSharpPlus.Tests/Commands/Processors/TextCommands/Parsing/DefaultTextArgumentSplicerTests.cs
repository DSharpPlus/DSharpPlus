using System.Collections.Generic;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Commands.Processors.TextCommands.Parsing;
using DSharpPlus.Tests.Commands.Cases;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace DSharpPlus.Tests.Commands.Processors.TextCommands.Parsing;

public sealed class DefaultTextArgumentSplicerTests
{
    private static CommandsExtension extension = null!;

    [OneTimeSetUp]
    public static void CreateExtension()
    {
        DiscordClientBuilder builder = DiscordClientBuilder.CreateDefault(
            "faketoken",
            DiscordIntents.None
        );

        builder.UseCommands(
            (_, extension) => extension.AddProcessor(new TextCommandProcessor()),
            new() { RegisterDefaultCommandProcessors = false }
        );

        DiscordClient client = builder.Build();
        extension = client.ServiceProvider.GetRequiredService<CommandsExtension>();
    }

    [TestCaseSource(typeof(UserInput), nameof(UserInput.ExpectedNormal), null)]
    public static void ParseNormalArguments(string input, string[] expectedArguments)
    {
        List<string> arguments = [];
        int position = 0;
        while (true)
        {
            string? argument = DefaultTextArgumentSplicer.Splice(extension, input, ref position);
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
            string? argument = DefaultTextArgumentSplicer.Splice(extension, input, ref position);
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
            string? argument = DefaultTextArgumentSplicer.Splice(extension, input, ref position);
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
            string? argument = DefaultTextArgumentSplicer.Splice(extension, input, ref position);
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
            string? argument = DefaultTextArgumentSplicer.Splice(extension, input, ref position);
            if (argument is null)
            {
                break;
            }

            arguments.Add(argument);
        }

        Assert.That(arguments, Has.Count.EqualTo(expectedArguments.Length));
        Assert.That(arguments, Is.EqualTo(expectedArguments));
    }
}
