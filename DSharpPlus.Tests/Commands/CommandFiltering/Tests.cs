using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.MessageCommands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Commands.Processors.UserCommands;
using DSharpPlus.Commands.Trees;
using NUnit.Framework;

namespace DSharpPlus.Tests.Commands.CommandFiltering;

public class Tests
{
    private static readonly SlashCommandProcessor slashCommandProcessor = new(new()
    {
        RegisterCommands = false
    });

    private static CommandsExtension extension = null!;
    private static TextCommandProcessor textCommandProcessor = null!;
    private static UserCommandProcessor userCommandProcessor = null!;
    private static MessageCommandProcessor messageCommandProcessor = null!;

    [OneTimeSetUp]
    public static async Task CreateExtensionAsync()
    {
        DiscordClient client = DiscordClientBuilder.CreateDefault("faketoken", DiscordIntents.None).Build();

        extension = client.UseCommands();
        extension.AddProcessor(slashCommandProcessor);
        extension.AddCommands([typeof(TestMultiLevelSubCommandsFiltered.RootCommand), typeof(TestMultiLevelSubCommandsFiltered.ContextMenues), typeof(TestMultiLevelSubCommandsFiltered.ContextMenuesInGroup)]);
        extension.BuildCommands();

        // Prepare the extension and processors
        await extension.RefreshAsync();
        textCommandProcessor = extension.GetProcessor<TextCommandProcessor>();
        userCommandProcessor = extension.GetProcessor<UserCommandProcessor>();
        messageCommandProcessor = extension.GetProcessor<MessageCommandProcessor>();
    }

    [Test]
    public static void TestSubGroupTextProcessor()
    {
        IReadOnlyList<Command> commands = extension.GetCommandsForProcessor(textCommandProcessor);

        Command? root = commands.FirstOrDefault(x => x.Name == "root");
        Assert.That(root, Is.Not.Null);

        Assert.That(root.Subcommands, Has.Count.EqualTo(2));
        Assert.That(root.Subcommands[0].Name, Is.EqualTo("subgroup"));
        Assert.That(root.Subcommands[1].Name, Is.EqualTo("subgroup-text-only"));

        Command generalGroup = root.Subcommands[0];
        Assert.That(generalGroup.Subcommands, Has.Count.EqualTo(2));
        Assert.That(generalGroup.Subcommands[0].Name, Is.EqualTo("command-text-only-attribute"));
        Assert.That(generalGroup.Subcommands[1].Name, Is.EqualTo("command-text-only-parameter"));

        Command textGroup = root.Subcommands[1];
        Assert.That(textGroup.Subcommands, Has.Count.EqualTo(2));
        Assert.That(textGroup.Subcommands[0].Name, Is.EqualTo("text-only-group"));
        Assert.That(textGroup.Subcommands[1].Name, Is.EqualTo("text-only-group2"));
    }

    [Test]
    public static void TestSubGroupSlashProcessor()
    {
        IReadOnlyList<Command> commands = extension.GetCommandsForProcessor(slashCommandProcessor);

        //toplevel command "root"
        Command? root = commands.FirstOrDefault(x => x.Name == "root");
        Assert.That(root, Is.Not.Null);

        Assert.That(root.Subcommands, Has.Count.EqualTo(2));
        Assert.That(root.Subcommands[0].Name, Is.EqualTo("subgroup"));
        Assert.That(root.Subcommands[1].Name, Is.EqualTo("subgroup-slash-only"));

        Command generalGroup = root.Subcommands[0];
        Command slashGroup = root.Subcommands[1];

        Assert.That(generalGroup.Subcommands, Has.Count.EqualTo(2));
        Assert.That(generalGroup.Subcommands[0].Name, Is.EqualTo("command-slash-only-attribute"));
        Assert.That(generalGroup.Subcommands[1].Name, Is.EqualTo("command-slash-only-parameter"));

        Assert.That(slashGroup.Subcommands, Has.Count.EqualTo(2));
        Assert.That(slashGroup.Subcommands[0].Name, Is.EqualTo("slash-only-group"));
        Assert.That(slashGroup.Subcommands[1].Name, Is.EqualTo("slash-only-group2"));
    }

    [Test]
    public static void TestUserContextMenu()
    {
        IReadOnlyList<Command> userContextCommands = userCommandProcessor.Commands;

        Command? contextOnlyCommand = userContextCommands.FirstOrDefault(x => x.Name == "UserContextOnly");
        Assert.That(contextOnlyCommand, Is.Not.Null);

        Command? bothCommand = userContextCommands.FirstOrDefault(x => x.Name == "SlashUserContext");
        Assert.That(bothCommand, Is.Not.Null);

        IReadOnlyList<Command> slashCommands = extension.GetCommandsForProcessor(slashCommandProcessor);
        Assert.That(slashCommands.FirstOrDefault(x => x.Name == "SlashUserContext"), Is.Not.Null);
    }

    [Test]
    public static void TestMessageContextMenu()
    {
        IReadOnlyList<Command> messageContextCommands = messageCommandProcessor.Commands;

        Command? contextOnlyCommand = messageContextCommands.FirstOrDefault(x => x.Name == "MessageContextOnly");
        Assert.That(contextOnlyCommand, Is.Not.Null);

        Command? bothCommand = messageContextCommands.FirstOrDefault(x => x.Name == "SlashMessageContext");
        Assert.That(bothCommand, Is.Not.Null);

        IReadOnlyList<Command> slashCommands = extension.GetCommandsForProcessor(slashCommandProcessor);
        Assert.That(slashCommands.FirstOrDefault(x => x.Name == "SlashMessageContext"), Is.Not.Null);
    }

    [Test]
    public static void TestUserContextMenuInGroup()
    {
        IReadOnlyList<Command> userContextCommands = userCommandProcessor.Commands;

        Command? contextOnlyCommand = userContextCommands.FirstOrDefault(x => x.FullName == "group UserContextOnly");
        Assert.That(contextOnlyCommand, Is.Not.Null);

        Command? bothCommand = userContextCommands.FirstOrDefault(x => x.FullName == "group SlashUserContext");
        Assert.That(bothCommand, Is.Not.Null);

        IReadOnlyList<Command> slashCommands = extension.GetCommandsForProcessor(slashCommandProcessor);
        Command? group = slashCommands.FirstOrDefault(x => x.Name == "group");
        Assert.That(group, Is.Not.Null);
        Assert.That(group.Subcommands.Any(x => x.Name == "SlashUserContext"));

    }

    [Test]
    public static void TestMessageContextMenuInGroup()
    {
        IReadOnlyList<Command> messageContextCommands = messageCommandProcessor.Commands;

        Command? contextOnlyCommand = messageContextCommands.FirstOrDefault(x => x.FullName == "group MessageContextOnly");
        Assert.That(contextOnlyCommand, Is.Not.Null);

        Command? bothCommand = messageContextCommands.FirstOrDefault(x => x.FullName == "group SlashMessageContext");
        Assert.That(bothCommand, Is.Not.Null);

        IReadOnlyList<Command> slashCommands = extension.GetCommandsForProcessor(slashCommandProcessor);
        Command? group = slashCommands.FirstOrDefault(x => x.Name == "group");
        Assert.That(group, Is.Not.Null);
        Assert.That(group.Subcommands.Any(x => x.Name == "SlashMessageContext"));
    }

    [OneTimeTearDown]
    public static void DisposeExtension() => extension.Dispose();
}
