using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace DSharpPlus.Test
{
    [Group("eval", CanInvokeWithoutSubcommand = true), Aliases("exec", "os", "env"), Hidden, Description("Provides evaluation and OS commands."), RequireOwner]
    public class TestBotEvalCommands
    {
        public Task ExecuteGroupAsync(CommandContext ctx, [RemainingText] string code) =>
            this.EvalCS(ctx, code);

        [Command("csharp"), Aliases("eval", "evalcs", "cseval", "csharp", "roslyn"), Description("Evaluates C# code."), RequireOwner]
        public async Task EvalCS(CommandContext ctx, [RemainingText] string code)
        {
            var msg = ctx.Message;

            var cs1 = code.IndexOf("```") + 3;
            cs1 = code.IndexOf('\n', cs1) + 1;
            var cs2 = code.LastIndexOf("```");

            if (cs1 == -1 || cs2 == -1)
                throw new ArgumentException("You need to wrap the code into a code block.");

            var cs = code.Substring(cs1, cs2 - cs1);

            msg = await ctx.RespondAsync("", embed: new DiscordEmbedBuilder()
                .WithColor(new DiscordColor("#FF007F"))
                .WithDescription("Evaluating...")
                .Build());

            try
            {
                var globals = new TestVariables(ctx.Message, ctx.Client, ctx);

                var sopts = ScriptOptions.Default;
                sopts = sopts.WithImports("System", "System.Collections.Generic", "System.Linq", "System.Text", "System.Threading.Tasks", "DSharpPlus", "DSharpPlus.CommandsNext", "DSharpPlus.Interactivity");
                sopts = sopts.WithReferences(AppDomain.CurrentDomain.GetAssemblies().Where(xa => !xa.IsDynamic && !string.IsNullOrWhiteSpace(xa.Location)));

                var script = CSharpScript.Create(cs, sopts, typeof(TestVariables));
                script.Compile();
                var result = await script.RunAsync(globals);

                if (result != null && result.ReturnValue != null && !string.IsNullOrWhiteSpace(result.ReturnValue.ToString()))
                    await msg.EditAsync(embed: new DiscordEmbedBuilder { Title = "Evaluation Result", Description = result.ReturnValue.ToString(), Color = new DiscordColor("#007FFF") }.Build());
                else
                    await msg.EditAsync(embed: new DiscordEmbedBuilder { Title = "Evaluation Successful", Description = "No result was returned.", Color = new DiscordColor("#007FFF") }.Build());
            }
            catch (Exception ex)
            {
                await msg.EditAsync(embed: new DiscordEmbedBuilder { Title = "Evaluation Failure", Description = string.Concat("**", ex.GetType().ToString(), "**: ", ex.Message), Color = new DiscordColor("#FF0000") }.Build());
            }
        }

        [Command("csharpold"), Description("Evaluates C# code."), RequireOwner]
        public Task EvalCSOld(CommandContext ctx, params string[] code_input)
        {
            var code = string.Join(" ", code_input);
            return this.EvalCS(ctx, code);
        }

        [Command("exec"), Aliases("shell", "cmd", "system"), Description("Executes a shell command."), RequireOwner]
        public async Task Exec(CommandContext ctx, string process, params string[] args)
        {
            try
            {
                var psi = new ProcessStartInfo(process, string.Join(" ", args))
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                var proc = Process.Start(psi);
                proc.WaitForExit();

                var o1 = await proc.StandardOutput.ReadToEndAsync();
                var o2 = await proc.StandardError.ReadToEndAsync();

                var embed = new DiscordEmbedBuilder
                {
                    Title = "Execution finished",
                };

                if (!string.IsNullOrWhiteSpace(o1))
                    embed.AddField("Output", $"```\n{o1}\n```", false);

                if (!string.IsNullOrWhiteSpace(o2))
                    embed.AddField("Error", $"```\n{o2}\n```", false);

                await ctx.RespondAsync("", embed: embed.Build());
            }
            catch (Exception ex)
            {
                await ctx.RespondAsync("", embed: new DiscordEmbedBuilder { Title = "Execution failed", Description = $"`{ex.GetType()}: {ex.Message}`", Color = new DiscordColor("#FF0000") }.Build());
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
            this.Member = this.Guild?.GetMemberAsync(this.User.Id).GetAwaiter().GetResult();
            this.Context = ctx;
        }

        public DiscordClient Client;
    }
}
