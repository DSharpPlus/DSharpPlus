// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2022 DSharpPlus Contributors
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
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

namespace DSharpPlus.Test;

public sealed class TestBotDynamicCommands : BaseCommandModule
{
    [Command("add"), Aliases("register"), Description("Dynamically registers a command from given source code."), Hidden, RequireOwner]
    public async Task AddCommandAsync(CommandContext ctx, string code)
    {
        DiscordMessage msg = ctx.Message;

        int cs1 = code.IndexOf("```") + 3;
        cs1 = code.IndexOf('\n', cs1) + 1;
        int cs2 = code.LastIndexOf("```");

        if (cs1 == -1 || cs2 == -1)
        {
            throw new ArgumentException("You need to wrap the code into a code block.");
        }

        string cs = code.Substring(cs1, cs2 - cs1);

        // I hate this
        cs = $"[ModuleLifespan(ModuleLifespan.Transient)]\npublic sealed class DynamicCommands : BaseCommandModule\n{{\n{cs}\n}}";

        msg = await ctx.RespondAsync(new DiscordEmbedBuilder()
            .WithColor(new DiscordColor("#FF007F"))
            .WithDescription("Compiling...")
            .Build()).ConfigureAwait(false);

        long number = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        string typeName = $"DynamicCommands{number}";
        Type moduleType = null;
        try
        {
            System.Collections.Generic.IEnumerable<PortableExecutableReference> references = AppDomain.CurrentDomain.GetAssemblies().Where(xa => !xa.IsDynamic && !string.IsNullOrWhiteSpace(xa.Location))
                .Select(x => MetadataReference.CreateFromFile(x.Location));

            SyntaxTree ast = SyntaxFactory.ParseSyntaxTree(cs, new CSharpParseOptions().WithKind(SourceCodeKind.Script).WithLanguageVersion(LanguageVersion.CSharp7_1));
            CSharpCompilationOptions copts = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, false, null, null, typeName,
                new[] { "System", "System.Collections.Generic", "System.Linq", "System.Text", "System.Threading.Tasks", "DSharpPlus", "DSharpPlus.Entities", "DSharpPlus.CommandsNext", "DSharpPlus.CommandsNext.Attributes", "DSharpPlus.Interactivity" },
                OptimizationLevel.Release, false, true, null, null, default, null, Platform.AnyCpu, ReportDiagnostic.Default, 4, null, true, false, null, null, null, null, null, false);

            CSharpCompilation csc = CSharpCompilation.CreateScriptCompilation($"DynamicCommands{number}", ast, references, copts, null, typeof(object), null);

            Assembly asm = null;
            using (MemoryStream ms = new MemoryStream())
            {
                Microsoft.CodeAnalysis.Emit.EmitResult er = csc.Emit(ms);
                ms.Position = 0;

                asm = Assembly.Load(ms.ToArray());
            }

            Type? outerType = asm.ExportedTypes.FirstOrDefault(x => x.Name == typeName);
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
    public async Task AddSimpleAsync(CommandContext ctx, string name, params string[] aliases)
    {
        await Task.Yield();
        CommandBuilder command = new CommandBuilder(null)
            .WithName(name)
            .WithDescription("Automatically-added command.")
            .WithExecutionChecks(new CooldownAttribute(1, 5, CooldownBucketType.Channel))
            .WithOverload(new CommandOverloadBuilder(new Func<CommandContext, Task>(Func0)).WithPriority(10))
            .WithOverload(new CommandOverloadBuilder(new Func<CommandContext, string, Task>(Func1)).WithPriority(0));

        if (aliases?.Any() == true)
        {
            command.WithAliases(aliases);
        }

        ctx.CommandsNext.RegisterCommands(command);
        await ctx.RespondAsync(DiscordEmoji.FromUnicode("👌").ToString()).ConfigureAwait(false);

        Task Func0(CommandContext c)
            => c.RespondAsync($"{c.Prefix} {c.Command.QualifiedName}");

        Task Func1(CommandContext c, string text)
            => c.RespondAsync($"{c.Prefix} {text}");
    }
}
