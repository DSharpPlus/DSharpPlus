﻿using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Builders;
using DSharpPlus.Entities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace DSharpPlus.Test
{
    public sealed class TestBotDynamicCommands : BaseCommandModule
    {
        [Command("add"), Aliases("register"), Description("Dynamically registers a command from given source code."), Hidden, RequireOwner]
        public async Task AddCommandAsync(CommandContext ctx, string code)
        {
            var msg = ctx.Message;

            var cs1 = code.IndexOf("```") + 3;
            cs1 = code.IndexOf('\n', cs1) + 1;
            var cs2 = code.LastIndexOf("```");

            if (cs1 == -1 || cs2 == -1)
                throw new ArgumentException("You need to wrap the code into a code block.");

            var cs = code.Substring(cs1, cs2 - cs1);

            // I hate this
            cs = $"[ModuleLifespan(ModuleLifespan.Transient)]\npublic sealed class DynamicCommands : BaseCommandModule\n{{\n{cs}\n}}";

            msg = await ctx.RespondAsync("", embed: new DiscordEmbedBuilder()
                .WithColor(new DiscordColor("#FF007F"))
                .WithDescription("Compiling...")
                .Build()).ConfigureAwait(false);

            var number = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var typeName = $"DynamicCommands{number}";
            Type moduleType = null;
            try
            {
                var references = AppDomain.CurrentDomain.GetAssemblies().Where(xa => !xa.IsDynamic && !string.IsNullOrWhiteSpace(xa.Location))
                    .Select(x => MetadataReference.CreateFromFile(x.Location));

                var ast = SyntaxFactory.ParseSyntaxTree(cs, new CSharpParseOptions().WithKind(SourceCodeKind.Script).WithLanguageVersion(LanguageVersion.CSharp7_1));
                var copts = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, false, null, null, typeName,
                    new[] { "System", "System.Collections.Generic", "System.Linq", "System.Text", "System.Threading.Tasks", "DSharpPlus", "DSharpPlus.Entities", "DSharpPlus.CommandsNext", "DSharpPlus.CommandsNext.Attributes", "DSharpPlus.Interactivity" },
                    OptimizationLevel.Release, false, true, null, null, default, null, Platform.AnyCpu, ReportDiagnostic.Default, 4, null, true, false, null, null, null, null, null, false);

                var csc = CSharpCompilation.CreateScriptCompilation($"DynamicCommands{number}", ast, references, copts, null, typeof(object), null);

                Assembly asm = null;
                using (var ms = new MemoryStream())
                {
                    var er = csc.Emit(ms);
                    ms.Position = 0;

                    asm = Assembly.Load(ms.ToArray());
                }

                var outerType = asm.ExportedTypes.FirstOrDefault(x => x.Name == typeName);
                moduleType = outerType.GetNestedTypes().FirstOrDefault(x => x.BaseType == typeof(BaseCommandModule));

                ctx.CommandsNext.RegisterCommands(moduleType);

                await msg.ModifyAsync(embed: new DiscordEmbedBuilder { Title = "Compilation Successful", Description = "Commands were registered.", Color = new DiscordColor("#007FFF") }.Build()).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await msg.ModifyAsync(embed: new DiscordEmbedBuilder { Title = "Compilation Failure", Description = string.Concat("**", ex.GetType().ToString(), "**: ", ex.Message), Color = new DiscordColor("#FF0000") }.Build()).ConfigureAwait(false);
            }
        }

        [Command("addsimple"), Description("Adds a simple echo command."), Hidden, RequireOwner]
        public async Task AddSimple(CommandContext ctx, string name, params string[] aliases)
        {
            var command = new CommandBuilder(null)
                .WithName(name)
                .WithDescription("Automatically-added command.")
                .WithExecutionChecks(new CooldownAttribute(1, 5, CooldownBucketType.Channel))
                .WithOverload(new CommandOverloadBuilder(new Func<CommandContext, Task>(Func0)).WithPriority(10))
                .WithOverload(new CommandOverloadBuilder(new Func<CommandContext, string, Task>(Func1)).WithPriority(0));

            if (aliases?.Any() == true)
                command.WithAliases(aliases);

            ctx.CommandsNext.RegisterCommands(command);

            Task Func0(CommandContext c)
                => c.RespondAsync($"{c.Prefix} {c.Command.QualifiedName}");

            Task Func1(CommandContext c, string text)
                => c.RespondAsync($"{c.Prefix} {text}");
        }
    }
}
