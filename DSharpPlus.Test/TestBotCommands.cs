using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using Microsoft.Extensions.DependencyInjection;

namespace DSharpPlus.Test
{
	public class TestBotCommands : BaseCommandModule
	{
		public static ConcurrentDictionary<ulong, string> PrefixSettings { get; } = new ConcurrentDictionary<ulong, string>();

		// disabled cause permissions'n'shit
		//[Command("testcreateow")]
		//public async Task TestCreateOwAsync(CommandContext ctx)
		//{
		//    List<DiscordOverwriteBuilder> dowbs = new List<DiscordOverwriteBuilder>()
		//        .Add(new DiscordOverwriteBuilder()
		//        .Allow(Permissions.ManageChannels)
		//        .Deny(Permissions.ManageMessages)
		//        .For(ctx.Member);

		//    await ctx.Guild.CreateTextChannelAsync("memes", overwrites: dowbs);
		//    await ctx.RespondAsync("naam check your shitcode");
		//}

		[Command("intext")]
		public async Task IntExtAsync(CommandContext ctx)
		{
			var mes = await ctx.Channel.WaitForMessageAsync(ctx.User, x => x == "ayy");

			if (mes != null)
				await mes.Message.RespondAsync("lmao");
		}

		[Command("pages")]
		public async Task PagesAsync(CommandContext ctx)
		{
			var i = ctx.Client.GetInteractivity();
            var pages = new List<Page>
            {
                new Page() { Content = "meme1" },
                new Page() { Content = "meme2" },
                new Page() { Content = "meme3" },
                new Page() { Content = "meme4" },
                new Page() { Content = "meme5" },
                new Page() { Content = "meme6" }
            };

            var emojis = new PaginationEmojis(ctx.Client)
            {
                Left = DiscordEmoji.FromName(ctx.Client, ":joy:")
            };

            await i.SendPaginatedMessage(ctx.Channel, ctx.User, pages, emojis: emojis);
		}

		[Command("embedcolor")]
		public async Task EmbedColorAsync(CommandContext ctx)
		{
			var e = new DiscordEmbedBuilder()
				.WithTitle("without color")
				.Build();

			var e2 = new DiscordEmbedBuilder()
				.WithTitle("with color")
				.WithColor(DiscordColor.PhthaloBlue)
				.Build();

			await ctx.RespondAsync(embed: e);
			await ctx.RespondAsync(embed: e2);
		}

		[Command("chekpin")]
		public async Task ChekPinsAsync(CommandContext ctx)
		{
			await ctx.Channel.GetPinnedMessagesAsync();
			await ctx.RespondAsync("u got mail!");
		}

		[Command("vsdb")]
		public async Task VStateDebugAsync(CommandContext ctx)
		{
			if (ctx.Member.VoiceState == null)
				await ctx.RespondAsync("voice state is null");
			else if (ctx.Member.VoiceState.Channel == null)
				await ctx.RespondAsync($"voice state is not null, channel is null");
			else
				await ctx.RespondAsync($"connected to channel {ctx.Member.VoiceState.Channel.Name}");
		}

		[Command("testpoll")]
		public async Task TestPollAsync(CommandContext ctx, [RemainingText]string question)
		{
			var intr = ctx.Client.GetInteractivity();
			var m = await ctx.RespondAsync(question);
			ctx.Client.DebugLogger.LogMessage(LogLevel.Debug, "interactivity-test", "sent message & got interactivity ext", DateTime.Now);
			List<DiscordEmoji> ems = new List<DiscordEmoji>();
			ems.Add(DiscordEmoji.FromUnicode(ctx.Client, "👍"));
			ems.Add(DiscordEmoji.FromUnicode(ctx.Client, "👎"));
			ctx.Client.DebugLogger.LogMessage(LogLevel.Debug, "interactivity-test", "added reactions", DateTime.Now);
			var rcc = await intr.CreatePollAsync(m, ems, TimeSpan.FromSeconds(10));
			ctx.Client.DebugLogger.LogMessage(LogLevel.Debug, "interactivity-test", "got results", DateTime.Now);
			string results = "";
			foreach (var smth in rcc.Reactions)
			{
				results += $"{smth.Key.ToString()}: {smth.Value}\n";
			}
			await m.DeleteAllReactionsAsync();
			await m.ModifyAsync(results);
			ctx.Client.DebugLogger.LogMessage(LogLevel.Debug, "interactivity-test", "sent results", DateTime.Now);
		}

