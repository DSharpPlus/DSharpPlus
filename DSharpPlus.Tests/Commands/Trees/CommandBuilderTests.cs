namespace DSharpPlus.Tests.Commands.Trees;

using System;
using System.Linq;
using DSharpPlus.Commands.Processors.TextCommands.Attributes;
using DSharpPlus.Commands.Trees;
using DSharpPlus.Entities;
using DSharpPlus.Tests.Commands.Cases;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class CommandBuilderTests
{
    [TestMethod]
    public void TopLevelEmptyCommand() => Assert.ThrowsException<ArgumentException>(CommandBuilder.From<TestSingleLevelSubCommands.EmptyCommand>);

    [TestMethod]
    public void TopLevelCommandMissingContext() => Assert.ThrowsException<ArgumentException>(() => CommandBuilder.From(TestTopLevelCommands.OopsAsync));

    [TestMethod]
    public void TopLevelCommandNoParameters()
    {
        CommandBuilder commandBuilder = CommandBuilder.From(TestTopLevelCommands.PingAsync);
        Command command = commandBuilder.Build();
        Assert.AreEqual("ping", command.Name);
        Assert.AreEqual(null, command.Description);
        Assert.IsNull(command.Parent);
        Assert.IsNull(command.Target);
        Assert.AreEqual(((Delegate)TestTopLevelCommands.PingAsync).Method, command.Method);
        Assert.AreEqual(0, command.Subcommands.Count);
        Assert.AreEqual(0, command.Parameters.Count);
    }

    [TestMethod]
    public void TopLevelCommandOneParameter()
    {
        CommandBuilder commandBuilder = CommandBuilder.From(TestTopLevelCommands.EchoAsync);
        Command command = commandBuilder.Build();
        Assert.AreEqual(1, command.Parameters.Count);
        Assert.AreEqual("message", command.Parameters[0].Name);
        Assert.AreEqual("No description provided.", command.Parameters[0].Description);
        Assert.AreEqual(typeof(string), command.Parameters[0].Type);
        Assert.AreEqual(false, command.Parameters[0].DefaultValue.HasValue);
        Assert.IsTrue(command.Parameters[0].Attributes.Count != 0);
        Assert.IsTrue(command.Parameters[0].Attributes.Any(attribute => attribute is RemainingTextAttribute));
    }

    [TestMethod]
    public void TopLevelCommandOneOptionalParameter()
    {
        CommandBuilder commandBuilder = CommandBuilder.From(TestTopLevelCommands.UserInfoAsync);
        Command command = commandBuilder.Build();
        Assert.AreEqual(1, command.Parameters.Count);
        Assert.AreEqual("user", command.Parameters[0].Name);
        Assert.AreEqual("No description provided.", command.Parameters[0].Description);
        Assert.AreEqual(typeof(DiscordUser), command.Parameters[0].Type);
        Assert.AreEqual(true, command.Parameters[0].DefaultValue.HasValue);
        Assert.AreEqual(null, command.Parameters[0].DefaultValue.Value);
    }

    [TestMethod]
    public void SingleLevelSubCommands()
    {
        CommandBuilder commandBuilder = CommandBuilder.From<TestSingleLevelSubCommands.TagCommand>();
        Assert.AreEqual(2, commandBuilder.Subcommands.Count);

        Command command = commandBuilder.Build();
        Assert.IsNull(command.Parent);
        Assert.AreEqual(2, command.Subcommands.Count);
        Assert.AreEqual("add", command.Subcommands[0].Name);
        Assert.AreEqual("get", command.Subcommands[1].Name);
        Assert.AreEqual(2, command.Subcommands[0].Parameters.Count);
        Assert.AreEqual(1, command.Subcommands[1].Parameters.Count);
    }

    [TestMethod]
    public void MultiLevelSubCommands()
    {
        CommandBuilder commandBuilder = CommandBuilder.From<TestMultiLevelSubCommands.InfoCommand>();
        Assert.AreEqual(2, commandBuilder.Subcommands.Count);

        Command command = commandBuilder.Build();
        Assert.IsNull(command.Parent);
        Assert.AreEqual(2, command.Subcommands.Count);
        Assert.AreEqual(command, command.Subcommands[0].Parent);
        Assert.AreEqual(command, command.Subcommands[1].Parent);
        Assert.AreEqual("user", command.Subcommands[0].Name);
        Assert.AreEqual("channel", command.Subcommands[1].Name);
        Assert.AreEqual(3, command.Subcommands[0].Subcommands.Count);
        Assert.AreEqual(2, command.Subcommands[1].Subcommands.Count);
        Assert.AreEqual(1, command.Subcommands[0].Subcommands[0].Parameters.Count);
        Assert.AreEqual(1, command.Subcommands[0].Subcommands[1].Parameters.Count);
        Assert.AreEqual(2, command.Subcommands[0].Subcommands[2].Parameters.Count);
        Assert.AreEqual(1, command.Subcommands[1].Subcommands[0].Parameters.Count);
        Assert.AreEqual(1, command.Subcommands[1].Subcommands[1].Parameters.Count);
    }
}
