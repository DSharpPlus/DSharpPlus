using System;
using DSharpPlus.Commands.Trees;
using DSharpPlus.Entities;
using DSharpPlus.Tests.Commands.Cases.Commands;
using NUnit.Framework;

namespace DSharpPlus.Tests.Commands.Trees;

public class CommandBuilderTests
{
    [Test]
    public void TopLevelEmptyCommand() => Assert.Throws<ArgumentException>(() => CommandBuilder.From<TestSingleLevelSubCommands.EmptyCommand>());

    [Test]
    public void TopLevelCommandMissingContext() => Assert.Throws<ArgumentException>(() => CommandBuilder.From(TestTopLevelCommands.OopsAsync));

    [Test]
    public void TopLevelCommandNoParameters()
    {
        CommandBuilder commandBuilder = CommandBuilder.From(TestTopLevelCommands.PingAsync);
        Command command = commandBuilder.Build();
        Assert.Multiple(() =>
        {
            Assert.That(command.Name, Is.EqualTo("ping"));
            Assert.That(command.Description, Is.Null);
            Assert.That(command.Parent, Is.Null);
            Assert.That(command.Target, Is.Null);
            Assert.That(command.Method, Is.EqualTo(((Delegate)TestTopLevelCommands.PingAsync).Method));
            Assert.That(command.Attributes, Is.Not.Empty);
            Assert.That(command.Subcommands, Is.Empty);
            Assert.That(command.Parameters, Is.Empty);
        });
    }

    [Test]
    public void TopLevelCommandOneOptionalParameter()
    {
        CommandBuilder commandBuilder = CommandBuilder.From(TestTopLevelCommands.UserInfoAsync);
        Command command = commandBuilder.Build();
        Assert.That(command.Parameters, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(command.Parameters[0].Name, Is.EqualTo("user"));
            Assert.That(command.Parameters[0].Description, Is.EqualTo("No description provided."));
            Assert.That(command.Parameters[0].Type, Is.EqualTo(typeof(DiscordUser)));
            Assert.That(command.Parameters[0].DefaultValue.HasValue, Is.True);
            Assert.That(command.Parameters[0].DefaultValue.Value, Is.Null);
        });
    }

    [Test]
    public void SingleLevelSubCommands()
    {
        CommandBuilder commandBuilder = CommandBuilder.From<TestSingleLevelSubCommands.TagCommand>();
        Assert.That(commandBuilder.Subcommands, Has.Count.EqualTo(2));

        Command command = commandBuilder.Build();
        Assert.Multiple(() =>
        {
            Assert.That(command.Parent, Is.Null);
            Assert.That(command.Subcommands, Has.Count.EqualTo(2));
        });

        // Will not execute if the subcommand count fails
        Assert.Multiple(() =>
        {
            Assert.That(command.Subcommands[0].Name, Is.EqualTo("add"));
            Assert.That(command.Subcommands[0].Parameters, Has.Count.EqualTo(2));
            Assert.That(command.Subcommands[1].Name, Is.EqualTo("get"));
            Assert.That(command.Subcommands[1].Parameters, Has.Count.EqualTo(1));
        });
    }

    [Test]
    public void MultiLevelSubCommands()
    {
        CommandBuilder commandBuilder = CommandBuilder.From<TestMultiLevelSubCommands.InfoCommand>();
        Assert.That(commandBuilder.Subcommands, Has.Count.EqualTo(2));

        Command command = commandBuilder.Build();
        Assert.Multiple(() =>
        {
            Assert.That(command.Parent, Is.Null);
            Assert.That(command.Subcommands, Has.Count.EqualTo(2));
        });

        Assert.Multiple(() =>
        {
            Assert.That(command.Subcommands[0].Name, Is.EqualTo("user"));
            Assert.That(command.Subcommands[0].Parent, Is.EqualTo(command));
            Assert.That(command.Subcommands[1].Name, Is.EqualTo("channel"));
            Assert.That(command.Subcommands[1].Parent, Is.EqualTo(command));
        });

        Assert.That(command.Subcommands[0].Subcommands, Has.Count.EqualTo(3));
        Assert.Multiple(() =>
        {
            Assert.That(command.Subcommands[0].Subcommands[0].Parameters, Has.Count.EqualTo(1));
            Assert.That(command.Subcommands[0].Subcommands[1].Parameters, Has.Count.EqualTo(1));
            Assert.That(command.Subcommands[0].Subcommands[2].Parameters, Has.Count.EqualTo(2));
        });

        Assert.That(command.Subcommands[1].Subcommands, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(command.Subcommands[1].Subcommands[0].Parameters, Has.Count.EqualTo(1));
            Assert.That(command.Subcommands[1].Subcommands[1].Parameters, Has.Count.EqualTo(1));
        });
    }
}