		[Command("testmodify"), RequireOwner]
		public async Task TestModifyAsync(CommandContext ctx, DiscordMember m)
		{
			await ctx.Channel.ModifyAsync(x =>
			{
				x.Name = "poopies_and_peepees";
				x.Topic = "Childish stuff";
				x.AuditLogReason = "Just because..";
			});

			await ctx.Guild.ModifyAsync(x =>
			{
				x.Name = "House of memes";
				x.AuditLogReason = "This is our name now.";
			});

			await m.ModifyAsync(x =>
			{
				x.Nickname = "Lord of the memes";
				x.AuditLogReason = "He owns u nao";
			});

			await ctx.RespondAsync($"You are now the lord of memes, {m.Mention}. Here in the house of memes. In the channel of poopies and peepees.");
		}

		[Command("setprefix"), Aliases("channelprefix"), Description("Sets custom command prefix for current channel. The bot will still respond to the default one."), RequireOwner]
		public async Task SetPrefixAsync(CommandContext ctx, [Description("The prefix to use for current channel.")] string prefix = null)
		{
			if (string.IsNullOrWhiteSpace(prefix))
				if (PrefixSettings.TryRemove(ctx.Channel.Id, out _))
					await ctx.RespondAsync("👍").ConfigureAwait(false);
				else
					await ctx.RespondAsync("👎").ConfigureAwait(false);
			else
			{
				PrefixSettings.AddOrUpdate(ctx.Channel.Id, prefix, (k, vold) => prefix);
				await ctx.RespondAsync("👍").ConfigureAwait(false);
			}
		}

		[Command("sudo"), Description("Run a command as another user."), Hidden, RequireOwner]
		public async Task SudoAsync(CommandContext ctx, DiscordUser user, [RemainingText] string content)
		{
            //await ctx.Client.GetCommandsNext().SudoAsync(user, ctx.Channel, content).ConfigureAwait(false);

            var cmd = ctx.CommandsNext.FindCommand(content, out var args);
            var fctx = ctx.CommandsNext.CreateFakeContext(user, ctx.Channel, content, ctx.Prefix, cmd, args);
            await ctx.CommandsNext.ExecuteCommandAsync(fctx).ConfigureAwait(false);
		}

        [Command("whoami"), Description("Displays information about the user running this command.")]
        public Task WhoAmIAsync(CommandContext ctx)
            => ctx.RespondAsync($"{ctx.User.Id} / {ctx.User.Username} / {ctx.User.Discriminator} / {ctx.User.GetAvatarUrl(ImageFormat.WebP, 1024)}");

		[Group("bind"), Description("Various argument binder testing commands.")]
		public class Binding : BaseCommandModule
		{
			[Command("user"), Description("Attempts to get a user.")]
			public Task UserAsync(CommandContext ctx, DiscordUser usr = null)
				=> ctx.RespondAsync(embed: new DiscordEmbedBuilder().WithDescription(usr?.Mention ?? "<null>"));

			[Command("member"), Description("Attempts to get a member.")]
			public Task MemberAsync(CommandContext ctx, DiscordMember mbr = null)
				=> ctx.RespondAsync(embed: new DiscordEmbedBuilder().WithDescription(mbr?.Mention ?? "<null>"));

			[Command("role"), Description("Attempts to get a role.")]
			public Task RoleAsync(CommandContext ctx, DiscordRole rol = null)
				=> ctx.RespondAsync(embed: new DiscordEmbedBuilder().WithDescription(rol?.Mention ?? "<null>"));

			[Command("channel"), Description("Attempts to get a channel.")]
			public Task ChannelAsync(CommandContext ctx, DiscordChannel chn = null)
				=> ctx.RespondAsync(embed: new DiscordEmbedBuilder().WithDescription(chn?.Mention ?? "<null>"));

			[Command("guild"), Description("Attempts to get a guild.")]
			public Task GuildAsync(CommandContext ctx, DiscordGuild gld = null)
				=> ctx.RespondAsync(embed: new DiscordEmbedBuilder().WithDescription(gld?.Name ?? "<null>"));

			[Command("emote"), Description("Attempts to get an emoji.")]
			public Task EmoteAsync(CommandContext ctx, DiscordEmoji emt = null)
				=> ctx.RespondAsync(embed: new DiscordEmbedBuilder().WithDescription(emt?.ToString() ?? "<null>"));

			[Command("string"), Description("Attempts to bind a string.")]
			public Task StringAsync(CommandContext ctx, string s = null)
				=> ctx.RespondAsync(s ?? "<null>");

			[Command("bool"), Description("Attempts to bind a boolean.")]
			public Task BoolAsync(CommandContext ctx, bool b)
				=> ctx.RespondAsync($"{b}");

			[Command("nullable"), Description("Attempts to bind a nullable integer.")]
			public Task NullableAsync(CommandContext ctx, int? x = 4)
				=> ctx.RespondAsync(x?.ToString("#,##0") ?? "<null>");

