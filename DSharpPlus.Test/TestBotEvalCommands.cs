using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace DSharpPlus.Test
{
    public class TestBotEvalCommands : BaseCommandModule
    {
        [Command("eval"), Aliases("evalcs", "cseval", "roslyn"), Description("Evaluates C# code."), Hidden, RequireOwner]
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
                .Build()).ConfigureAwait(false);

            try
            {
                var globals = new TestVariables(ctx.Message, ctx.Client, ctx);

                var sopts = ScriptOptions.Default;
                sopts = sopts.WithImports("System", "System.Collections.Generic", "System.Linq", "System.Text", "System.Threading.Tasks", "DSharpPlus", "DSharpPlus.CommandsNext", "DSharpPlus.Interactivity", "Microsoft.Extensions.Logging");
                sopts = sopts.WithReferences(AppDomain.CurrentDomain.GetAssemblies().Where(xa => !xa.IsDynamic && !string.IsNullOrWhiteSpace(xa.Location)));

                var script = CSharpScript.Create(cs, sopts, typeof(TestVariables));
                script.Compile();
                var result = await script.RunAsync(globals).ConfigureAwait(false);

                if (result != null && result.ReturnValue != null && !string.IsNullOrWhiteSpace(result.ReturnValue.ToString()))
                    await msg.ModifyAsync(embed: new DiscordEmbedBuilder { Title = "Evaluation Result", Description = result.ReturnValue.ToString(), Color = new DiscordColor("#007FFF") }.Build()).ConfigureAwait(false);
                else
                    await msg.ModifyAsync(embed: new DiscordEmbedBuilder { Title = "Evaluation Successful", Description = "No result was returned.", Color = new DiscordColor("#007FFF") }.Build()).ConfigureAwait(false);
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
                this.Member = this.Guild.GetMemberAsync(this.User.Id).ConfigureAwait(false).GetAwaiter().GetResult();
            this.Context = ctx;
        }

        public DiscordClient Client;
    }
}
