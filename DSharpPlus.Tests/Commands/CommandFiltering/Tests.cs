using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Commands.Trees;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace DSharpPlus.Tests.Commands.CommandFiltering;

public class Tests
{
    private static CommandsExtension _extension = null!;
    private static TextCommandProcessor textCommandProcessor = new();
    private static SlashCommandProcessor slashCommandProcessor = new();
    
    [OneTimeSetUp]
    public static async Task CreateExtensionAsync()
    {
        DiscordClient client = new(new DiscordConfiguration()
        {
            Token = "FakeToken",
        });

        _extension = client.UseCommands(new()
        {
            RegisterDefaultCommandProcessors = false,
            ServiceProvider = new ServiceCollection().BuildServiceProvider()
        });
        await _extension.AddProcessorAsync(textCommandProcessor);
        await _extension.AddProcessorAsync(slashCommandProcessor);
        
        _extension.AddCommands([typeof(TestMultiLevelSubCommandsFiltered.RootCommand)]);
        _extension.BuildCommands();
    }

    [Test]
    public static void TestSubGroupTextProcessor()
    {
        IReadOnlyList<Command> commands = _extension.GetCommandsForProcessor(textCommandProcessor);
        
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
        IReadOnlyList<Command> commands = _extension.GetCommandsForProcessor(slashCommandProcessor);
        
        //toplevel command "root"
        Assert.That(commands, Has.Count.EqualTo(1));
        Assert.That(commands[0].Name, Is.EqualTo("root"));
        Command root = commands[0];
        
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
    
    [OneTimeTearDown]
    public static void DisposeExtension() => _extension.Dispose();
}