			//[Command("enum"), Description("Attempts to bind an enum value.")]
			//public Task EnumAsync(CommandContext ctx, TestEnum? te = null)
			//    => ctx.RespondAsync(te?.ToString() ?? "<null>");

			//public enum TestEnum
			//{
			//    String,
			//    Integer
			//}
		}

		[Group]
		public class ImplicitGroup : BaseCommandModule
		{
			[Command]
			public Task ImplicitAsync(CommandContext ctx)
				=> ctx.RespondAsync("Hello from trimmed name!");

			[Command]
			public Task Another(CommandContext ctx)
				=> ctx.RespondAsync("Hello from untrimmed name!");
		}

		[Group]
		public class Prefixes : BaseCommandModule
		{
			[Command, RequirePrefixes("<<", ShowInHelp = true)]
			public Task PrefixShown(CommandContext ctx)
				=> ctx.RespondAsync("Hello from shown prefix.");

			[Command, RequirePrefixes("<<", ShowInHelp = false)]
			public Task PrefixHidden(CommandContext ctx)
				=> ctx.RespondAsync("Hello from hidden prefix.");
		}

		// this is a mention of _moonPtr#8058 (276460831187664897)
		// I don't hate you, in fact I appreciate you breaking this stuff
		// but revenge is revenge
		// nothing personnel kid 😎
		[Group("<@!276460831187664897>"), Aliases("<@276460831187664897>"), Description("That's what you get for breaking my christian lib.")]
		public class Moon : BaseCommandModule
		{
			[Command("test1")]
			public Task WhatTheHeck(CommandContext ctx)
				=> ctx.RespondAsync("wewlad 0");

			[Command("test2")]
			public Task StopBreakingMyStuff(CommandContext ctx)
				=> ctx.RespondAsync("wewlad 1");
		}

		[Group("conflict")]
		public class Conflict1 : BaseCommandModule
		{
			[Command]
			public Task Command1(CommandContext ctx)
				=> ctx.RespondAsync("If you can see this, something went terribly wrong (1).");
		}

		//[Group("conflict")]
		//public class Conflict2 : BaseCommandModule
		//{
		//    [Command]
		//    public Task Command1(CommandContext ctx)
		//        => ctx.RespondAsync("If you can see this, something went terribly wrong (1).");
		//}

		[Group]
		public class Nesting1 : BaseCommandModule
		{
			[Group]
			public class Nesting2
			{
				[Group]
				public class Nesting3
				{
					[Command]
					public Task NestingAsync(CommandContext ctx)
						=> ctx.RespondAsync("Hello from nested crap.");
				}
			}
		}

		[Group]
		public class Executable1 : BaseCommandModule
		{
			public TestBotService Service { get; }

			public Executable1(TestBotService srv)
			{
				this.Service = srv;
			}

			[GroupCommand, Priority(10)]
			public Task ExecuteGroupAsync(CommandContext ctx)
			{
				this.Service.InrementUseCount();
				return ctx.RespondAsync("Incremented by 1.");
			}

			[GroupCommand, Priority(5)]
			public Task ExecuteGroupAsync(CommandContext ctx, int arg)
			{
				if (arg > 0)
				{
					for (var i = 0; i < arg; i++)
						this.Service.InrementUseCount();

					return ctx.RespondAsync($"Incremented by {arg} (int).");
				}
				else
				{
					return ctx.RespondAsync("Not incremented (int).");
				}
			}

			[GroupCommand, Priority(0)]
			public Task ExecuteGroupAsync(CommandContext ctx, [RemainingText] string arg)
			{
				if (arg != null && arg.Length > 0)
				{
					for (var i = 0; i < arg.Length; i++)
						this.Service.InrementUseCount();

					return ctx.RespondAsync($"Incremented by {arg.Length} (string).");
				}
				else
				{
					return ctx.RespondAsync("Not incremented (string).");
				}
			}

			[Command, Priority(10)]
			public Task TestAsync(CommandContext ctx)
				=> ctx.RespondAsync("Argument-less TEST.");

			[Command, Priority(0)]
			public Task TestAsync(CommandContext ctx, [RemainingText] string text)
				=> ctx.RespondAsync($"Argumented TEST (s): {text}.");

			[Command("test"), Priority(5)]
			public Task NotNameAsync(CommandContext ctx, int i)
				=> ctx.RespondAsync($"Argumented TEST (i): {i}");

			[Command]
			public Task StatusAsync(CommandContext ctx)
				=> ctx.RespondAsync($"Counter: {this.Service.CommandCounter}");
		}

