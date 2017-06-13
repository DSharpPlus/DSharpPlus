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
    [Group("eval"), Aliases("exec", "os", "env"), Hidden, Description("Provides evaluation and OS commands."), SimpleCanTest]
    public class TestBotEvalCommands
    {
        [Command("csharp"), Aliases("eval", "evalcs", "cseval", "csharp", "roslyn"), Description("Evaluates C# code.")]
        public async Task EvalCS(CommandContext ctx, params string[] code_input)
        {
            var msg = ctx.Message;
            var code = string.Join(" ", code_input);

            var cs1 = code.IndexOf("```") + 3;
            cs1 = code.IndexOf('\n', cs1) + 1;
            var cs2 = code.LastIndexOf("```");

            if (cs1 == -1 || cs2 == -1)
                throw new ArgumentException("You need to wrap the code into a code block.");

            var cs = code.Substring(cs1, cs2 - cs1);

            msg = await ctx.RespondAsync("", embed: new DiscordEmbed
            {
                Color = 0xFF007F,
                Description = "Evaluating..."
            });

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
                    await msg.EditAsync(embed: new DiscordEmbed { Title = "Evaluation Result", Description = result.ReturnValue.ToString(), Color = 0x007FFF });
                else
                    await msg.EditAsync(embed: new DiscordEmbed { Title = "Evaluation Successful", Description = "No result was returned.", Color = 0x007FFF });
            }
            catch (Exception ex)
            {
                await msg.EditAsync(embed: new DiscordEmbed { Title = "Evaluation Failure", Description = string.Concat("**", ex.GetType().ToString(), "**: ", ex.Message), Color = 0xFF0000 });
            }
        }

        [Command("exec"), Aliases("shell", "cmd", "system"), Description("Executes a shell command.")]
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

                var embed = new DiscordEmbed
                {
                    Title = "Execution finished",
                    Fields = new List<DiscordEmbedField>()
                };

                if (!string.IsNullOrWhiteSpace(o1))
                    embed.Fields.Add(new DiscordEmbedField
                    {
                        Inline = false,
                        Name = "Output",
                        Value = $"```\n{o1}\n```"
                    });

                if (!string.IsNullOrWhiteSpace(o2))
                    embed.Fields.Add(new DiscordEmbedField
                    {
                        Inline = false,
                        Name = "Error",
                        Value = $"```\n{o2}\n```"
                    });

                await ctx.RespondAsync("", embed: embed);
            }
            catch (Exception ex)
            {
                await ctx.RespondAsync("", embed: new DiscordEmbed { Title = "Execution failed", Description = $"`{ex.GetType()}: {ex.Message}`", Color = 0xFF0000 });
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
