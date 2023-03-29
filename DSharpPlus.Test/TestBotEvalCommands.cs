// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2023 DSharpPlus Contributors
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
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace DSharpPlus.Test;

public class TestBotEvalCommands : BaseCommandModule
{
    [Command("eval"), Aliases("evalcs", "cseval", "roslyn"), Description("Evaluates C# code."), Hidden, RequireOwner]
    public async Task EvalCSAsync(CommandContext ctx, [RemainingText] string code)
    {
        DiscordMessage msg = ctx.Message;

        string cs = string.Empty;
        if (code.Trim().StartsWith("```"))
        {
            int cs1 = code.IndexOf("```") + 3;
            cs1 = code.IndexOf('\n', cs1) + 1;
            int cs2 = code.LastIndexOf("```");

            if (cs1 == -1 || cs2 == -1)
            {
                throw new ArgumentException("You need to wrap the code into a code block.");
            }

            cs = code.Substring(cs1, cs2 - cs1);
        }
        else
        {
            cs = code;
        }

        msg = await ctx.RespondAsync(embed: new DiscordEmbedBuilder()
            .WithColor(new DiscordColor("#FF007F"))
            .WithDescription("Evaluating...")
            .Build()).ConfigureAwait(false);

        try
        {
            TestVariables globals = new(ctx.Message, ctx.Client, ctx);

            ScriptOptions sopts = ScriptOptions.Default;
            sopts = sopts.WithImports("System", "System.Collections.Generic", "System.Linq", "System.Text", "System.Threading.Tasks", "DSharpPlus", "DSharpPlus.CommandsNext", "DSharpPlus.Interactivity", "Microsoft.Extensions.Logging");
            sopts = sopts.WithReferences(AppDomain.CurrentDomain.GetAssemblies().Where(xa => !xa.IsDynamic && !string.IsNullOrWhiteSpace(xa.Location)));

            Script<object> script = CSharpScript.Create(cs, sopts, typeof(TestVariables));
            script.Compile();
            ScriptState<object> result = await script.RunAsync(globals).ConfigureAwait(false);

            if (result != null && result.ReturnValue != null && !string.IsNullOrWhiteSpace(result.ReturnValue.ToString()))
            {
                await msg.ModifyAsync(embed: new DiscordEmbedBuilder { Title = "Evaluation Result", Description = result.ReturnValue.ToString(), Color = new DiscordColor("#007FFF") }.Build()).ConfigureAwait(false);
            }
            else
            {
                await msg.ModifyAsync(embed: new DiscordEmbedBuilder { Title = "Evaluation Successful", Description = "No result was returned.", Color = new DiscordColor("#007FFF") }.Build()).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            await msg.ModifyAsync(embed: new DiscordEmbedBuilder { Title = "Evaluation Failure", Description = string.Concat("**", ex.GetType().ToString(), "**: ", ex.Message), Color = new DiscordColor("#FF0000") }.Build()).ConfigureAwait(false);
        }
    }
}

public class TestVariables
{
    public DiscordMessage Message { get; set; }
    public DiscordChannel Channel { get; set; }
    public DiscordGuild Guild { get; set; }
    public DiscordUser User { get; set; }
    public DiscordMember Member { get; set; }
    public CommandContext Context { get; set; }

    public TestVariables(DiscordMessage msg, DiscordClient client, CommandContext ctx)
    {
        this.Client = client;

        this.Message = msg;
        this.Channel = msg.Channel;
        this.Guild = this.Channel.Guild;
        this.User = this.Message.Author;
        if (this.Guild != null)
        {
            this.Member = this.Guild.GetMemberAsync(this.User.Id).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        this.Context = ctx;
    }

    public DiscordClient Client;
}