		[Command, Priority(10)]
		public Task OverloadTestAsync(CommandContext ctx)
			=> ctx.RespondAsync("Overload with no args.");

		[Command, Priority(0)]
		public Task OverloadTestAsync(CommandContext ctx, [RemainingText, Description("Catch-all argument.")] string arg)
			=> ctx.RespondAsync($"Overload with catch-all string: {arg}.");

		[Command, Priority(5)]
		public Task OverloadTestAsync(CommandContext ctx, [Description("An integer.")] int arg)
			=> ctx.RespondAsync($"Overload with int: {arg}");

		[Command]
		public Task EmojiTest(CommandContext ctx, params DiscordEmoji[] args)
		{
			var sb = new StringBuilder();
			foreach (var arg in args)
			{
				sb.Append($"Name: {arg.Name} | Id: {arg.Id} | Animated: {arg.IsAnimated}\n");
			}

			return ctx.RespondAsync(sb.ToString().Trim());
		}

		// close your eyes, there are disasters ahead
		[Group, ModuleLifespan(ModuleLifespan.Transient), TestBotCheck]
		public class Transient : BaseCommandModule
		{
			[Command]
			public unsafe Task GetPtr(CommandContext ctx)
			{
				var x = this;
				var r = __makeref(x);
				var ptr = **(IntPtr**)(&r);

				return ctx.RespondAsync($"0x{ptr:x16}");
			}

			public override Task BeforeExecutionAsync(CommandContext ctx)
				=> ctx.RespondAsync("before execute");

			public override Task AfterExecutionAsync(CommandContext ctx)
				=> ctx.RespondAsync("after execute");
		}

		[Group, ModuleLifespan(ModuleLifespan.Singleton)]
		public class Singleton : BaseCommandModule
		{
			[Command]
			public unsafe Task GetPtr(CommandContext ctx)
			{
				var x = this;
				var r = __makeref(x);
				var ptr = **(IntPtr**)(&r);

				return ctx.RespondAsync($"0x{ptr:x16}");
			}

			public override Task BeforeExecutionAsync(CommandContext ctx)
				=> ctx.RespondAsync("before execute");

			public override Task AfterExecutionAsync(CommandContext ctx)
				=> ctx.RespondAsync("after execute");
		}
		// ok you can open your eyes again

		[Group, ModuleLifespan(ModuleLifespan.Transient)]
		public class Scoped : BaseCommandModule
		{
			public TestBotScopedService Service { get; }

			public Scoped(TestBotScopedService srv)
			{
				this.Service = srv;
			}

			[Command]
			public Task GetPtr(CommandContext ctx)
			{
				var ptr0 = this.Service.GetPtr();
				var ptr1 = ctx.Services.GetRequiredService<TestBotScopedService>().GetPtr();
				var ptr2 = ctx.Services.GetRequiredService<TestBotScopedService>().GetPtr();

				return ctx.RespondAsync($"{ptr0} | {ptr1} | {ptr2}");
			}
		}

		[Group, Test]
		public class CustomAttributes : BaseCommandModule
		{
			[GroupCommand]
			public Task ListAttributes(CommandContext ctx)
			{
				return ctx.RespondAsync(string.Join(" ", ctx.Command.CustomAttributes.Select(x => Formatter.InlineCode(x.GetType().ToString()))));
			}
		}

		[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
		public class TestAttribute : Attribute
		{
			public TestAttribute() { }
		}
	}

    [DebugCheck]
    public class CheckTestModule : BaseCommandModule
    {
        [Command]
        public Task DebugCommandAsync(CommandContext ctx, [RemainingText] string arg)
            => ctx.RespondAsync(arg);

        [Command, Priority(1)]
        public Task DebugCommandAsync(CommandContext ctx, int arg)
            => ctx.RespondAsync(arg.ToString("#,##0"));
    }

    public class TestBotService
	{
		public int CommandCounter => this._cmd_counter;
		private volatile int _cmd_counter;

		public TestBotService()
		{
			this._cmd_counter = 0;
		}

		public void InrementUseCount()
		{
			Interlocked.Increment(ref this._cmd_counter);
		}
	}

	public class TestBotScopedService
	{
		public unsafe string GetPtr()
		{
			var x = this;
			var r = __makeref(x);
			var ptr = **(IntPtr**)(&r);

			return $"0x{ptr:x16}";
		}
	}

    public class DebugCheckAttribute : CheckBaseAttribute
    {
        public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
        {
            ctx.Client.DebugLogger.LogMessage(LogLevel.Debug, "Test-Check", "Testing check", DateTime.Now);
            return Task.FromResult(true);
        }
    }
}
